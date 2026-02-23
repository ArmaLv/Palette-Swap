using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    public enum TimeState { Day, Night }
    public event Action<TimeState> OnTimeSwitched;

    [Header("Settings")]
    [SerializeField] private TimeState startingState = TimeState.Day;
    [SerializeField] private Key toggleKey = Key.Tab;

    [Header("Transition")]
    [Tooltip("Seconds for transition to night. 0 = instant.")]
    [Range(0f, 10f)] [SerializeField] private float transitionDuration = 0f;
    [Tooltip("Seconds for transition back to day. 0 = instant. -1 = use Transition Duration.")]
    [Range(-1f, 10f)] [SerializeField] private float transitionDurationBack = -1f;

    public TimeState CurrentState { get; private set; }
    public event Action<float> OnTransitionProgress;

    private bool _transitioning = false;
    private float _transitionTimer = 0f;
    private TimeState _targetState;

    private void Awake()
    {
        // Singleton setup — persists across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentState = startingState;
    }

    private void Update()
    {
        if (Keyboard.current[toggleKey].wasPressedThisFrame && !_transitioning)
            BeginSwitch();

        if (_transitioning)
            TickTransition();
    }

    private float GetDuration()
    {
        if (CurrentState == TimeState.Night)
        {
            // Transitioning back to day
            return transitionDurationBack < 0f ? transitionDuration : transitionDurationBack;
        }
        return transitionDuration;
    }

    private void BeginSwitch()
    {
        _targetState = CurrentState == TimeState.Day ? TimeState.Night : TimeState.Day;

        float duration = GetDuration();

        if (duration <= 0f)
        {
            // Instant
            ApplyState(_targetState);
        }
        else
        {
            // Gradual — start ticking
            _transitioning = true;
            _transitionTimer = 0f;
        }
    }

    private void TickTransition()
    {
        float duration = GetDuration();
        _transitionTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_transitionTimer / duration);

        // Broadcast a normalised progress value so listeners can lerp colours
        OnTransitionProgress?.Invoke(
            CurrentState == TimeState.Day ? t : 1f - t  // 0 = full day, 1 = full night
        );

        if (t >= 1f)
        {
            _transitioning = false;
            ApplyState(_targetState);
        }
    }

    private void ApplyState(TimeState newState)
    {
        CurrentState = newState;
        // Snap the blend weight to a clean value after transition completes
        OnTransitionProgress?.Invoke(newState == TimeState.Night ? 1f : 0f);
        OnTimeSwitched?.Invoke(CurrentState);
    }

    public void SetState(TimeState state)
    {
        if (_transitioning) CancelTransition();
        ApplyState(state);
    }

    public bool IsDay   => CurrentState == TimeState.Day;
    public bool IsNight => CurrentState == TimeState.Night;

    private void CancelTransition()
    {
        _transitioning = false;
        _transitionTimer = 0f;
    }
}