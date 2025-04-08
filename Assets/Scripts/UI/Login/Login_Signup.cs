using UnityEngine;
using UnityEngine.UI;

public class Login_Signup : MonoBehaviour
{
    [SerializeField] private GameObject signupComponent; // SignupComponent 게임오브젝트 참조
    [SerializeField] private GameObject loginComponent; // LoginComponent 게임오브젝트 참조
    
    private void Start()
    {
        // 시작 시 SignupComponent는 비활성화
        if (signupComponent != null)
        {
            signupComponent.SetActive(false);
        }
        if (loginComponent != null)
        {
            loginComponent.SetActive(true);
        }
    }
    
    // 회원가입 화면으로 전환하는 메서드
    public void SwitchToSignup()
    {
        // LoginComponent를 비활성화
        if (loginComponent != null)
        {
            loginComponent.SetActive(false);
        }
        // SignupComponent를 활성화
        if (signupComponent != null)
        {
            signupComponent.SetActive(true);
        }

    }
    
    public void SwitchToLogin()
    {
        // SignupComponent를 비활성화
        if (signupComponent != null)
        {
            signupComponent.SetActive(false);
        }
        // LoginComponent를 활성화
        if (loginComponent != null)
        {
            loginComponent.SetActive(true);
        }
    }
}