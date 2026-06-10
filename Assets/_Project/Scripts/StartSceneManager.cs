using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump;

public class StartSceneManager : MonoBehaviour
{
    public GameObject howToPlayPanel;

    private void Awake()
    {
        foreach (var p in FindObjectsByType<ContinuousMoveProvider>(FindObjectsSortMode.None))
            p.enabled = false;
        foreach (var p in FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets.DynamicMoveProvider>(FindObjectsSortMode.None))
            p.enabled = false;
        foreach (var p in FindObjectsByType<SnapTurnProvider>(FindObjectsSortMode.None))
            p.enabled = false;
        foreach (var p in FindObjectsByType<ContinuousTurnProvider>(FindObjectsSortMode.None))
            p.enabled = false;
        foreach (var p in FindObjectsByType<JumpProvider>(FindObjectsSortMode.None))
            p.enabled = false;
    }

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