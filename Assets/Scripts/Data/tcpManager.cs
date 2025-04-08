using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tcpManager : MonoBehaviour
{
    private const string serverIP = "34.22.64.103";
    private const int serverPort = 8080;
    private static tcpManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static IEnumerator CommunicateWithServerCoroutine(string message, Action<Tuple<float[], string>> callback)
    {
        Tuple<float[], string> result = null;
        TcpClient client = null;
        NetworkStream stream = null;
        BinaryWriter writer = null;
        BinaryReader reader = null;

        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);
        }
        catch (Exception e)
        {
            Debug.LogError("연결 실패: " + e.Message);
            callback(null);
            yield break;
        }

        yield return new WaitForEndOfFrame();

        // 데이터 송수신 로직
        try 
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            writer.Write(IPAddress.HostToNetworkOrder(messageBytes.Length));
            writer.Write(messageBytes);
            writer.Flush();
            Debug.Log("서버로 문자열 전송 완료: " + message);

            // 리스트 길이 읽기 (4바이트)
            int listLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());

            // float 리스트 읽기
            float[] predList = new float[listLength];
            for (int i = 0; i < listLength; i++)
            {
                predList[i] = BitConverter.ToSingle(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(reader.ReadInt32())), 0);
            }

            // 문자열 길이 읽기 (4바이트)
            int riskLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());

            // 문자열 데이터 읽기
            string risk_level = Encoding.UTF8.GetString(reader.ReadBytes(riskLength));

            Debug.Log("서버로부터 받은 데이터: " + string.Join(", ", predList) + risk_level);
            result = new Tuple<float[], string>(predList, risk_level);
        }
        catch (Exception e)
        {
            Debug.LogError("데이터 송수신 실패: " + e.Message);
            result = null;
        }
        finally
        {
            writer?.Close();
            reader?.Close();
            stream?.Close();
            client?.Close();
        }

        callback(result);
    }
    /*
    IEnumerator WaitForServerResponse(Action<(float[], string)> callback)
    {
        byte[] buffer = new byte[1024];
        int bytesRead = 0;

        while (bytesRead == 0)  // **서버가 데이터를 보낼 때까지 대기**
        {
            if (stream.DataAvailable)
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }
            yield return null;  // **다음 프레임까지 대기**
        }

        // 받은 데이터 파싱
        int offset = 0;

        // 1. float 개수 읽기
        int floatCount = BitConverter.ToInt32(buffer, offset);
        offset += 4;

        // 2. float 리스트 읽기
        float[] pred_list = new float[floatCount];
        for (int i = 0; i < floatCount; i++)
        {
            pred_list[i] = BitConverter.ToSingle(buffer, offset);
            offset += 4;
        }

        // 3. 문자열 길이 읽기
        int stringLength = BitConverter.ToInt32(buffer, offset);
        offset += 4;

        // 4. 문자열 읽기
        string risk = Encoding.UTF8.GetString(buffer, offset, stringLength);

        // 결과 출력
        Debug.Log($"서버에서 받은 float 리스트: {string.Join(", ", pred_list)}");
        Debug.Log($"서버에서 받은 문자열: {risk}");

        callback((pred_list, risk));

        // 연결 종료
        stream.Close();
        client.Close();
        Debug.Log("서버 연결 종료");
    }
    */
}
