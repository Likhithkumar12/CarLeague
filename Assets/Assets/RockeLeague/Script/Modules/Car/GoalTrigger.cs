using System.Collections;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [Tooltip("Name of the team that scores when ball enters THIS goal.")]
    public string scoringTeamName = "Blue";

    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Ball")) return;

        _triggered = true;
        GoalManager.Instance.OnGoalScored(scoringTeamName);
    }

    public void ResetTrigger() => _triggered = false;
}