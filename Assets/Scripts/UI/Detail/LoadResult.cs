using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 관리를 위해 필요
using TMPro;

public class LoadResult : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    private string stock_name;
    
    private void Start()
    {
        // 이 스크립트가 붙어있는 게임오브젝트의 부모를 DontDestroyOnLoad로 설정
        DontDestroyOnLoad(transform.root.gameObject);
    }

    public void OnClickLoadResult()
    {
        // 씬 전환 전에 미리 stock_name 저장
        stock_name = dropdown.options[dropdown.value].text;
        // dropdown 참조 제거
        dropdown = null;
        SceneManager.LoadScene("ResultScene");
    }

    public string GetStockName()
    {
        print(stock_name);
        return stock_name;
    }
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    
}