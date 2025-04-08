using UnityEngine;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class dbManager : MonoBehaviour
{
    private static ThreadLocal<MySqlConnection> connection = new ThreadLocal<MySqlConnection>(); // SQL Connection

    private static string sql_connection = null; //SQL 접속 명령인자

    static dbManager()
    {
        // 접속 명령인자 생성
        StringBuilder builder = new StringBuilder();

        builder.AppendFormat(
            "Server={0};", "database-1.c3ykuqosgy9m.ap-northeast-2.rds.amazonaws.com"); // DB가 설치된 ID
        builder.AppendFormat("Port={0};", "3306"); // DB의 포트 번호
        builder.AppendFormat("Database={0};", "joseon_ameba"); // 데이터베이스 이름
        builder.AppendFormat("Uid={0};", "taeho"); // 사용자 이름
        builder.AppendFormat("Pwd={0}", ""); // 사용자의 비밀번호

        sql_connection = builder.ToString();
    }

    private static void db_connect()
    {
        try
        {
            connection.Value = new MySqlConnection(sql_connection);
            connection.Value.Open();

            if (connection.Value.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection failed to open.");
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private static void db_disconnect()
    {
        if (connection.Value != null && connection.Value.State != ConnectionState.Closed)
        {
            connection.Value.Close();
            connection.Value = null;
        }
    }

    public static MySqlErrorCode insert(string table_name, string columns, string data)
    {
        Debug.Log($"테이블:{table_name}, 열:{columns}, 데이터:{data}");
        db_connect(); // 접속

        try
        {
            string cmd = $"INSERT INTO {table_name} ({columns}) VALUES ({data});";
            Debug.Log(cmd);
            MySqlCommand db_cmd = new MySqlCommand(cmd, connection.Value); // 명령어를 커맨드에 입력
            db_cmd.ExecuteNonQuery(); // 명령어를 SQL에 보냄

            return MySqlErrorCode.None;
        }
        catch (MySqlException e) // SQL 오류
        {
            Debug.Log(e.ToString());
            return e.ErrorCode;
        }
        finally
        {
            db_disconnect(); // 접속 해제
        }
    }

    public static MySqlErrorCode delete(string table_name, string cond)
    {
        Debug.Log($"테이블:{table_name}");
        db_connect(); // 접속

        try
        {
            string cmd = $"DELETE FROM {table_name} WHERE {cond};";
            Debug.Log(cmd);
            MySqlCommand db_cmd = new MySqlCommand(cmd, connection.Value); // 명령어를 커맨드에 입력
            db_cmd.ExecuteNonQuery(); // 명령어를 SQL에 보냄

            return MySqlErrorCode.None;
        }
        catch (MySqlException e) // SQL 오류
        {
            Debug.Log(e.ToString());
            return e.ErrorCode;
        }
        finally
        {
            db_disconnect(); // 접속 해제
        }
    }

    public static DataTableReader select(string table_name, string data, string cond = null, string join = null, string join_cond = null)
    {
        Debug.Log($"{table_name}");
        db_connect(); // 접속

        try
        {
            var cmd = "";
            if ((cond == null) && (join == null) && (join_cond == null))
            {
                cmd += $"SELECT {data} FROM {table_name};";
            }
            else if ((join == null) && (join_cond == null))
            {
                cmd += $"SELECT {data} FROM {table_name} WHERE {cond};";
            }
            else
            {
                cmd += $"SELECT {data} FROM {table_name} JOIN {join} ON {join_cond} WHERE {cond};";
            }

            Debug.Log(cmd);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd, connection.Value);
            DataTable table = new DataTable(); // 테이블 생성
            adapter.Fill(table); // 데이터 테이블 채우기
            if (table != null && table.Rows.Count > 0)
            {
                return table.CreateDataReader(); // 성공적으로 select를 했다면, 데이터 리더를 생성
            }
            else
            {
                Debug.Log("테이블이 없읍니다.");
                return null;
            }
        }
        catch (MySqlException e) // SQL 오류 발생
        {
            Debug.Log(e.ToString());
            return null;
        }
        finally
        {
            db_disconnect(); // 접속 해제
        }
    }
}
