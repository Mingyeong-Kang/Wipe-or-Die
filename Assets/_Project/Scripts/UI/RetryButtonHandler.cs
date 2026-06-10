using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButtonHandler : MonoBehaviour
{
    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}