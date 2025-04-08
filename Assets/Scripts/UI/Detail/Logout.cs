using UnityEngine;
using UnityEngine.SceneManagement;
using user;

public class Logout : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickLogout()
    {
        // User 인스턴스 초기화
        User.Instance.delUser();  // static instance를 null로 설정하여 다음 로그인에서 새로 생성되게 함
        
        // Login 씬으로 전환
        SceneManager.LoadScene("Login");
    }
}
