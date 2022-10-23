using System.Collections.Generic;
using UnityEngine;
using UnifiedAnalyticsSDK;

public class TestGameManager : MonoBehaviour
{
    private const string LevelName = "Test Level";

    [SerializeField] private Rigidbody2D player;

    [SerializeField] private List<EventTrigger> eventTriggers;

    [SerializeField] private Joystick joystick;

    private Vector3 velocity = Vector3.zero;

    private const float MovementSmoothing = .05f;

    public const float JumpForce = 20;

    private void Awake()
    {
        UnifiedAnalytics.Initialize();

        UnifiedAnalytics.Tracker.SendLevelStartEvent(LevelName);

        foreach (var trigger in this.eventTriggers)
        {
            trigger.EventTriggered += this.OnEventTriggered;
        }
    }

    private void Update()
    {
        this.MovePlayer(joystick.Horizontal, joystick.Vertical > 0.8f);
    }

    private void OnEventTriggered(TestEventType type)
    {
        switch (type)
        {
            case TestEventType.GoalReached:
                UnifiedAnalytics.Tracker.SendLevelCompletedEvent(LevelName, true);
                break;
            case TestEventType.PlayerDied:
                UnifiedAnalytics.Tracker.SendLevelFailedEvent(LevelName, "Player jumped of a cliff!");
                break;
            case TestEventType.EnemyEncountered:
                UnifiedAnalytics.Tracker.SendLevelCompletedEvent(LevelName, false);
                break;
            default:
                Debug.LogWarning($"Not supported event type {type}");
                return;
        }

        Debug.Log($"Sent Event: {type} - level: {LevelName}");
    }

    private void MovePlayer(float move, bool jump)
    {
        var targetVelocity = new Vector2(move * 10, this.player.velocity.y);

        this.player.velocity = Vector3.SmoothDamp(this.player.velocity, targetVelocity, ref this.velocity, MovementSmoothing);

        if (jump)
        {		
            this.player.AddForce(new Vector2(0f, JumpForce));
        }
    }
}