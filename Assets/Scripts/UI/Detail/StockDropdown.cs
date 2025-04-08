using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StockDropdown : MonoBehaviour
{
    [SerializeField] private SpellDropdown spellDropdown;
    private TMP_Dropdown stockDropdown;

    private readonly Dictionary<string, string> choSungRanges = new Dictionary<string, string>
    {
        {"ㄱ", "가-나"},
        {"ㄴ", "나-다"},
        {"ㄷ", "다-라"},
        {"ㄹ", "라-마"},
        {"ㅁ", "마-바"},
        {"ㅂ", "바-사"},
        {"ㅅ", "사-아"},
        {"ㅇ", "아-자"},
        {"ㅈ", "자-차"},
        {"ㅊ", "차-카"},
        {"ㅋ", "카-타"},
        {"ㅌ", "타-파"},
        {"ㅍ", "파-하"},
        {"ㅎ", "하-힣"}
    };

    void Start()
    {
        stockDropdown = GetComponent<TMP_Dropdown>();
        spellDropdown.OnSpellSelected += UpdateStockList;

        // 시작할 때 '가'로 시작하는 주식 목록을 정렬해서 가져오기
        string condition = "stock_name >= '가' AND stock_name < '나' ORDER BY stock_name";
        List<string> filteredStocks = new List<string>();

        using (var stock_reader = dbManager.select("stock", "stock_name", condition))
        {
            if (stock_reader == null)
            {
                Debug.Log("'가'로 시작하는 주식이 없습니다.");
                return;
            }

            while (stock_reader.Read())
            {
                string stockName = stock_reader["stock_name"].ToString();
                filteredStocks.Add(stockName);
            }
        }

        stockDropdown.ClearOptions();
        stockDropdown.AddOptions(filteredStocks);
    }

    private void UpdateStockList(string selectedSpell)
    {
        List<string> filteredStocks = new List<string>();
        string condition;

        if (char.IsLetter(selectedSpell[0]) && !choSungRanges.ContainsKey(selectedSpell))
        {
            string upperSpell = selectedSpell.ToUpper();
            string lowerSpell = selectedSpell.ToLower();
            condition = $"LOWER(stock_name) LIKE '{lowerSpell}%' OR LOWER(stock_name) LIKE '{upperSpell}%' ORDER BY stock_name";
        }
        else
        {
            string range = choSungRanges[selectedSpell];
            string[] bounds = range.Split('-');
            
            if (selectedSpell == "ㅎ")
            {
                condition = $"stock_name >= '{bounds[0]}' AND stock_name <= '{bounds[1]}' ORDER BY stock_name";
            }
            else
            {
                condition = $"stock_name >= '{bounds[0]}' AND stock_name < '{bounds[1]}' ORDER BY stock_name";
            }
        }

        using (var stock_reader = dbManager.select("stock", "stock_name", condition))
        {
            if (stock_reader == null)
            {
                Debug.Log($"'{selectedSpell}'로 시작하는 주식이 없습니다.");
                return;
            }

            while (stock_reader.Read())
            {
                string stockName = stock_reader["stock_name"].ToString();
                filteredStocks.Add(stockName);
            }
        }

        stockDropdown.ClearOptions();
        stockDropdown.AddOptions(filteredStocks);
    }

    private void OnDestroy()
    {
        if (spellDropdown != null)
            spellDropdown.OnSpellSelected -= UpdateStockList;
    }
}
