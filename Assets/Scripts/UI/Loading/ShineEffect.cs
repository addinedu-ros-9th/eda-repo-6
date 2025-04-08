using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ShineEffect : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    private float delayTimer = 0f;  // 타이머 하나만 추가

    private RectTransform rt;
    private Image shineImage;
    private ProgressBar parentBar;

    private void Start()
{
    rt = GetComponent<RectTransform>();
    shineImage = GetComponent<Image>();
    parentBar = GetComponentInParent<ProgressBar>();
    
    rt.anchoredPosition = new Vector2(0, 6f);
    
    rt.anchorMin = new Vector2(0, 0.5f);
    rt.anchorMax = new Vector2(0, 0.5f);
    rt.pivot = new Vector2(0.5f, 0.5f);

    // 처음에는 투명하게 시작
    Color startColor = shineImage.color;
    startColor.a = 0;
    shineImage.color = startColor;
    
    // 1초 후에 페이드 인
    Invoke("ShowShineEffect", 1f);
}

    void Update()
    {
        if (parentBar == null) return;
        
        float parentWidth = parentBar.GetComponent<RectTransform>().rect.width;
        float currentFillAmount = parentBar.GetComponent<Image>().fillAmount;
        
        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        float currentX = rt.anchoredPosition.x + (speed * Time.deltaTime);
        float maxX = currentFillAmount * parentWidth;
        
        if (currentX > maxX - 6f)
        {
            currentX = 6f;
            delayTimer = 0.5f;  // 0.5초 대기
        }
        
        rt.anchoredPosition = new Vector2(currentX, 6f);
    }
    private void ShowShineEffect()
{
    Color finalColor = shineImage.color;
    finalColor.a = 0.03f;
    shineImage.color = finalColor;
}
    
}