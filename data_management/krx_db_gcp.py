import requests as rq
from io import BytesIO # 파일 시스템에 접근하지 않고 메모리 스트림 다루기 위함
import pandas as pd
import datetime
import openpyxl
from openpyxl.utils.dataframe import dataframe_to_rows
import numpy as np
from tqdm import tqdm
from datetime import datetime


# generate 헤더 요청 URL
url = "http://data.krx.co.kr/comm/fileDn/GenerateOTP/generate.cmd"


# generate 페이로드의 양식 데이터
data = {
    "locale": "ko_KR",
    "mktId": "ALL",
    "share": "1",
    "csvxls_isNo": "false",
    "name": "fileDown",
    "url": "dbms/MDC/STAT/standard/MDCSTAT01901",
}


headers = {
    "Referer": "http://data.krx.co.kr/contents/MDC/MDI/mdiLoader/index.cmd?menuId=MDC0201020203",
    "User-Agent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36"
}

otp = rq.post(url=url, data=data, headers=headers).text

# download의 헤더 요청 URL
down_url = "http://data.krx.co.kr/comm/fileDn/download_csv/download.cmd"

# 서버로부터 데이터 요청 후 읽어들이기
down_sector = rq.post(url=down_url, data={"code": otp}, headers=headers)
sectors = pd.read_csv(BytesIO(down_sector.content), encoding="EUC-KR")

std_code = []
abbr = []
stock_name = []
day = []
for i in range(len(sectors)):
    std_code.append(sectors.iloc[i].iloc[0])
    abbr.append(sectors.iloc[i].iloc[1])
    stock_name.append(sectors.iloc[i].iloc[3])
    day.append(sectors.iloc[i].iloc[5])

now = datetime.today().strftime("%Y%m%d")



import mysql.connector

aws = mysql.connector.connect(
        host = "database-1.c3ykuqosgy9m.ap-northeast-2.rds.amazonaws.com",
        port = 3306,
        user = "root",
        password = "kim4582345",
        database = "joseon_ameba"
        )
    
cur = aws.cursor(buffered=True)


url_price = "http://data.krx.co.kr/comm/fileDn/GenerateOTP/generate.cmd"

headers = {
"Referer": "http://data.krx.co.kr/contents/MDC/MDI/mdiLoader/index.cmd?menuId=MDC0201020203",
"User-Agent": "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36"
}


indv_down_url = "http://data.krx.co.kr/comm/fileDn/download_csv/download.cmd"

for i in tqdm(range(len(stock_name))): 
    

    # 각 종목 당일 데이터 수집
    data_price = {
        "locale": "ko_KR",
        "tboxisuCd_finder_stkisu0_1": f"{abbr[i]}/{stock_name[i]}",
        "isuCd": f"{std_code[i]}",
        "isuCd2": "KR7005930003",
        "codeNmisuCd_finder_stkisu0_1": f"{stock_name[i]}",
        "param1isuCd_finder_stkisu0_1": "ALL",
        "strtDd": f"{now}",
        "endDd": f"{now}",
        "adjStkPrc_check": "Y",
        "adjStkPrc": "2",
        "share": "1",
        "money": "1",
        "csvxls_isNo": "false",
        "name": "fileDown",
        "url": "dbms/MDC/STAT/standard/MDCSTAT01701"
    }
    

    otp_indv = rq.post(url_price, data_price, headers=headers).text
    

    # 서버로부터 데이터 요청 후 csv로 읽어들이기
    indv_down = rq.post(url=indv_down_url, data={"code": otp_indv}, headers=headers)
    indv_price = pd.read_csv(BytesIO(indv_down.content), encoding="EUC-KR").sort_values(by="일자", ascending=True)

    # 일자	종가	대비	등락률	시가	고가	저가	거래량	거래대금	시가총액	상장주식수

    
    
    for idx, row in indv_price.iterrows():
        
        try:
            sql = f"""insert into stock_price_per_date values ('{std_code[i]}', '{row.iloc[0]}', '{row.iloc[1]}');"""
            cur.execute(sql)
            aws.commit()
        except Exception as e:
            pass

aws.close()

# # 데이터베이스 연결 함수
# def insert_data():
#     connection = mysql.connector.connect(
#         host='your-database-host',
#         user='your-username',
#         password='your-password',
#         database='your-database'
#     )

#     cursor = connection.cursor()

#     # 예시 데이터
# indv_price를 가져오는 크롤러 위치

#     # 데이터 삽입 SQL 쿼리
#    for idx, row in indv_price.iterrows():
#        sql = f"""insert into stock_price_per_date values ('{std_code[i]}', '{row.iloc[0]}', '{row.iloc[1]}');"""
#        cur.execute(sql)
#        aws.commit()
    
#    cursor.close()
#    connection.close()

# # 매일 자정에 insert_data 함수 실행
# schedule.every().day.at("00:00").do(insert_data)

# # 무한 루프 실행 (서버가 종료되지 않으면 계속 실행)
# while True:
#     schedule.run_pending()
#     time.sleep(60)  # 1분에 한 번씩 실행
