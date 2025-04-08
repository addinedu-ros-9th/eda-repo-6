using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RiskDegree : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI riskText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (riskText == null)
        {
            riskText = GameObject.Find("risk_data").GetComponent<TextMeshProUGUI>();
        }

        StockDataManager.OnRiskLevelUpdated += SetRiskLevel;
    }

    private void SetRiskLevel(string riskLevel)
    {
        Color baseColor;
        switch (riskLevel)
        {
            case "낮음":
                progressBar.fillAmount = 0.25f;
                baseColor = new Color(0, 0.7f, 1f); // 연푸른색
                break;
            case "중간":
                progressBar.fillAmount = 0.5f;
                baseColor = new Color(1f, 0.92f, 0.016f); // 노란색
                break;
            case "높음":
                progressBar.fillAmount = 0.75f;
                baseColor = new Color(1f, 0.3f, 0.3f); // 붉은색
                break;
            default:
                progressBar.fillAmount = 0f;
                baseColor = Color.white;
                break;
        }
        
        progressBar.color = baseColor;
        var gradient = progressBar.GetComponent<UIGradient>();
        if (gradient != null)
        {
            gradient.gradientStart = baseColor;
            gradient.gradientEnd = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
        }
    }

    void OnDestroy()
    {
        StockDataManager.OnRiskLevelUpdated -= SetRiskLevel;
    }
}
