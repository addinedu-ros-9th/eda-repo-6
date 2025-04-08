using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;  // Image 컴포넌트를 사용하기 위해 추가

public class LoadingText : MonoBehaviour
{
    private TextMeshProUGUI tmpText;
    private Coroutine updateCoroutine;  // 코루틴 참조 저장용
    private readonly string[] loadingMessages = new string[]
    {
        "시장 데이터 불러오는 중...",
        "가격 정보 분석 중...",
        "거래 내역 확인 중...",
        "최종 데이터 검증 중..."
    };

    void Start()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        var ProgressBar = FindFirstObjectByType<ProgressBar>();
        if (ProgressBar != null)
        {
            updateCoroutine = StartCoroutine(UpdateTextBasedOnProgress(ProgressBar));
            ProgressBar.OnComplete += ShowCompleteMessage;
        }
    }

    private IEnumerator UpdateTextBasedOnProgress(ProgressBar ProgressBar)
    {
        while (!ProgressBar.GetComponent<Image>().fillAmount.Equals(1f))  // 다 채워지면 종료
        {
            float progress = ProgressBar.GetComponent<Image>().fillAmount;
            int messageIndex = Mathf.FloorToInt(progress * loadingMessages.Length);
            messageIndex = Mathf.Clamp(messageIndex, 0, loadingMessages.Length - 1);
            tmpText.text = loadingMessages[messageIndex];
            
            yield return null;
        }
    }

    private void ShowCompleteMessage()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);  // 코루틴 정지
        }
        tmpText.text = "완료!"; // 로딩이 끝나면 완료 메시지 표시
    }
}