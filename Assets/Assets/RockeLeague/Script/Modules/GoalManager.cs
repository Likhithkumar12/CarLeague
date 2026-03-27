using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Single source of truth for scores and goal events.
/// Uses a single TextMeshPro for score display and goal announcements.
/// </summary>
public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI txtDisplay;        // Single text for score, goals, and timer

    [Header("Reset")]
    public Transform ballTransform;
    public Vector3   ballResetPosition = Vector3.zero;
    public GoalTrigger[] goalTriggers;        // drag both RedGoal and BlueGoal triggers
    

    [Header("Game Settings")]
    public int goalLimit = 5;
    public float goalDisplayTime = 3f;

    private int  _redScore  = 0;
    private int  _blueScore = 0;
    private bool _matchOver = false;
    private bool _showingGoal = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Start the timer when gameplay begins
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.StartTimer();
            GameSessionManager.Instance.OnTimerExpired += OnTimerExpired;
        }

        RefreshDisplay();
    }

    void OnDestroy()
    {
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.OnTimerExpired -= OnTimerExpired;
        }
    }

    void Update()
    {
        if (!_matchOver && !_showingGoal)
        {
            RefreshDisplay();
        }
    }

    // ── Called by GoalTrigger ─────────────────────────────────────────────────

    public void OnGoalScored(string scoringTeam)
    {
        if (_matchOver) return;

        if (scoringTeam == "Red")  _redScore++;
        else                       _blueScore++;

        StartCoroutine(GoalSequence(scoringTeam));
    }

    // ── Sequence: show goal UI → reset ball → resume ──────────────────────────

    private IEnumerator GoalSequence(string scoringTeam)
    {
        _showingGoal = true;
        
        // 2. Show goal announcement
        if (txtDisplay != null)
        {
            txtDisplay.text = $"<size=120><color=yellow>GOAL!</color></size>\n<size=60>{scoringTeam.ToUpper()} SCORES!</size>";
            txtDisplay.fontSize = 80;
        }

        yield return new WaitForSeconds(goalDisplayTime);

        _showingGoal = false;

        // 3. Check win condition
        if (_redScore >= goalLimit || _blueScore >= goalLimit)
        {
            ShowResult(scoringTeam);
            yield break;
        }

        // 4. Reset ball
        ResetBall();

        // 5. Reset goal triggers
        foreach (var gt in goalTriggers)
            gt?.ResetTrigger();
        // 7. Resume normal display
        RefreshDisplay();
    }
    // ── Timer Callback ────────────────────────────────────────────────────────

    private void OnTimerExpired()
    {
        if (_matchOver) return;

        // Determine winner based on score
        string winner;
        if (_redScore > _blueScore)
            winner = "Red";
        else if (_blueScore > _redScore)
            winner = "Blue";
        else
            winner = "Draw";

        ShowResult(winner);
    }

    // ── Display Helpers ───────────────────────────────────────────────────────

    private void RefreshDisplay()
    {
        if (txtDisplay == null) return;

        string timer = GameSessionManager.Instance != null 
            ? GameSessionManager.Instance.GetFormattedTime() 
            : "00:00";

        txtDisplay.fontSize = 48;
        txtDisplay.text = $"<color=red>RED {_redScore}</color>  - <color=blue>BLUE {_blueScore}</color>\n<size=36>Time: {timer}</size>";
    }

    private void ResetBall()
    {
        if (ballTransform == null) return;

        var rb = ballTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        ballTransform.position = ballResetPosition;
        ballTransform.rotation = Quaternion.identity;
    }

    private void ShowResult(string winner)
    {
        _matchOver = true;

        // Stop the timer
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.StopTimer();
        }

        // Show result screen via UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowResultScreen(_redScore, _blueScore, winner);
        }
        else
        {
            // Fallback: display on main text
            if (txtDisplay != null)
            {
                txtDisplay.fontSize = 80;
                if (winner == "Draw")
                {
                    txtDisplay.text = $"<color=yellow>DRAW!</color>\n<size=48>RED {_redScore} - BLUE {_blueScore}</size>";
                }
                else
                {
                    txtDisplay.text = $"<color=yellow>{winner.ToUpper()} WINS!</color>\n<size=48>RED {_redScore} - BLUE {_blueScore}</size>";
                }
            }
        }
    }
}
