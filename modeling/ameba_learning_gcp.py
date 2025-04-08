import pandas as pd
import numpy as np
import requests as rq
from io import BytesIO

import mysql.connector

from tqdm import tqdm
from datetime import datetime

import torch
import torch.nn as nn
from torch.optim.adam import Adam
from torch.utils.data.dataloader import DataLoader
from torch.utils.data.dataset import Dataset

import time


class ECO(Dataset):
    def __init__(self):
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
    

#종목코드 추출
sectors = pd.read_csv("/home/john/rnn_model/sectors.csv") # ssh에 sectors 파일 업로드

std = []
name = []

for i in range(len(sectors)):
   std.append(sectors.iloc[i, 1])
   name.append(sectors.iloc[i, 4])


aws = mysql.connector.connect(
        host = "database-1.c3ykuqosgy9m.ap-northeast-2.rds.amazonaws.com",
        port = 3306,
        user = "root",
        password = "kim4582345",
        database = "joseon_ameba"
        )
    
cur = aws.cursor(buffered=True)

# create a loop to prepare data for modeling


for i in tqdm(range(len(std))):
    try:
        sql = f"select closing_price from stock_price_per_date where std_code = '{std[i]}'"
        cur.execute(sql)

        result = cur.fetchall()

        values = [x[0] for x in result]
        
        time.sleep(0.5)


        device = "cuda" if torch.cuda.is_available() else "cpu" # allow for gpu calculation

        model = RNN().to(device)  # 모델의 정의 
        model.train()

        dataset = ECO()       # 데이터셋의 정의

        loader = DataLoader(dataset, batch_size=32, num_workers=0, pin_memory=True)  # 배치 크기를 32로 설정
        optim = Adam(params=model.parameters(), lr=0.0001) # 사용할 최적화 설정




        for epoch in range(100):
            time.sleep(0.2)
            for data, label in loader:
                data = data.type(torch.float32).to(device)
                label = label.type(torch.float32).to(device)
    
                # 초기 은닉 상태
                h0 = torch.zeros(5, data.size(0), 8).to(device)
    
                if data.dim() == 2:  # (batch_size, seq_len)
                        data = data.unsqueeze(-1)
    
                optim.zero_grad()
        
                # 모델의 예측값
                pred = model(data, h0)
    
                # 손실의 계산
                loss = nn.MSELoss()(pred, label)
                loss.backward()  # 오차 역전파
                optim.step()     # 최적화 진행

        torch.save(model.state_dict(), f"/home/john/rnn_model/model_for_each/{name[i]}_rnn.pth")  # 모델 저장

        time.sleep(0.5)
    except Exception as e:
        print(f"오류 발생: {e}, {name[i]} 학습을 건너뜁니다.")
        continue  # 오류 발생 시 다음 루프로 넘어감


aws.close()




