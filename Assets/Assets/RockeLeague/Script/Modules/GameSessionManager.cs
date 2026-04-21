using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

/// <summary>
/// Persists across scenes. Stores pre-game choices:
/// selected car index per player and match mode (1v1 / 2v2).
/// Also manages the gameplay timer.
/// </summary>
public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [Header("Defaults")]
    [SerializeField] private MatchMode defaultMatchMode = MatchMode.OneVsOne;
    
    [Header("Game Timer")]
    [SerializeField] private float matchDurationMinutes = 10f;

    public MatchMode CurrentMatchMode { get; private set; }

    // Local player's car selection (set before joining/hosting)
    public int LocalCarIndex { get; private set; } = 0;

    // Server-side: car index per PlayerRef (populated when players join)
    private readonly Dictionary<PlayerRef, int> playerCarMap = new();

    // Timer
    private float _matchTimeRemaining;
    private bool _timerRunning = false;
    
    public float MatchTimeRemaining => _matchTimeRemaining;
    public bool IsTimerRunning => _timerRunning;
    
    public event Action OnTimerExpired;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentMatchMode = defaultMatchMode;
    }

    void Update()
    {
        if (_timerRunning)
        {
            _matchTimeRemaining -= Time.deltaTime;
            
            if (_matchTimeRemaining <= 0)
            {
                _matchTimeRemaining = 0;
                _timerRunning = false;
                OnTimerExpired?.Invoke();
                Debug.Log("[GameSession] Timer expired!");
            }
        }
    }

    // ─── Called from UI ───────────────────────────────────────────────────────

    public void SetMatchMode(MatchMode mode)
    {
        CurrentMatchMode = mode;
        Debug.Log($"[GameSession] Match mode set to: {mode}");
    }

    public void SetLocalCarIndex(int index)
    {
        LocalCarIndex = index;
        Debug.Log($"[GameSession] Local car index: {index}");
    }

    // ─── Timer Control ────────────────────────────────────────────────────────

    public void StartTimer()
    {
        _matchTimeRemaining = matchDurationMinutes * 60f;
        _timerRunning = true;
        Debug.Log($"[GameSession] Timer started: {matchDurationMinutes} minutes");
    }

    public void StopTimer()
    {
        _timerRunning = false;
        Debug.Log("[GameSession] Timer stopped");
    }
    #if UNITY_EDITOR
        [ContextMenu("Debug / Finish Match Now")]
    #endif
        public void DebugFinishMatch()
        {
            Debug.LogWarning("[GameSession] DEBUG: DebugFinishMatch() called — forcing match end.");
            ExpireTimer();
        }
     
        // ─── Internal timer expiry (single source of truth) ───────────────────────
     
        private void ExpireTimer()
        {
            _matchTimeRemaining = 0;
            _timerRunning = false;
            OnTimerExpired?.Invoke();
            Debug.Log("[GameSession] Timer expired!");
        }


    public void ResetTimer()
    {
        _matchTimeRemaining = matchDurationMinutes * 60f;
        _timerRunning = false;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(_matchTimeRemaining / 60f);
        int seconds = Mathf.FloorToInt(_matchTimeRemaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    // ─── Called from NetworkManager (server only) ─────────────────────────────

    public void RegisterPlayerCar(PlayerRef player, int carIndex)
    {
        playerCarMap[player] = carIndex;
    }

    public int GetCarIndex(PlayerRef player)
    {
        return playerCarMap.TryGetValue(player, out int idx) ? idx : 0;
    }

    public void ClearSession()
    {
        playerCarMap.Clear();
        LocalCarIndex = 0;
        CurrentMatchMode = defaultMatchMode;
        ResetTimer();
    }
}
