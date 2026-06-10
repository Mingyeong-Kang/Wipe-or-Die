using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    public GameObject howToPlayPanel;

    public void OnClickMainGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OnClickHowToPlay()
    {
        howToPlayPanel.SetActive(true);
    }

    public void OnClickTutorial()
    {
        SceneManager.LoadScene("TUTORIAL");
    }

    public void OnClickClose()
    {
        howToPlayPanel.SetActive(false);
    }
}