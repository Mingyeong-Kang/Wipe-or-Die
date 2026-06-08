using UnityEngine;

/// <summary>
/// 창문 하나의 청소 상태를 관리하는 상태머신.
/// 청소 순서: Dirty → Sprayed → Sponged → Rinsed → Clean
/// </summary>
public class Window : MonoBehaviour
{
    public enum CleaningState { Dirty, Sprayed, Sponged, Rinsed, Clean }

    [Header("Cleaning Settings")]
    [SerializeField] private float spongeProgressRequired = 0.8f;
    [SerializeField] private float wipeProgressRequired = 0.8f;

    [Header("Visuals")]
    [SerializeField] private Renderer windowRenderer;
    // Inspector에서 순서대로 할당: 0=Dirty, 1=Sprayed, 2=Sponged, 3=Rinsed, 4=Clean
    [SerializeField] private Material[] stateMaterials;

    [Header("VFX")]
    [SerializeField] private ParticleSystem foamVFX;
    [SerializeField] private ParticleSystem wetVFX;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spraySound;
    [SerializeField] private AudioClip cleanSound;

    private CleaningState _state = CleaningState.Dirty;
    private float _spongeProgress;
    private float _wipeProgress;

    public CleaningState State => _state;
    public bool IsClean => _state == CleaningState.Clean;
    public float SpongeProgress => _spongeProgress;
    public float WipeProgress => _wipeProgress;

    public event System.Action<Window> OnCleaned;

    private void Start() => RefreshVisual();

    // --- 세정제 분사 ---
    public bool TryApplySpray()
    {
        if (_state != CleaningState.Dirty) return false;
        SetState(CleaningState.Sprayed);
        PlaySound(spraySound);
        return true;
    }

    // --- 스펀지 문지르기 (매 프레임 delta 누적) ---
    public void AddSpongeProgress(float delta)
    {
        if (_state != CleaningState.Sprayed) return;
        _spongeProgress = Mathf.Clamp01(_spongeProgress + delta);
        if (_spongeProgress >= spongeProgressRequired)
            SetState(CleaningState.Sponged);
    }

    // --- 물 분사 ---
    public bool TryApplyWater()
    {
        if (_state != CleaningState.Sponged) return false;
        SetState(CleaningState.Rinsed);
        PlaySound(spraySound);
        return true;
    }

    // --- 마른 천으로 닦기 (매 프레임 delta 누적) ---
    public void AddWipeProgress(float delta)
    {
        if (_state != CleaningState.Rinsed) return;
        _wipeProgress = Mathf.Clamp01(_wipeProgress + delta);
        if (_wipeProgress >= wipeProgressRequired)
        {
            SetState(CleaningState.Clean);
            PlaySound(cleanSound);
            OnCleaned?.Invoke(this);
        }
    }

    // 전체 청소 진행도 (UI 프로그레스바용)
    public float GetOverallProgress()
    {
        return _state switch
        {
            CleaningState.Dirty => 0f,
            CleaningState.Sprayed => 0.25f * _spongeProgress,
            CleaningState.Sponged => 0.5f,
            CleaningState.Rinsed => 0.5f + 0.5f * _wipeProgress,
            CleaningState.Clean => 1f,
            _ => 0f
        };
    }

    private void SetState(CleaningState newState)
    {
        _state = newState;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        int idx = (int)_state;
        if (windowRenderer != null && stateMaterials != null && idx < stateMaterials.Length && stateMaterials[idx] != null)
            windowRenderer.material = stateMaterials[idx];

        if (foamVFX != null)
        {
            if (_state == CleaningState.Sprayed) foamVFX.Play();
            else foamVFX.Stop();
        }

        if (wetVFX != null)
        {
            if (_state == CleaningState.Rinsed) wetVFX.Play();
            else wetVFX.Stop();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
