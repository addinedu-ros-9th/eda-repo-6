using UnityEngine;
using System.Collections.Generic;
using stockinfo;
using stockdetail;
using System.Linq;
using TMPro;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Net;

public class StockDataManager : MonoBehaviour
{
    private DrawGraph drawGraph;
    public static event Action<string> OnRiskLevelUpdated;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        drawGraph = FindFirstObjectByType<DrawGraph>();
        GameObject loadResult = GameObject.Find("LoadResult");
        string stock_name = loadResult.GetComponent<LoadResult>().GetStockName();
        loadResult.GetComponent<LoadResult>().DestroySelf();

        // 오늘 날짜 구하기
        System.DateTime today = System.DateTime.Today;
        // 29일 전 날짜 구하기
        System.DateTime startDate = today.AddDays(-29);
        string today_str = today.ToString("yyyyMMdd");
        string startDate_str = startDate.ToString("yyyyMMdd");
        StockInfo stockInfo = new StockInfo(startDate_str, today_str);
        List<StockDetail> stock_data_arr = await stockInfo.get_stock_info(stock_name);
        string std = stock_data_arr[0].std_code;
        float cur_price = Convert.ToSingle(stock_data_arr[stock_data_arr.Count - 1].closing_price);

        // 기본 데이터로 그래프 초기화
        List<float> dataValues = new List<float>();
        List<string> dataLabels = new List<string>();

        foreach (StockDetail stock in stock_data_arr)
        {
            dataValues.Add(stock.closing_price);
            string dateStr = stock.day;
            DateTime date = DateTime.ParseExact(dateStr, "yyyy/MM/dd", null);
            string formattedDate = date.ToString("MM-dd");
            dataLabels.Add(formattedDate);
        }

        // 기본 그래프 표시
        drawGraph.SetStockData(dataValues, dataLabels);

        // 기본 정보 먼저 표시
        GameObject.Find("stock_name_txt").GetComponent<TextMeshProUGUI>().text = stock_name;
        GameObject.Find("cur_price_data").GetComponent<TextMeshProUGUI>().text = 
            $"{stock_data_arr[stock_data_arr.Count - 1].closing_price:N0}원";

        TextMeshProUGUI contrastText = GameObject.Find("cur_price_cont").GetComponent<TextMeshProUGUI>();
        float contrast = stock_data_arr[stock_data_arr.Count - 1].contrast;
        float currentPrice = stock_data_arr[stock_data_arr.Count - 1].closing_price;
        float previousPrice = currentPrice - contrast;
        float percentageChange = (contrast / previousPrice) * 100;
        string sign = contrast >= 0 ? "+" : "";
        contrastText.text = $"{sign}{contrast:N0}원 ({sign}{percentageChange:F2}%)";
        contrastText.color = contrast >= 0 ? Color.red : new Color(0, 0.7f, 1f);

        int sum = 0;
        foreach (StockDetail stock in stock_data_arr)
        {
            sum += stock.closing_price;
        }

        // 최대/최소 종가 찾기
        int maxIndex = 0;
        int minIndex = 0;
        int maxPrice = stock_data_arr[0].closing_price;
        int minPrice = stock_data_arr[0].closing_price;

        for (int i = 1; i < stock_data_arr.Count; i++)
        {
            if (stock_data_arr[i].closing_price > maxPrice)
            {
                maxPrice = stock_data_arr[i].closing_price;
                maxIndex = i;
            }

            if (stock_data_arr[i].closing_price < minPrice)
            {
                minPrice = stock_data_arr[i].closing_price;
                minIndex = i;
            }
        }

        GameObject.Find("avg_price_data").GetComponent<TextMeshProUGUI>().text = 
            $"{(sum / stock_data_arr.Count):N0}원";

        GameObject.Find("max_price_data").GetComponent<TextMeshProUGUI>().text = 
            $"{maxPrice:N0}원";
        GameObject.Find("max_price_date").GetComponent<TextMeshProUGUI>().text = 
            stock_data_arr[maxIndex].day.Replace("/", "-");

        GameObject.Find("min_price_data").GetComponent<TextMeshProUGUI>().text = 
            $"{minPrice:N0}원";
        GameObject.Find("min_price_date").GetComponent<TextMeshProUGUI>().text = 
            stock_data_arr[minIndex].day.Replace("/", "-");

        GameObject.Find("fluc_rate_data").GetComponent<TextMeshProUGUI>().text = 
            $"{stock_data_arr[stock_data_arr.Count - 1].fluctuation_rate:F2}%";

        long totalShares = stock_data_arr[stock_data_arr.Count - 1].num_of_sh;
        string shareText;
        if (totalShares >= 1000000)
        {
            float millionShares = totalShares / 1000000f;
            shareText = $"{millionShares:F2}M";
        }
        else
        {
            shareText = totalShares.ToString("N0");
        }
        GameObject.Find("total_share_data").GetComponent<TextMeshProUGUI>().text = shareText;

        // 예측 데이터는 코루틴으로 처리
        StartCoroutine(UpdatePredictionData(stockInfo, std, dataValues, dataLabels, cur_price));
    }

    private IEnumerator UpdatePredictionData(StockInfo stockInfo, string std, List<float> dataValues, List<string> dataLabels, float cur_price)
    {
        // 예측 데이터 요청 시작
        var coroutine = StartCoroutine(tcpManager.CommunicateWithServerCoroutine(std, predict_risk => {
            if (predict_risk != null)
            {
                float[] pred_list = predict_risk.Item1;
                List<float> newDataValues = new List<float>(dataValues);
                List<string> newDataLabels = new List<string>(dataLabels);

                for (int i = 0; i < pred_list.Length; i++)
                {
                    newDataValues.Add(pred_list[i]);
                    newDataLabels.Add($"+ {i+1}일");
                }
                float cur_pred = pred_list[pred_list.Length - 1];

                // 예측 관련 UI 업데이트
                drawGraph.SetStockData(newDataValues, newDataLabels);

                string riskLevel = predict_risk.Item2;
                string koreanRiskLevel = ConvertRiskLevelToKorean(riskLevel);
                GameObject.Find("risk_data").GetComponent<TextMeshProUGUI>().text = koreanRiskLevel;

                GameObject.Find("pred_data").GetComponent<TextMeshProUGUI>().text = 
                    $"{cur_pred:N0}원";

                TextMeshProUGUI predContText = GameObject.Find("pred_cont").GetComponent<TextMeshProUGUI>();
                float priceDiff = cur_pred - cur_price;
                string predSign = priceDiff >= 0 ? "+" : "";
                predContText.text = $"{predSign}{priceDiff:N0}원";
                predContText.color = priceDiff >= 0 ? Color.red : new Color(0, 0.7f, 1f);
            }
        }));

        yield return coroutine;
    }

    // 위험도 텍스트 변환 메서드 추가
    private string ConvertRiskLevelToKorean(string englishRisk)
    {
        string koreanRisk;
        switch (englishRisk.ToLower())
        {
            case "high risk":
                koreanRisk = "높음";
                break;
            case "medium risk":
                koreanRisk = "중간";
                break;
            case "low risk":
                koreanRisk = "낮음";
                break;
            default:
                koreanRisk = englishRisk; // 알 수 없는 위험도는 원문 그대로 반환
                break;
        }
        OnRiskLevelUpdated?.Invoke(koreanRisk);
        return koreanRisk;
    }
}
