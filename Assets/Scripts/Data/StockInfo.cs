namespace stockinfo
{
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;
using System.Globalization;
using stockdetail;
using UnityEngine;

public class StockInfo
{
    private List<StockDetail> stock_data_arr;
    private string strtDd;
    private string endDd;
    private string risk;
    private Tuple<float[], string> pred_risk;

    public StockInfo(string start, string end)
    {
        stock_data_arr = new List<StockDetail>();
        strtDd = start;
        endDd = end;
    }

    public DataTable ConvertCsvToTable(string csvData)
    {
        DataTable dataTable = new DataTable();
        using (var reader = new StringReader(csvData))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,   // 첫 줄을 헤더로 인식
            IgnoreBlankLines = true,  // 빈 줄 무시
            BadDataFound = null       // 잘못된 데이터 무시
        }))
        {
            using (var dr = new CsvDataReader(csv))
            {
                dataTable.Load(dr);
            }
            return dataTable;
        }
    }
    
    public async Task<List<StockDetail>> get_stock_info(string stock_name)
    {
        // EUC-KR 인코딩을 사용하기 전에 인코딩 제공자를 등록
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        // 그 후에 EUC-KR 인코딩 사용
        var encoding = Encoding.GetEncoding("euc-kr");
        
        // 종목 표준코드, 약식코드 검색
        string std_code = "", abbr = "";
        using (var stock_reader = dbManager.select("stock", "*"))
        {
            if (stock_reader == null || !stock_reader.HasRows)  // 데이터가 없는 경우 체크
            {
                return null;
            }
            else
            {
                while (stock_reader.Read())
                {
                    if (stock_reader["stock_name"].ToString() == stock_name)
                    {
                        std_code = (string)stock_reader["std_code"];
                        abbr = (string)stock_reader["abbr"];
                        break;  // 조건에 맞는 첫 번째 행만 찾으면 종료
                    }
                }
            }
        }
        Debug.Log(std_code);
        Debug.Log(abbr);

        /*
        // 종목 주가 정보 수집
        using (var price_reader = dbManager.select(
            "stock_price_per_date p", "*", $"s.stock_name='{stock_name}' AND (p.day BETWEEN '{strtDd}' AND '{endDd}')", "stock s", "p.std_code=s.std_code"))
        {
            if (price_reader == null || !price_reader.HasRows)  // 데이터가 없는 경우 체크
            {
                DateTime today = DateTime.Now.Date;
                DateTime startDate = DateTime.ParseExact(strtDd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(endDd, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                
                if (startDate == today && endDate == today)
                {
                    // 날짜가 오늘이면 현재 가격 수집을 계속 진행
                    Debug.Log("Today!");
                }
                else
                {
                    return null;
                }
            }
            else
            {
                while (price_reader.Read())
                {
                    DateTime date = Convert.ToDateTime(price_reader["day"]);
                    stock_data_arr.Add(new StockDetail(
                        stock_name, std_code, 
                        price_reader["day"].ToString(), 
                        Convert.ToInt32(price_reader["closing_price"])));
                }
            }
        }
        */

        // 종목 주가 정보 수집
        // generate 헤더 요청 URL
        string url_price = "http://data.krx.co.kr/comm/fileDn/GenerateOTP/generate.cmd";

        // generate 페이로드의 양식 데이터
        var data_price = new Dictionary<string, string>
        {
            { "locale", "ko_KR" },
            { "tboxisuCd_finder_stkisu0_1", $"{abbr}/{stock_name}" },
            { "isuCd", std_code },
            { "isuCd2", "KR7005930003" },
            { "codeNmisuCd_finder_stkisu0_1", stock_name },
            { "param1isuCd_finder_stkisu0_1", "ALL"},
            { "strtDd", strtDd },
            { "endDd", endDd },
            { "adjStkPrc_check", "Y"},
            { "adjStkPrc", "2" },
            { "share", "1" },
            { "money", "1" },
            { "csvxls_isNo", "false" },
            { "name", "fileDown" },
            { "url", "dbms/MDC/STAT/standard/MDCSTAT01701" }
        };

        // 브라우저에서 서버로 보내는 헤더값
        var headers = new Dictionary<string, string>
        {
            { "Referer", "http://data.krx.co.kr/contents/MDC/MDI/mdiLoader/index.cmd?menuId=MDC0201020203" },
            { "User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36" }
        };

        string indv_data;
        using (var client = new HttpClient())
        {
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            // generate 부분의 헤더에서 Referer과 User-Agent를 따올 수 있음 User-Agent는 모두 동일
            // KRX의 다른 정보를 따올 때는 Referer만 바꿔주기
            // download payload(요청데이터)와 동일해야함
            var content_indv = new FormUrlEncodedContent(data_price);
            var response_indv = await client.PostAsync(url_price, content_indv);
            var otp_indv = await response_indv.Content.ReadAsStringAsync();

            // download의 헤더 요청 URL
            string downUrl_indv = "http://data.krx.co.kr/comm/fileDn/download_csv/download.cmd";

            // 서버로부터 데이터 요청 후 읽어들이기
            var downloadData_indv = new Dictionary<string, string>
            {
                { "code", otp_indv }
            };
            var downloadContent_indv = new FormUrlEncodedContent(downloadData_indv);
            var downSectorResponse_indv = await client.PostAsync(downUrl_indv, downloadContent_indv);
            var stream_indv = await downSectorResponse_indv.Content.ReadAsStreamAsync();

            using (var reader = new StreamReader(stream_indv, encoding))
            {
                indv_data = reader.ReadToEnd();
            }
        }

        DataTable indv_table = ConvertCsvToTable(indv_data);

        // 디버깅을 위해 테이블 정보 출력
        Debug.Log($"테이블 행 수: {indv_table.Rows.Count}");
        foreach (DataColumn column in indv_table.Columns)
        {
            Debug.Log($"열 이름: {column.ColumnName}");
        }
            
        // 테이블의 모든 행을 리스트로 변환
        var rows = new List<DataRow>();
        foreach (DataRow row in indv_table.Rows)
        {
            rows.Add(row);
        }
            
        // 리스트를 역순으로 순회
        for (int i = rows.Count - 1; i >= 0; i--)
        {
            try 
            {
                stock_data_arr.Add(new StockDetail(
                    stock_name, std_code, Convert.ToString(rows[i]["일자"]), Convert.ToInt32(rows[i]["종가"]), abbr, 
                    Convert.ToInt32(rows[i]["대비"]), Convert.ToSingle(rows[i]["등락률"]),
                    Convert.ToInt32(rows[i]["시가"]), Convert.ToInt32(rows[i]["고가"]),
                    Convert.ToInt32(rows[i]["저가"]), Convert.ToDouble(rows[i]["거래량"]),
                    Convert.ToInt64(rows[i]["거래대금"]), Convert.ToInt64(rows[i]["시가총액"]),
                    Convert.ToInt64(rows[i]["상장주식수"])));
            }
            catch (Exception ex)
            {
                Debug.LogError($"데이터 처리 중 오류 발생: {ex.Message}");
            }
        }
        return stock_data_arr;
    }
    
    public void predict_stock_info(string std_code, Action<Tuple<float[], string>> callback)
    {
        MonoBehaviour mono = GameObject.FindFirstObjectByType<MonoBehaviour>();
        mono.StartCoroutine(tcpManager.CommunicateWithServerCoroutine(std_code, result => {
            pred_risk = result;
            callback(result);
        }));
    }
}
}