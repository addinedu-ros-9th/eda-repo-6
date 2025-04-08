using UnityEngine;
using TMPro;
using user;

public class NameAssign : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI greetingText;
    [SerializeField] private TextMeshProUGUI logoutText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // User 인스턴스에서 사용자 이름 가져오기
        string username = User.Instance.getName();

        // 텍스트 컴포넌트에 사용자 이름 설정
        if (greetingText != null)
        {
            greetingText.text = $"안녕하세요, {username}님!";
        }

        if (logoutText != null)
        {
            logoutText.text = username + "님 ▼";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
