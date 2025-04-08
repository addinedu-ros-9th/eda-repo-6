import socket
import struct

# 송신할 정수 리스트
data = []# 이 위치에 예측값 리스트 대체

# TCP 서버 설정
HOST = ''  # 로컬호스트
PORT = 8080        # 포트 번호

# 소켓 생성 및 바인딩
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))
server_socket.listen(1)
print(f"서버 실행 중... {HOST}:{PORT}")

conn, addr = server_socket.accept()
print(f"클라이언트 연결됨: {addr}")

# 유니티에서 보낸 문자열 길이(4바이트) 수신
data_length = struct.unpack("!I", conn.recv(4))[0]

# 해당 길이만큼 문자열 데이터 수신
received_string = conn.recv(data_length).decode()
print(f"유니티로부터 받은 문자열: {received_string}")

# ====== 데이터 처리(RNN) =======

# 데이터 직렬화 (길이 + 데이터)
packed_data = struct.pack(f"!I{len(data)}i", len(data), *data)  # I: Unsigned Int (길이), i: Int

# 데이터 송신
conn.sendall(packed_data)
print("데이터 송신 완료:", data)

conn.close()
server_socket.close()
