using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DrawGraph : MonoBehaviour
{
    [Header("Graph Settings")]
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private Color lineColor = new Color(0.3f, 0.85f, 0.4f);
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
    [SerializeField] private float lineThickness = 3f;
    
    [Header("Data")]
    [SerializeField] private List<float> dataValues = new List<float>();
    [SerializeField] private List<string> dataLabels = new List<string>();
    
    [Header("Grid and Labels")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private int gridLinesX = 4;
    [SerializeField] private int gridLinesY = 4;
    [SerializeField] private bool showLabels = true;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private TMP_FontAsset labelFontAsset;
    [SerializeField] private bool showAllGridLines = false; // 모든 데이터 포인트에 그리드 라인 표시 여부
    
    [Header("Y Axis Format")]
    [SerializeField] private string yAxisNumberFormat = "N0"; // 숫자 포맷 (N0: 천 단위 구분기호, F1: 소수점 1자리)
    [SerializeField] private bool roundYAxisLabels = true;    // 레이블 값을 더 읽기 쉽게 반올림
    
    [Header("Graph Padding")]
    [SerializeField] private float yAxisPaddingPercentage = 20f; // Y축 상하 여백 비율(%)
    
    [Header("Line Colors")]
    [SerializeField] private Color actualLineColor = new Color(0.3f, 0.85f, 0.4f); // 기존 데이터 색상
    [SerializeField] private Color predictionLineColor = new Color(0.85f, 0.3f, 0.3f); // 예측 데이터 색상
    
    private List<GameObject> points = new List<GameObject>();
    private List<GameObject> lines = new List<GameObject>();
    private GameObject background;
    private List<GameObject> gridLines = new List<GameObject>();
    private List<GameObject> labels = new List<GameObject>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        // 그래프 생성
        CreateGraph();
    }

    public void CreateGraph()
    {
        if(dataValues.Count == 0)
        {
            Debug.Log("dataValues is empty");
            return;
        }
        ClearGraph();
        CreateBackground();
        
        if (dataValues.Count < 2) return;
        
        // 최대값, 최소값 계산
        float maxValue = Mathf.Max(dataValues.ToArray());
        float minValue = Mathf.Min(dataValues.ToArray());
        float range = maxValue - minValue;
        
        // 여백 추가 (20%)
        float padding = range * (yAxisPaddingPercentage / 100f);
        maxValue += padding;
        minValue -= padding;
        
        // 범위가 너무 작을 경우(데이터가 거의 일정할 때) 최소한의 범위 보장
        if (range < 0.01f * maxValue)
        {
            float minRange = maxValue * 0.1f; // 최대값의 10% 정도는 범위로 확보
            maxValue = maxValue + minRange/2;
            minValue = minValue - minRange/2;
        }
        
        range = maxValue - minValue;
        
        // 그래프 영역 크기
        float graphWidth = graphContainer.rect.width;
        float graphHeight = graphContainer.rect.height;
        
        // 축 여백 설정 - 레이블을 위한 공간 확보
        float xAxisMargin = 50f;           // 왼쪽 여백(Y축 레이블용)
        float yAxisMargin = 30f;           // 아래쪽 여백 - 기존 60f에서 30f로 감소
        
        // 실제 그래프가 그려질 영역 계산
        float plotWidth = graphWidth - xAxisMargin;
        float plotHeight = graphHeight - yAxisMargin;
        
        // 데이터 포인트 생성 전에 X축 위치를 미리 계산
        List<float> xPositions = new List<float>();
        for (int i = 0; i < dataLabels.Count; i++)
        {
            float xPos = xAxisMargin + ((i / (float)(dataLabels.Count - 1)) * plotWidth);
            xPositions.Add(xPos);
        }
        
        // Y축 그리드 라인 및 레이블 생성
        for (int i = 0; i <= gridLinesY; i++)
        {
            float yPos = yAxisMargin + (i / (float)gridLinesY) * plotHeight;
            
            // Y축 그리드 라인
            if (showGrid)
            {
                GameObject gridLine = CreateGridLine(new Vector2(xAxisMargin, yPos), 
                                                   new Vector2(xAxisMargin + plotWidth, yPos));
                gridLines.Add(gridLine);
            }
            
            // Y축 레이블
            if (showLabels)
            {
                float rawValue = minValue + ((maxValue - minValue) * (i / (float)gridLinesY));
                float displayValue = rawValue;
                
                // 읽기 쉽게 레이블 값 반올림 (선택적)
                if (roundYAxisLabels)
                {
                    float magnitude = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(Mathf.Abs(rawValue))));
                    displayValue = Mathf.Round(rawValue / (magnitude / 10)) * (magnitude / 10);
                }
                
                // isXAxisLabel 매개변수를 전달하지 않으면 기본값 false가 적용됩니다
                GameObject label = CreateLabel(displayValue.ToString(yAxisNumberFormat), new Vector2(xAxisMargin/2, yPos));
                labels.Add(label);
            }
        }
        
        // X축 그리드 라인 및 레이블 생성
        for (int i = 0; i < dataLabels.Count; i++)
        {
            // X축 그리드 라인 (선택적으로 모든 데이터 포인트 또는 일부만)
            if (showGrid && (showAllGridLines || i % Mathf.Max(1, dataLabels.Count / (gridLinesX + 1)) == 0))
            {
                GameObject gridLine = CreateGridLine(new Vector2(xPositions[i], yAxisMargin), 
                                                  new Vector2(xPositions[i], yAxisMargin + plotHeight));
                gridLines.Add(gridLine);
            }
            
            // X축 레이블 
            if (showLabels)
            {
                // X축 레이블 위치를 더 아래로 이동 (yAxisMargin/2 → yAxisMargin * 0.2)
                // 음수 값을 사용하여 그래프 영역 밖으로 완전히 빼내기
                Vector2 labelPosition = new Vector2(xPositions[i], -10); // 그래프 영역 밑으로 이동
                GameObject label = CreateLabel(dataLabels[i], labelPosition, true);
                labels.Add(label);
            }
        }
        
        // 실제 X축과 Y축 선 그리기 (더 굵게)
        if (showGrid)
        {
            // X축 (가로선)
            GameObject xAxisObj = new GameObject("XAxis");
            xAxisObj.transform.SetParent(graphContainer, false);
            
            RectTransform xAxisRect = xAxisObj.AddComponent<RectTransform>();
            xAxisRect.anchorMin = Vector2.zero;
            xAxisRect.anchorMax = Vector2.zero;
            xAxisRect.anchoredPosition = new Vector2(xAxisMargin + plotWidth/2, yAxisMargin);
            xAxisRect.sizeDelta = new Vector2(plotWidth, 2f); // 더 두꺼운 선
            
            Image xAxisImage = xAxisObj.AddComponent<Image>();
            xAxisImage.color = new Color(1f, 1f, 1f, 0.8f); // 더 진한 색상
            gridLines.Add(xAxisObj);
            
            // Y축 (세로선)
            GameObject yAxisObj = new GameObject("YAxis");
            yAxisObj.transform.SetParent(graphContainer, false);
            
            RectTransform yAxisRect = yAxisObj.AddComponent<RectTransform>();
            yAxisRect.anchorMin = Vector2.zero;
            yAxisRect.anchorMax = Vector2.zero;
            yAxisRect.anchoredPosition = new Vector2(xAxisMargin, yAxisMargin + plotHeight/2);
            yAxisRect.sizeDelta = new Vector2(2f, plotHeight); // 더 두꺼운 선
            
            Image yAxisImage = yAxisObj.AddComponent<Image>();
            yAxisImage.color = new Color(1f, 1f, 1f, 0.8f); // 더 진한 색상
            gridLines.Add(yAxisObj);
        }
        
        // 데이터 포인트와 라인 생성
        Vector2 lastPointPosition = Vector2.zero;
        int predictionStartIndex = dataValues.Count - 30; // 뒤의 30개 데이터는 예측값

        for (int i = 0; i < dataValues.Count; i++)
        {
            float yPosition = yAxisMargin + ((dataValues[i] - minValue) / range) * plotHeight;
            Vector2 pointPosition = new Vector2(xPositions[i], yPosition);
            
            // 데이터 포인트 생성 (예측 데이터는 다른 색상 사용)
            Color pointColor = i >= predictionStartIndex ? predictionLineColor : actualLineColor;
            GameObject pointObject = CreatePoint(pointPosition, pointColor);
            points.Add(pointObject);
            
            // 라인 생성 (첫 포인트 제외)
            if (i > 0)
            {
                // 현재 포인트가 예측 데이터의 시작점이면 이전 데이터와 연결하는 선도 예측 색상으로
                Color lineColor = i >= predictionStartIndex ? predictionLineColor : actualLineColor;
                GameObject lineObject = CreateLine(lastPointPosition, pointPosition, lineColor);
                lines.Add(lineObject);
            }
            
            lastPointPosition = pointPosition;
        }
    }
    
    private void CreateBackground()
    {
        background = new GameObject("Background");
        background.transform.SetParent(graphContainer, false);
        
        RectTransform rectTransform = background.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        Image image = background.AddComponent<Image>();
        image.color = backgroundColor;
    }
    
    private GameObject CreatePoint(Vector2 position, Color color)
    {
        GameObject pointObject = new GameObject("Point");
        pointObject.transform.SetParent(graphContainer, false);
        
        RectTransform rectTransform = pointObject.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(10, 10);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        
        Image image = pointObject.AddComponent<Image>();
        image.sprite = circleSprite;
        image.color = color;
        
        return pointObject;
    }
    
    private GameObject CreateLine(Vector2 startPosition, Vector2 endPosition, Color color)
    {
        GameObject lineObject = new GameObject("Line");
        lineObject.transform.SetParent(graphContainer, false);
        
        RectTransform rectTransform = lineObject.AddComponent<RectTransform>();
        
        Vector2 direction = (endPosition - startPosition).normalized;
        float distance = Vector2.Distance(startPosition, endPosition);
        
        rectTransform.anchoredPosition = startPosition + direction * distance * 0.5f;
        rectTransform.sizeDelta = new Vector2(distance, lineThickness);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.localEulerAngles = new Vector3(0, 0, angle);
        
        Image image = lineObject.AddComponent<Image>();
        image.color = color;
        
        return lineObject;
    }
    
    private void ClearGraph()
    {
        // 이전 포인트들 제거
        foreach (GameObject point in points)
        {
            if (point != null)
                Destroy(point);
        }
        points.Clear();
        
        // 이전 라인들 제거
        foreach (GameObject line in lines)
        {
            if (line != null)
                Destroy(line);
        }
        lines.Clear();
        
        // 배경 제거
        if (background != null)
            Destroy(background);
        
        // 그리드 제거
        foreach (GameObject gridLine in gridLines)
        {
            if (gridLine != null)
                Destroy(gridLine);
        }
        gridLines.Clear();
        
        // 레이블 제거
        foreach (GameObject label in labels)
        {
            if (label != null)
                Destroy(label);
        }
        labels.Clear();
    }
    
    // 주식 데이터 설정 메서드
    public void SetStockData(List<float> values, List<string> labels)
    {
        dataValues = values;
        dataLabels = labels;
        CreateGraph();
    }

    private GameObject CreateGridLine(Vector2 start, Vector2 end)
    {
        GameObject gridObject = new GameObject("GridLine");
        gridObject.transform.SetParent(graphContainer, false);
        
        RectTransform rectTransform = gridObject.AddComponent<RectTransform>();
        
        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        
        rectTransform.anchoredPosition = start + direction * distance * 0.5f;
        rectTransform.sizeDelta = new Vector2(distance, 1f);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.localEulerAngles = new Vector3(0, 0, angle);
        
        Image image = gridObject.AddComponent<Image>();
        image.color = gridColor;
        
        return gridObject;
    }

    private GameObject CreateLabel(string text, Vector2 position, bool isXAxisLabel = false)
    {
        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(graphContainer, false);
        
        RectTransform rectTransform = labelObject.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        
        // X축 레이블 크기를 더 합리적으로 조정 (100,60에서 축소)
        rectTransform.sizeDelta = isXAxisLabel ? new Vector2(70, 40) : new Vector2(70, 30);
        
        TextMeshProUGUI textComponent = labelObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        
        if (labelFontAsset != null)
        {
            textComponent.font = labelFontAsset;
        }
        
        textComponent.fontSize = fontSize;
        textComponent.color = labelColor;
        
        if (isXAxisLabel)
        {
            // X축 레이블 회전 각도 조정
            rectTransform.Rotate(0, 0, 30); // 왼쪽으로 45도 회전
            
            // 정렬 방식 변경
            textComponent.alignment = TextAlignmentOptions.Center; // 중앙 정렬
            
            // 위치 미세 조정 (아래로 약간 이동)
            rectTransform.anchoredPosition += new Vector2(0, -10); // 수치 감소
        }
        else
        {
            textComponent.alignment = position.x < 0 ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.Midline;
        }
        
        textComponent.enableAutoSizing = false;
        textComponent.fontStyle = FontStyles.Bold;
        
        return labelObject;
    }

}
