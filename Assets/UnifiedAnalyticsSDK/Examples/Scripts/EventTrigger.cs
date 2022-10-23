using System;
using UnityEngine;

public enum TestEventType
{
    GoalReached,
    PlayerDied,
    EnemyEncountered
}

public class EventTrigger : MonoBehaviour
{
    [SerializeField] private TestEventType type;
    
    public TestEventType Type => this.type;

    public event Action<TestEventType> EventTriggered;

    private bool triggered;

    private void Awake()
    {
        this.triggered = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (this.triggered)
        {
            return;
        }

        // we should only trigger once.
        this.EventTriggered?.Invoke(this.type);
        this.triggered = true;
    }
}