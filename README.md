# eda-repo-6 조선아메바(국장의 미래)
<b>주가 정보 조회 및 예측 서비스</b>

![intro_image](https://github.com/user-attachments/assets/6b98b5ec-f2a2-4fef-8996-a3065e7e90e4)

## 1. 제작 배경
- 어떤 사람은 국장의 미래를 낙관적으로 보고, 어떤 사람은 비관적으로 본다
- 국장의 미래를 파악해보기 위해 예측 모델을 사용하여 각 종목의 주가를 예측해보고자 한다

## 2. 팀원 소개
![team_image](https://github.com/user-attachments/assets/1bcc9091-9fc1-45dd-9f69-7cc687f156c0)

## 3. 서비스 유형
### 3.1 보유 종목 정보 조회
![info_image](https://github.com/user-attachments/assets/01754d4c-73e0-42ae-b393-0c7a27a99338)

https://github.com/user-attachments/assets/9f0c9e66-f191-4c06-b0c4-78efe71e3491
- 본인이 소유한 종목 추가 및 삭제 가능
- 본인이 소유한 종목의 정보(종목명, 현재가, 매수가, 수익률) 조회

### 3.2 주가 예측
<p align="center">  
  <img src="https://github.com/user-attachments/assets/13ad1195-8ffe-40c3-8592-1e8fed55d5b3" align="center" width="49%">  
  <img src="https://github.com/user-attachments/assets/95ff8557-dd54-4155-ae58-f6e24bb63a92" align="center" width="49%">
</p>

https://github.com/user-attachments/assets/71178f32-a5ea-4089-973e-ece8e6521460

- 본인이 원하는 종목 검색 -> 지난 30일치 데이터, 향후 30일치 예측치 조회
- 현재 정보(현재가, 기간내 최고,최저가 등)과 30일 뒤 예측가, 리스크 지수 조회

## 4. 사용 모델 및 서빙 방식
- RNN 기반의 주가 예측 모델을 활용, 지난 주가 데이터를 기반으로 향후 30일치 예측치 산출
![rnn_image](https://github.com/user-attachments/assets/29bb2edc-4b2b-46ce-83a2-73dcc26d42d0)

- 모델의 효율적인 사용을 위해 별도의 서버 운용, 서버에서 모델을 돌리고 예측치를 서비스에 전송
![server_image](https://github.com/user-attachments/assets/e0c3ac45-3a7f-4178-81e0-0db5dfed79e4)

## 5. 서비스 운영 계획
- 서버 운영 기간(예상): ~2025/06/25
- DB 운영 기간(예상): ~2025/06/25
