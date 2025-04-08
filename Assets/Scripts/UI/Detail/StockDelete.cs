using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using user;
using System.Threading.Tasks;
using TMPro;
using stockinfo;
using System;
using stockdetail;

public class StockDelete : MonoBehaviour
{
    private bool isDeleteMode = false;
    private List<Toggle> stockToggles = new List<Toggle>();
    private ScrollManager scrollManager;
    [SerializeField] private Button deleteButton;

    void Start()
    {
        scrollManager = FindFirstObjectByType<ScrollManager>();
    }

    public void OnDeleteButtonClick()
    {
        if (!isDeleteMode)
        {
            deleteButton.GetComponentInChildren<TextMeshProUGUI>().text = "일괄 삭제";
            // 삭제 모드 활성화
            isDeleteMode = true;
            scrollManager.ToggleCheckboxes(true);
        }
        else
        {
            // 체크된 항목들 삭제
            DeleteSelectedStocks();
            isDeleteMode = false;
            scrollManager.ToggleCheckboxes(false);
        }
    }

    private void DeleteSelectedStocks()
    {
        List<string> stocksToDelete = scrollManager.GetSelectedStocks();
        if (stocksToDelete.Count == 0)
        {
            deleteButton.GetComponentInChildren<TextMeshProUGUI>().text = "종목 삭제";
            return;
        }
        foreach (string stock in stocksToDelete)
        {
            if (User.Instance != null)
            {
                User.Instance.delStock(stock);
            }
        }
        scrollManager.RefreshAfterDeletion();
        deleteButton.GetComponentInChildren<TextMeshProUGUI>().text = "종목 삭제";
    }

    private void SetTabTextColors(int selectedTab)
    {
        try 
        {
            var dashboardText = GameObject.Find("dashboard_txt");
            var selectText = GameObject.Find("select_txt");
            var mystockText = GameObject.Find("mystock_txt");
            // ...
        }
        catch (Exception ex)
        {
            Debug.LogError("Error setting tab text colors: " + ex.Message);
        }
    }
}