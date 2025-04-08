import pandas as pd
import numpy as np

import requests as rq
from io import BytesIO

import mysql.connector

from tqdm import tqdm
import time

import torch
import torch.nn as nn
from torch.optim.adam import Adam
from torch.utils.data.dataloader import DataLoader
from torch.utils.data.dataset import Dataset
import socket
import struct
from datetime import datetime
from dateutil.relativedelta import relativedelta





class ECO(Dataset):
    def __init__(self, values):
       self.csv = values

       # 입력 데이터 정규화
       self.data = self.csv   # 종가 데이터
       self.data = self.data / np.max(self.data)  # 0과 1 사이로 정규화

       # 종가 데이터 정규화
       self.label = self.csv       
       self.label = self.label / np.max(self.label)

    def __len__(self):
       return len(self.data) - 30
    
    def __getitem__(self, i):
       data = self.data[i:i+30]
       label = self.label[i+30]

       return data, label
    




class RNN(nn.Module):
    def __init__(self):
        super(RNN, self).__init__()

        # RNN층의 정의
        self.rnn = nn.RNN(input_size=1, hidden_size=8, num_layers=5,
                            batch_first=True)

        # 주가를 예측하는 MLP층 정의
        self.fc1 = nn.Linear(in_features=240, out_features=64)
        self.fc2 = nn.Linear(in_features=64, out_features=1)

        self.relu = nn.Tanh() # 활성화 함수 정의
        
    def forward(self, x, h0):
        x, hn = self.rnn(x, h0)  # RNN층의 출력

        # MLP층의 입력으로 사용되게 모양 변경
        x = torch.reshape(x, (x.shape[0], -1))

        # MLP층을 이용해 종가 예측
        x = self.fc1(x)
        x = self.relu(x)
        x = self.fc2(x)

        # 예측한 종가를 1차원 벡터로 표현
        x = torch.flatten(x)

        return x
    
# 소켓 통신

def handle_client(conn, addr):
    try:
        print(f"클라이언트 연결됨: {addr}")

        # 유니티에서 보낸 문자열 길이(4바이트) 수신
        data_length = struct.unpack("!I", conn.recv(4))[0]

        # 해당 길이만큼 문자열 데이터 수신
        received_string = conn.recv(data_length).decode()
        print(f"유니티로부터 받은 문자열: {received_string}")

        if received_string:
            # 종목 코드 추출 및 데이터 준비
            sectors = pd.read_csv("/home/john/rnn_model/std_name.csv") # ssh에 sectors 파일 업로드

            name_df = sectors[sectors["std"] == f"{received_string}"]
            name = name_df.iloc[0, 2]


            aws = mysql.connector.connect(
                host = "database-1.c3ykuqosgy9m.ap-northeast-2.rds.amazonaws.com",
                port = 3306,
                user = "root",
                password = "kim4582345",
                database = "joseon_ameba"
                )
        
            cur = aws.cursor(buffered=True)
            
            sql = f"select closing_price from stock_price_per_date where std_code = '{received_string}'"
            cur.execute(sql)

            result = cur.fetchall()

            values = [x[0] for x in result]


            device = "cuda" if torch.cuda.is_available() else "cpu"
            model = RNN().to(device)  # 모델의 정의
            dataset = ECO(values)  # 데이터셋의 정의, ECO 클래스 객체 생성 시 values를 전달

            loader = DataLoader(dataset, batch_size=1, num_workers=0, pin_memory=True)

            preds = []
            data_seq = dataset.data[-30:].reshape(1, -1, 1)  # 최근 30일 데이터를 초기 입력으로 사용
            data_seq = torch.tensor(data_seq, dtype=torch.float32).to(device)

            with torch.no_grad():
                model.load_state_dict(torch.load(f"/home/john/rnn_model/model_for_each/{name}_rnn.pth", map_location=device))
                model.eval()

                h0 = torch.zeros(5, 1, 8).to(device)

                for j in range(30):
                    pred = model(data_seq, h0)
                    noise = torch.tensor(np.random.normal(0, 0.01), dtype=torch.float32).to(device)
                    pred = pred + noise
                    preds.append(pred.item())

                    new_data = torch.tensor([[pred.item()]], dtype=torch.float32).to(device)
                    data_seq = torch.cat((data_seq[:, 1:, :], new_data.unsqueeze(1)), dim=1)

            # 예측치 표준화
            standardized = np.array(preds) * np.max(values)

            # 30일 예측값의 표준편차 및 평균 계산
            std_dev = np.std(preds)
            mean_price = np.mean(preds)

            # 리스크 지수 계산
            risk_index = std_dev / mean_price

            # 리스크 구간 정의
            if risk_index > 0.05:
                risk_level = "High Risk"
            elif risk_index > 0.03:
                risk_level = "Medium Risk"
            else:
                risk_level = "Low Risk"

            # 데이터 직렬화 및 송신
            fmt = f"!I{len(standardized)}fI{len(risk_level)}s"
            data = struct.pack(fmt, len(standardized), *standardized, len(risk_level), risk_level.encode())

            conn.send(data)
            print("데이터 전송 완료!")

        aws.close()
        conn.close()
    except Exception as e:
        print(f"클라이언트 처리 중 오류 발생: {e}")
        conn.close()

def start_server():
    # TCP 서버 설정
    HOST = ''  # 로컬호스트
    PORT = 8080  # 포트 번호

    # 소켓 생성 및 바인딩
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((HOST, PORT))
    server_socket.listen(1)
    print(f"서버 실행 중... {HOST}:{PORT}")

    while True:
        try:
            # 클라이언트 연결 대기
            conn, addr = server_socket.accept()
            print(f"새로운 클라이언트 연결됨: {addr}")

            # 유니티에서 데이터를 받을 때까지 대기
            handle_client(conn, addr)

        except Exception as e:
            print(f"서버 오류 발생: {e}")
            break

    server_socket.close()

if __name__ == "__main__":
    start_server()



