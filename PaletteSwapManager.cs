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

    [Header("Transition (optional)")]
    [Tooltip("Seconds for a smooth palette lerp. Set to 0 for instant swap.")]
    [SerializeField] private float transitionDuration = 0f;

    public TimeState CurrentState { get; private set; }

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
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
            Debug.Log($"[TimeManager] TAB pressed, transitioning: {_transitioning}");

        if (Keyboard.current[toggleKey].wasPressedThisFrame && !_transitioning)
            BeginSwitch();

        if (_transitioning)
            TickTransition();
    }

    private void BeginSwitch()
    {
        _targetState = CurrentState == TimeState.Day ? TimeState.Night : TimeState.Day;

        if (transitionDuration <= 0f)
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
        _transitionTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_transitionTimer / transitionDuration);

        // Broadcast a normalised progress value so listeners can lerp colours
        OnTransitionProgress?.Invoke(
            CurrentState == TimeState.Day ? t : 1f - t   // 0 = full day, 1 = full night
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
        OnTransitionProgress?.Invoke(newState == TimeState.Night ? 1f : 0f);
        OnTimeSwitched?.Invoke(CurrentState);
    }

    public event Action<float> OnTransitionProgress;

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