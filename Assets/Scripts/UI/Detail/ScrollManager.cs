using UnityEngine;
using UnityEngine.UI;
using user;
using System.Collections.Generic;
using TMPro;
using stockinfo;
using System;
using stockdetail;
using System.Threading.Tasks;
using System.Linq;

public class ScrollManager : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;     // 스크롤뷰 컴포넌트
    [SerializeField] private GameObject groupPrefab;    // 그룹 프리팹
    [SerializeField] private GameObject viewPrefab;     // 뷰 프리팹
    
    private GameObject currentGroup;                    // 현재 활성화된 그룹
    private int itemsInCurrentGroup = 0;               // 현재 그룹에 들어있는 아이템 수
    private int totalItemsAdded = 0;
    private const int MAX_ITEMS_PER_GROUP = 2;         // 그룹당 최대 아이템 수
    private List<User.HoldingStockInfo> holdingStocks;
    private List<Toggle> stockToggles = new List<Toggle>();
    private List<StockItemView> stockViews = new List<StockItemView>();

    // 각 종목의 매수가와 수량을 저장할 구조체 정의
    private class StockPurchaseInfo
    {
        public int PurchasePrice { get; set; }
        public int NumberOfStocks { get; set; }
    }

    private Dictionary<string, StockPurchaseInfo> stockPurchaseInfos = new Dictionary<string, StockPurchaseInfo>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        if (User.Instance != null)
        {
            // 초기 데이터 로드
            holdingStocks = User.Instance.getStock();
            
            // stockPurchaseInfos 딕셔너리 초기화
            foreach (var stock in holdingStocks)
            {
                stockPurchaseInfos[stock.StockName] = new StockPurchaseInfo
                {
                    PurchasePrice = stock.PurchasePrice,
                    NumberOfStocks = stock.NumberOfStocks
                };
            }
            
            await InitializeStockList();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async Task AddNewItem()
    {
        if (currentGroup == null || itemsInCurrentGroup >= MAX_ITEMS_PER_GROUP)
        {
            CreateNewGroup();
        }

        string stockName = holdingStocks[totalItemsAdded].StockName;
        GameObject newView = Instantiate(viewPrefab, currentGroup.transform);
        await SetStockItemData(newView, stockName);
        
        totalItemsAdded++;
        itemsInCurrentGroup++;
    }

    private async Task SetStockItemData(GameObject stockItem, string stockName)
    {
        string targetDate = await GetMostRecentTradingDay(stockName);
        StockInfo stockInfo = new StockInfo(targetDate, targetDate);
        List<StockDetail> stock_data_arr = await stockInfo.get_stock_info(stockName);

        // 각 TMP 컴포넌트 찾기
        TextMeshProUGUI name = stockItem.transform.Find("name").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cur_price = stockItem.transform.Find("cur_price").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI my_price = stockItem.transform.Find("mine").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI margin = stockItem.transform.Find("margin").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI fluct = stockItem.transform.Find("fluc").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI attr = stockItem.transform.Find("attr").GetComponent<TextMeshProUGUI>();

        // 데이터 설정
        if (name != null) name.text = stockName;
        // 여기에 다른 데이터 설정 추가
        // 예: 현재가, 매수가, 수익률 등
        if (cur_price != null) cur_price.text = Convert.ToString(stock_data_arr[0].closing_price);

        // 매수가와 수량 정보 설정
        if (stockPurchaseInfos.ContainsKey(stockName))
        {
            var purchaseInfo = stockPurchaseInfos[stockName];
            
            if (my_price != null)
                my_price.text = purchaseInfo.PurchasePrice.ToString();
            
            // 수익률 계산
            if (margin != null)
            {
                float currentPrice = (float)stock_data_arr[0].closing_price;
                float purchasePrice = float.Parse(purchaseInfo.PurchasePrice.ToString());
                float marginRate = ((currentPrice - purchasePrice) / purchasePrice) * 100;
                string sign = marginRate >= 0 ? "+" : "";
                margin.text = $"{sign}{marginRate:F2}%";
                margin.color = marginRate >= 0 ? Color.red : new Color(0, 0.7f, 1f);
            }
            
            // 필요한 경우 수량 정보도 표시
            // TextMeshProUGUI stockAmount = stockItem.transform.Find("amount").GetComponent<TextMeshProUGUI>();
            // if (stockAmount != null)
            //     stockAmount.text = purchaseInfo.NumberOfStocks;
        }

        if (fluct != null) 
        {
            float fluctRate = stock_data_arr[0].fluctuation_rate;
            string arrow = fluctRate >= 0 ? " ▲" : " ▼";
            fluct.text = $"{fluctRate:F2}%{arrow}";
            fluct.color = fluctRate >= 0 ? Color.red : new Color(0, 0.7f, 1f);
        }

        if (attr != null) attr.text = stock_data_arr[0].abbr;

        // Toggle 컴포넌트 찾아서 리스트에 추가
        Toggle stockToggle = stockItem.GetComponentInChildren<Toggle>();
        if (stockToggle != null)
        {
            stockToggle.gameObject.SetActive(false); // 초기에는 숨김
            stockToggles.Add(stockToggle);
        }
    }

    private void CreateNewGroup()
    {
        if (scrollRect == null || groupPrefab == null)
        {
            Debug.LogError("Required components are missing!");
            return;
        }
        
        currentGroup = Instantiate(groupPrefab, scrollRect.content);
        itemsInCurrentGroup = 0;
    }

    private void OnEnable()
    {
        TabManager.OnStockAdded += HandleStockAdded;
    }

    private void OnDisable()
    {
        TabManager.OnStockAdded -= HandleStockAdded;
    }

    private async void HandleStockAdded(string newStock, int purchasePrice, int numOfStock)
    {
        // 새로운 주식 정보를 딕셔너리에 추가
        stockPurchaseInfos[newStock] = new StockPurchaseInfo
        {
            PurchasePrice = purchasePrice,
            NumberOfStocks = numOfStock
        };
        
        // UI 갱신
        await RefreshStockList();
    }

    private async void HandleStockUpdated()
    {
        await RefreshStockList();
    }

    private async Task RefreshStockList()
    {
        try
        {
            // 기존 아이템 정리
            foreach (Transform child in scrollRect.content)
            {
                Destroy(child.gameObject);
            }
            
            stockViews.Clear();
            stockToggles.Clear();
            
            // 상태 초기화
            currentGroup = null;
            itemsInCurrentGroup = 0;
            totalItemsAdded = 0;
            
            // 목록 다시 로드
            holdingStocks = User.Instance.getStock();
            await InitializeStockList();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to refresh stock list: {e}");
        }
    }
    
    // 체크박스 표시/숨김 토글 메서드
    public void ToggleCheckboxes(bool show)
    {
        foreach (Toggle toggle in stockToggles)
        {
            if (toggle != null)
            {
                toggle.gameObject.SetActive(show);
                toggle.isOn = false; // 체크박스 초기화
            }
        }
    }

    // 선택된 주식 목록 반환
    public List<string> GetSelectedStocks()
    {
        List<string> selectedStocks = new List<string>();
        for (int i = 0; i < stockToggles.Count; i++)
        {
            if (stockToggles[i] != null && stockToggles[i].isOn)
            {
                selectedStocks.Add(holdingStocks[i].StockName);
            }
        }
        return selectedStocks;
    }

    // 삭제 후 목록 새로고침
    public async void RefreshAfterDeletion()
    {
        foreach (var view in stockViews)
        {
            if (view != null) Destroy(view.gameObject);
        }
        stockViews.Clear();
        
        currentGroup = null;
        itemsInCurrentGroup = 0;
        totalItemsAdded = 0;
        stockToggles.Clear();

        await RefreshStockList();
    }

    private async Task<string> GetMostRecentTradingDay(string stockName)
    {
        DateTime currentDate = DateTime.Now;
        int maxAttempts = 10; // 최대 10일 전까지만 확인
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // 주말 체크
            if (currentDate.DayOfWeek == DayOfWeek.Saturday)
            {
                currentDate = currentDate.AddDays(-1);
                attempts++;
                continue;
            }
            if (currentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                currentDate = currentDate.AddDays(-2);
                attempts++;
                continue;
            }

            // 해당 날짜의 데이터가 있는지 확인
            string dateString = currentDate.ToString("yyyyMMdd");
            try
            {
                StockInfo testInfo = new StockInfo(dateString, dateString);
                List<StockDetail> testData = await testInfo.get_stock_info(stockName);
                
                // 데이터가 존재하면 해당 날짜 반환
                if (testData != null && testData.Count > 0)
                {
                    return dateString;
                }
            }
            catch
            {
                // 에러 발생 시 (데이터가 없는 경우) 이전 날짜 확인
            }

            currentDate = currentDate.AddDays(-1);
            attempts++;
        }

        // 기본값으로 현재 날짜 반환 (모든 시도가 실패한 경우)
        return DateTime.Now.ToString("yyyyMMdd");
    }

    private async Task InitializeStockList()
    {
        try
        {
            if (User.Instance == null) return;
            
            // 모든 주식 데이터를 병렬로 가져오기
            var tasks = holdingStocks.Select(async stock => {
                var targetDate = await GetMostRecentTradingDay(stock.StockName);
                var stockInfo = new StockInfo(targetDate, targetDate);
                var stockData = await stockInfo.get_stock_info(stock.StockName);
                return (stock, stockData);
            });

            var results = await Task.WhenAll(tasks);

            // UI 업데이트는 한번에 처리
            foreach (var (stock, stockData) in results)
            {
                await CreateStockItem(stock, stockData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize stock list: {e}");
        }
    }

    private async Task CreateStockItem(User.HoldingStockInfo stock, List<StockDetail> stockData)
    {
        if (currentGroup == null || itemsInCurrentGroup >= MAX_ITEMS_PER_GROUP)
        {
            CreateNewGroup();
        }

        GameObject newView = Instantiate(viewPrefab, currentGroup.transform);
        await SetStockItemData(newView, stock.StockName);
        
        totalItemsAdded++;
        itemsInCurrentGroup++;

        StockItemView stockItemView = newView.GetComponent<StockItemView>();
        if (stockItemView != null)
        {
            stockItemView.Initialize(new StockViewData
            {
                StockName = stock.StockName,
                CurrentPrice = (float)stockData[0].closing_price,
                PurchasePrice = stock.PurchasePrice,
                FluctuationRate = (float)stockData[0].fluctuation_rate,
                Attribute = stockData[0].abbr
            });
            stockViews.Add(stockItemView);
        }
    }
}

// 프리팹용 컴포넌트 클래스 생성
public class StockItemView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI currentPriceText;
    [SerializeField] private TextMeshProUGUI myPriceText;
    [SerializeField] private TextMeshProUGUI marginText;
    [SerializeField] private TextMeshProUGUI fluctText;
    [SerializeField] private TextMeshProUGUI attrText;
    [SerializeField] private Toggle selectionToggle;

    public void Initialize(StockViewData data)
    {
        nameText.text = data.StockName;
        currentPriceText.text = data.CurrentPrice.ToString("N0");
        myPriceText.text = data.PurchasePrice.ToString("N0");
        
        float marginRate = ((data.CurrentPrice - data.PurchasePrice) / data.PurchasePrice) * 100;
        marginText.text = $"{(marginRate >= 0 ? "+" : "")}{marginRate:F2}%";
        marginText.color = marginRate >= 0 ? Color.red : new Color(0, 0.7f, 1f);
        
        // ... 나머지 UI 업데이트
    }
}

// 데이터 구조체
public struct StockViewData
{
    public string StockName;
    public float CurrentPrice;
    public float PurchasePrice;
    public float FluctuationRate;
    public string Attribute;
}
