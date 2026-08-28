using UnityEngine;
using UnityEngine.AI;

public class MoveToTarget : MonoBehaviour
{
    [Header("Movement")]
    public NavMeshAgent agent;
    public float speed = 3.5f;

    [Header("Default Target")]
    public Transform defaultTarget;
    public string defaultTargetTag = "TownHall";

    [Header("State")]
    public bool IsFollowingManualCommand { get; private set; }
    public bool IsAttackMove { get; private set; }
    public bool IsRetaliating { get; private set; }
    public bool IsStoppedManually { get; private set; }

    public Vector3 ManualDestination { get; private set; }
    public bool HasManualDestination { get; private set; }

    [Header("Performance")]
    public float updateInterval = 0.15f; // How often to update the destination

    private Transform chaseTarget;
    private bool isChasing = false;
    private Attack attack;
    private float nextUpdateTime;

    // ─── Clear retaliation flag (used by Attack) ─────────────────────────
    public void ClearRetaliation()
    {
        IsRetaliating = false;
    }

    public void SetChaseTarget(Transform target)
    {
        chaseTarget = target;
        isChasing = target != null;
        if (isChasing)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            if (HasManualDestination)
                agent.SetDestination(ManualDestination);
            else if (defaultTarget != null)
                agent.SetDestination(defaultTarget.position);
        }
    }

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.speed = speed;

        attack = GetComponent<Attack>();

        if (defaultTarget == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(defaultTargetTag);
            if (obj != null) defaultTarget = obj.transform;
        }

        if (defaultTarget != null)
            SetTarget(defaultTarget);
        else
            IsStoppedManually = true;
    }

    void Update()
    {
        if (agent == null || !agent.enabled) return;

        // ⛔ If we are in combat, the agent is stopped; do NOT override destination.
        if (attack != null && attack.IsInCombat)
            return;

        // ─── 1. Chase target (used by Attack to chase enemies) ────────────
        if (isChasing && chaseTarget != null)
        {
            if (Time.time >= nextUpdateTime)
            {
                agent.SetDestination(chaseTarget.position);
                nextUpdateTime = Time.time + updateInterval;
            }
            return;
        }

        // ─── 2. Manual destination (normal move / attack‑move) ──────────
        if (HasManualDestination)
        {
            if (Time.time >= nextUpdateTime)
            {
                agent.SetDestination(ManualDestination);
                nextUpdateTime = Time.time + updateInterval;
            }
            return;
        }

        // ─── 3. Stopped manually (pressed S) ─────────────────────────────
        if (IsStoppedManually)
            return;

        // ─── 4. Default target ────────────────────────────────────────────
        if (defaultTarget != null)
        {
            if (Time.time >= nextUpdateTime)
            {
                agent.SetDestination(defaultTarget.position);
                nextUpdateTime = Time.time + updateInterval;
            }
        }
    }

    // ─── Commands ──────────────────────────────────────────────────────────

    public void SetDestination(Vector3 worldPos)
    {
        ManualDestination = worldPos;
        HasManualDestination = true;
        IsFollowingManualCommand = true;
        IsAttackMove = false;
        IsRetaliating = false;
        IsStoppedManually = false;
        SetChaseTarget(null);
        agent.SetDestination(worldPos);
        agent.isStopped = false;
        nextUpdateTime = Time.time + updateInterval; // Reset timer
    }

    public void SetAttackMove(Vector3 worldPos)
    {
        ManualDestination = worldPos;
        HasManualDestination = true;
        IsFollowingManualCommand = true;
        IsAttackMove = true;
        IsRetaliating = false;
        IsStoppedManually = false;
        SetChaseTarget(null);
        agent.SetDestination(worldPos);
        agent.isStopped = false;
        nextUpdateTime = Time.time + updateInterval;
    }

    public void SetTarget(Transform target)
    {
        if (target == null) return;
        defaultTarget = target;
        IsFollowingManualCommand = false;
        IsAttackMove = false;
        IsRetaliating = false;
        IsStoppedManually = false;
        HasManualDestination = false;
        SetChaseTarget(null);
        agent.SetDestination(target.position);
        agent.isStopped = false;
        nextUpdateTime = Time.time + updateInterval;
    }

    public void StopMoving()
    {
        HasManualDestination = false;
        IsFollowingManualCommand = false;
        IsAttackMove = false;
        IsRetaliating = false;
        IsStoppedManually = true;
        SetChaseTarget(null);
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // ─── Retaliation (called by Attack) ─────────────────────────────────
    public void Retaliate(Transform attacker)
    {
        if (attacker == null) return;
        IsRetaliating = true;
        IsFollowingManualCommand = true;
        IsAttackMove = false;
        IsStoppedManually = false;
        HasManualDestination = false;
        SetChaseTarget(attacker);
        agent.SetDestination(attacker.position);
        agent.isStopped = false;
        nextUpdateTime = Time.time + updateInterval;
    }

    public bool IsMoving() => agent != null && agent.velocity.sqrMagnitude > 0.01f;
    public void SetSpeed(float newSpeed) { speed = newSpeed; if (agent != null) agent.speed = speed; }
}