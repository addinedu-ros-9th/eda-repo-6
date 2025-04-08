using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToDetail : MonoBehaviour
{
    public void OnClickBackToDetail()
    {
        SceneManager.LoadScene("UserDetail");
    }
}
