using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 튜토리얼 단계 관리.
/// Step 1: 이동 설명 패널
/// Step 2: 청소 도구 설명 패널 → 유리창 직접 닦기
/// Step 3: 장애물 설명 패널 → 장애물 등장
/// Step 4: 완료 패널 → MainScene 이동
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public enum TutorialStep { Move, Clean, Obstacle, Complete }

    [Header("Panels")]
    [SerializeField] private GameObject movePanel;
    [SerializeField] private GameObject cleanPanel;
    [SerializeField] private GameObject obstaclePanel;
    [SerializeField] private GameObject completePanel;

    [Header("Tutorial Window")]
    [SerializeField] private Window tutorialWindow;

    [Header("Obstacle")]
    [SerializeField] private ObstacleSpawner obstacleSpawner;

    [Header("Step Text (optional)")]
    [SerializeField] private TextMeshProUGUI stepText;

    private TutorialStep _currentStep = TutorialStep.Move;

    private void Start()
    {
        ShowStep(TutorialStep.Move);
        if (tutorialWindow != null)
            tutorialWindow.OnCleaned += OnWindowCleaned;
    }

    private void OnDestroy()
    {
        if (tutorialWindow != null)
            tutorialWindow.OnCleaned -= OnWindowCleaned;
    }

    // ── 패널 표시 ──────────────────────────────────────────
    private void ShowStep(TutorialStep step)
    {
        _currentStep = step;

        movePanel?.SetActive(step == TutorialStep.Move);
        cleanPanel?.SetActive(step == TutorialStep.Clean);
        obstaclePanel?.SetActive(step == TutorialStep.Obstacle);
        completePanel?.SetActive(step == TutorialStep.Complete);

        if (stepText != null)
            stepText.text = $"Step {(int)step + 1} / {System.Enum.GetValues(typeof(TutorialStep)).Length}";

        // 장애물 스텝 시작 시 스포너 활성화
        if (step == TutorialStep.Obstacle && obstacleSpawner != null)
            obstacleSpawner.enabled = true;
    }

    // ── Got it 버튼 ────────────────────────────────────────
    /// <summary>Step 1 Got it 버튼에 연결</summary>
    public void OnGotItMove()
    {
        if (_currentStep != TutorialStep.Move) return;
        ShowStep(TutorialStep.Clean);
    }

    /// <summary>Step 2 Got it 버튼에 연결 — 실제로 창문을 닦아야 넘어감</summary>
    public void OnGotItClean()
    {
        if (_currentStep != TutorialStep.Clean) return;
        // 패널만 닫고 창문 청소 대기
        cleanPanel?.SetActive(false);
    }

    /// <summary>Step 3 Got it 버튼에 연결</summary>
    public void OnGotItObstacle()
    {
        if (_currentStep != TutorialStep.Obstacle) return;
        // 패널 닫고 장애물 등장 대기
        obstaclePanel?.SetActive(false);

        // 일정 시간 후 자동으로 완료 (장애물 피하거나 맞으면)
        Invoke(nameof(CompleteObstacleStep), 5f);
    }

    // ── 이벤트 ─────────────────────────────────────────────
    private void OnWindowCleaned(Window window)
    {
        if (_currentStep != TutorialStep.Clean) return;
        ShowStep(TutorialStep.Obstacle);
    }

    private void CompleteObstacleStep()
    {
        if (_currentStep != TutorialStep.Obstacle) return;

        if (obstacleSpawner != null)
            obstacleSpawner.StopSpawning();

        ShowStep(TutorialStep.Complete);
    }

    // ── 완료 버튼 ──────────────────────────────────────────
    /// <summary>Complete 패널 버튼에 연결</summary>
    public void OnClickStartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    /// <summary>Complete 패널 버튼에 연결</summary>
    public void OnClickBackToMenu()
    {
        SceneManager.LoadScene("START");
    }
}
