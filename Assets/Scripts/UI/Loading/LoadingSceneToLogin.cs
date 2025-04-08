using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneToLogin : MonoBehaviour
{
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private string nextSceneName = "LoginScene"; // 기본값으로 LoginScene 설정
    
    private void Start()
    {
        if (progressBar == null)
        {
            progressBar = GetComponent<ProgressBar>();
        }
        
        if (progressBar != null)
        {
            // 프로그레스 바의 OnComplete 이벤트에 씬 로드 메서드 등록
            progressBar.OnComplete += LoadNextScene;
        }
        else
        {
            Debug.LogError("ProgressBar 컴포넌트를 찾을 수 없습니다.");
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (progressBar != null)
        {
            progressBar.OnComplete -= LoadNextScene;
        }
    }
    
    private void LoadNextScene()
    {
        // 다음 씬으로 이동
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // 씬 이름이 지정되지 않았다면 다음 씬 인덱스로 이동
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            
            // 다음 씬이 존재하는지 확인
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("다음 씬이 존재하지 않습니다.");
            }
        }
    }
}