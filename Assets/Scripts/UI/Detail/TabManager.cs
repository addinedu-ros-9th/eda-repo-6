using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using user;

public class TabManager : MonoBehaviour
{
    [Header("Tab Components")]
    [SerializeField] private GameObject onPickedEffect; // 선택 효과
    [SerializeField] private GameObject dashboardComponent;
    [SerializeField] private GameObject selectComponent;
    [SerializeField] private GameObject mystockComponent;
    
    [Header("UI References")]
    [SerializeField] private GameObject addStockPopup; // 종목 추가 팝업 UI
    [SerializeField] private TMP_Dropdown stockDropdown; // 종목 선택 드롭다운
    [SerializeField] private TMP_InputField purchasePriceInput; // 매수가 입력 필드
    [SerializeField] private TMP_InputField numOfStockInput;    // 주식 수량 입력 필드
    [SerializeField] private TextMeshProUGUI dashboardText;
    [SerializeField] private TextMeshProUGUI selectText;
    [SerializeField] private TextMeshProUGUI mystockText;

    private Vector3[] tabPositions = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(0, -110, 0),
        new Vector3(0, -220, 0)
    };

    // 선택된/선택되지 않은 텍스트 색상
    private Color selectedColor = new Color32(0x64, 0xFF, 0xDA, 0xFF); // 0x80은 255의 50%인 128
    private Color unselectedColor = new Color(0.5f, 0.5f, 0.5f); // 회색

    // 종목 추가 이벤트를 위한 델리게이트와 이벤트 선언
    public delegate void StockAddedEventHandler(string stockName, int purchasePrice, int numOfStock);
    public static event StockAddedEventHandler OnStockAdded;

    private void Awake()
    {
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (dashboardComponent == null) Debug.LogError("Dashboard Component is missing!");
        if (selectComponent == null) Debug.LogError("Select Component is missing!");
        if (mystockComponent == null) Debug.LogError("Mystock Component is missing!");
        // ... 다른 컴포넌트들도 검증
    }

    void Start()
    {
        // 시작할 때 모든 텍스트 색상을 unselectedColor로 초기화
        SetTabTextColors(0);  // 0번(Dashboard) 탭이 선택된 상태로 시작

        if (onPickedEffect != null)
        {
            onPickedEffect.SetActive(true);
            // 컴포넌트 활성화/비활성화
            dashboardComponent.SetActive(true);
            selectComponent.SetActive(false);
            mystockComponent.SetActive(false);
            // 이펙트 이동
            MoveEffectToTab(0);
        }
    }

    private void SetTabTextColors(int selectedTab)
    {
        if (dashboardText == null || selectText == null || mystockText == null)
        {
            Debug.LogError("Some text components are missing!");
            return;
        }

        // 모든 텍스트를 기본 색상으로 초기화
        dashboardText.color = unselectedColor;
        selectText.color = unselectedColor;
        mystockText.color = unselectedColor;

        // 선택된 탭만 색상 변경
        switch(selectedTab)
        {
            case 0: dashboardText.color = selectedColor; break;
            case 1: selectText.color = selectedColor; break;
            case 2: mystockText.color = selectedColor; break;
        }
    }

    public void OnClickDashboardTab()
    {
        MoveEffectToTab(0);
        SetTabTextColors(0);
        dashboardComponent.SetActive(true);
        selectComponent.SetActive(false);
        mystockComponent.SetActive(false);
    }

    public void OnClickSelectTab()
    {
        MoveEffectToTab(1);
        SetTabTextColors(1);
        dashboardComponent.SetActive(false);
        selectComponent.SetActive(true);
        mystockComponent.SetActive(false);
    }

    public void OnClickMyStockTab()
    {
        MoveEffectToTab(2);
        SetTabTextColors(2);
        dashboardComponent.SetActive(false);
        selectComponent.SetActive(false);
        mystockComponent.SetActive(true);
    }

    private void MoveEffectToTab(int tabIndex)
    {
        if (onPickedEffect != null)
        {
            onPickedEffect.GetComponent<RectTransform>().anchoredPosition = tabPositions[tabIndex];
        }
    }

    // 팝업 활성화
    public void OnClickAddButton()
    {
        if (addStockPopup != null)
        {
            addStockPopup.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Add Stock Popup is not assigned!");
        }
    }

    // 팝업 비활성화
    public void OnClickClosePopup()
    {
        if (addStockPopup != null)
        {
            addStockPopup.SetActive(false);
        }
    }

    public void OnClickAddStock()
    {
        if (!ValidateAddStockInputs()) return;

        try
        {
            string selectedStock = stockDropdown.options[stockDropdown.value].text;
            if (!int.TryParse(purchasePriceInput.text, out int purchasePrice))
            {
                Debug.LogError("Invalid purchase price input");
                return;
            }
            if (!int.TryParse(numOfStockInput.text, out int numOfStock))
            {
                Debug.LogError("Invalid number of stocks input");
                return;
            }

            User.Instance?.setStock(selectedStock, purchasePrice, numOfStock);
            OnStockAdded?.Invoke(selectedStock, purchasePrice, numOfStock);
            OnClickClosePopup();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding stock: {e.Message}");
        }
    }

    private bool ValidateAddStockInputs()
    {
        if (addStockPopup == null || stockDropdown == null)
        {
            Debug.LogError("Add stock popup or dropdown is missing");
            return false;
        }
        if (purchasePriceInput == null || numOfStockInput == null)
        {
            Debug.LogError("Price or number input field is missing");
            return false;
        }
        if (string.IsNullOrEmpty(purchasePriceInput.text) || 
            string.IsNullOrEmpty(numOfStockInput.text))
        {
            Debug.LogError("Price or number input is empty");
            return false;
        }
        return true;
    }
}