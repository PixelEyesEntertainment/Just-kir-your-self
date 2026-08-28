using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Events;

public class Attack : MonoBehaviour
{
    [Header("Combat")]
    public int damage = 10;
    public float attackCooldown = 1f;
    public float attackStartupDelay = 0.2f;
    public float rotationSpeed = 5f;

    [Header("Ranges")]
    public float detectionRange = 10f;
    public float attackRange = 2f;

    public LayerMask enemyLayers = ~0;

    [Header("Projectile (optional)")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 10f;
    public float projectileArcHeight = 2f;
    public bool rotateToTrajectory = true;

    [Header("Events")]
    public UnityEvent onAttack;
    public UnityEvent onAttackStart;
    public UnityEvent onAttackMiss;

    [Header("Animation")]
    public Animator animator;
    public string isAttackingParam = "isAttacking";

    [Header("Performance")]
    public float scanInterval = 0.15f;

    [Header("Debug")]
    public bool showDebugLogs = false;

    public bool IsInCombat { get; private set; }

    private NavMeshAgent agent;
    private MoveToTarget mover;
    private HealthSystem myHealth;
    private Transform currentTarget;
    private Collider targetCollider;   // used for ClosestPoint
    private float lastAttackTime;
    private bool isAttacking;
    private Collider[] overlapBuffer = new Collider[20];
    private bool agentAutoRotate;
    private float nextScanTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
        mover = GetComponent<MoveToTarget>(); // optional
        myHealth = GetComponent<HealthSystem>();

        if (animator == null) animator = GetComponent<Animator>();

        if (agent != null)
            agentAutoRotate = agent.updateRotation;

        if (myHealth != null)
            myHealth.onTakeDamage.AddListener(OnGotHit);
    }

    void Update()
    {
        // ─── Scan every `scanInterval` seconds ──────────────────────────
        if (Time.time < nextScanTime)
        {
            if (currentTarget != null && IsInCombat)
                FaceTarget(currentTarget);
            return;
        }
        nextScanTime = Time.time + scanInterval;

        // ─── 1. Scan for enemies ──────────────────────────────────────────
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, overlapBuffer, enemyLayers);
        bool hasEnemy = false;
        Transform closest = null;
        Collider closestCollider = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];
            if (col == null || col.transform == transform) continue;

            HealthSystem health = col.GetComponentInChildren<HealthSystem>();
            if (health == null || health.IsDead()) continue;

            // ✅ Use ClosestPoint to get the distance to the surface of the collider
            Vector3 closestPoint = col.ClosestPoint(transform.position);
            float dist = Vector3.Distance(transform.position, closestPoint);

            if (dist < minDist)
            {
                minDist = dist;
                closest = col.transform;
                closestCollider = col;
            }
            hasEnemy = true;
        }

        if (showDebugLogs)
        {
            string moverInfo = mover != null ?
                $"IsAttackMove={mover.IsAttackMove}, IsRetaliating={mover.IsRetaliating}, IsFollowingManual={mover.IsFollowingManualCommand}, IsStopped={mover.IsStoppedManually}, IsMoving={mover.IsMoving()}"
                : "no MoveToTarget";
            Debug.Log($"[Attack] {gameObject.name}: hasEnemy={hasEnemy}, {moverInfo}");
        }

        // ─── 2. Determine if we should react ─────────────────────────────
        bool shouldReact;

        if (mover == null)
        {
            // Tower (no mover) – always react
            shouldReact = true;
        }
        else
        {
            bool isNormalMove = mover.IsFollowingManualCommand && !mover.IsAttackMove;
            bool isManuallyStopped = mover.IsStoppedManually;

            if (isNormalMove || isManuallyStopped)
                shouldReact = mover.IsRetaliating;
            else
                shouldReact = true;
        }

        if (!shouldReact)
        {
            currentTarget = null;
            targetCollider = null;
            IsInCombat = false;
            SetAttackAnimation(false);
            if (mover != null) mover.SetChaseTarget(null);
            if (agent != null)
            {
                agent.isStopped = false;
                if (!agent.updateRotation)
                    agent.updateRotation = agentAutoRotate;
            }
            return;
        }

        // ─── 3. React to enemies ──────────────────────────────────────────
        if (hasEnemy && closest != null)
        {
            currentTarget = closest;
            targetCollider = closestCollider;

            // Distance to the surface of the target's collider
            Vector3 closestPoint = targetCollider.ClosestPoint(transform.position);
            float distToSurface = Vector3.Distance(transform.position, closestPoint);

            if (distToSurface <= attackRange)
            {
                // ✅ In attack range – stop and attack
                IsInCombat = true;
                if (agent != null)
                {
                    agent.isStopped = true;
                    if (agent.updateRotation)
                        agent.updateRotation = false;
                }
                if (mover != null) mover.SetChaseTarget(null);
                if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
                {
                    HealthSystem health = currentTarget.GetComponentInChildren<HealthSystem>();
                    if (health != null && !health.IsDead())
                    {
                        StartCoroutine(PerformAttack(currentTarget));
                    }
                    else
                    {
                        currentTarget = null;
                        targetCollider = null;
                        IsInCombat = false;
                        SetAttackAnimation(false);
                        if (agent != null)
                        {
                            agent.isStopped = false;
                            if (!agent.updateRotation)
                                agent.updateRotation = agentAutoRotate;
                        }
                    }
                }
            }
            else
            {
                // ❌ Outside attack range – chase (only if mover exists and not manually stopped)
                if (mover != null && !mover.IsStoppedManually && agent != null)
                {
                    IsInCombat = false;
                    SetAttackAnimation(false);
                    if (agent.updateRotation)
                        agent.updateRotation = false;
                    mover.SetChaseTarget(currentTarget);
                    agent.isStopped = false;
                }
                else if (mover == null)
                {
                    // Tower – cannot chase, just wait
                    IsInCombat = false;
                }
            }
        }
        else
        {
            // ─── No enemy detected ──────────────────────────────────────────
            currentTarget = null;
            targetCollider = null;
            IsInCombat = false;
            SetAttackAnimation(false);
            if (mover != null) mover.SetChaseTarget(null);
            if (agent != null)
            {
                agent.isStopped = false;
                if (!agent.updateRotation)
                    agent.updateRotation = agentAutoRotate;
            }

            if (mover != null && mover.IsAttackMove && mover.HasManualDestination)
            {
                agent?.SetDestination(mover.ManualDestination);
            }
            else if (mover != null && mover.IsRetaliating)
            {
                mover.ClearRetaliation();
                if (mover.IsFollowingManualCommand && !mover.IsAttackMove && mover.HasManualDestination)
                {
                    agent?.SetDestination(mover.ManualDestination);
                }
            }
        }

        // ─── Face target if in combat ──────────────────────────────────────
        if (currentTarget != null && shouldReact)
        {
            FaceTarget(currentTarget);
        }
    }

    // ─── Face target ──────────────────────────────────────────────────────
    private void FaceTarget(Transform target)
    {
        if (target == null) return;

        if (agent != null && agent.updateRotation)
            agent.updateRotation = false;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // ─── Animation helper ──────────────────────────────────────────────────
    private void SetAttackAnimation(bool attacking)
    {
        if (animator != null)
            animator.SetBool(isAttackingParam, attacking);
    }

    // ─── Attack coroutine ─────────────────────────────────────────────────

    private IEnumerator PerformAttack(Transform target)
    {
        isAttacking = true;
        SetAttackAnimation(true);
        onAttackStart.Invoke();

        yield return new WaitForSeconds(attackStartupDelay);

        if (target == null)
        {
            isAttacking = false;
            SetAttackAnimation(false);
            yield break;
        }

        HealthSystem health = target.GetComponentInChildren<HealthSystem>();
        if (health == null || health.IsDead())
        {
            isAttacking = false;
            SetAttackAnimation(false);
            yield break;
        }

        if (projectilePrefab == null)
        {
            health.TakeDamage(damage);
            onAttack.Invoke();
        }
        else
        {
            FireProjectile(target);
            onAttack.Invoke();
        }

        lastAttackTime = Time.time;
        isAttacking = false;
        SetAttackAnimation(false);
    }

    // ─── Auto‑retaliation ─────────────────────────────────────────────────

    private void OnGotHit()
    {
        if (mover != null && !mover.IsRetaliating && !mover.IsAttackMove)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, overlapBuffer, enemyLayers);
            Transform closest = null;
            Collider closestCollider = null;
            float minDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider col = overlapBuffer[i];
                if (col == null || col.transform == transform) continue;
                HealthSystem health = col.GetComponentInChildren<HealthSystem>();
                if (health == null || health.IsDead()) continue;
                Vector3 closestPoint = col.ClosestPoint(transform.position);
                float dist = Vector3.Distance(transform.position, closestPoint);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = col.transform;
                    closestCollider = col;
                }
            }
            if (closest != null)
            {
                mover.Retaliate(closest);
                currentTarget = closest;
                targetCollider = closestCollider;
            }
        }
    }

    // ─── Projectile logic ─────────────────────────────────────────────────

    private void FireProjectile(Transform target)
    {
        if (projectilePrefab == null) return;
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        StartCoroutine(MoveProjectile(proj, target));
    }

    private IEnumerator MoveProjectile(GameObject proj, Transform target)
    {
        Vector3 start = proj.transform.position;
        Vector3 end = target.position;
        float journey = Vector3.Distance(start, end);
        float duration = journey / projectileSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null)
            {
                Destroy(proj);
                onAttackMiss.Invoke();
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 pos = Vector3.Lerp(start, end, t);
            float height = projectileArcHeight * 4f * t * (1f - t);
            pos.y += height;
            proj.transform.position = pos;

            if (rotateToTrajectory)
            {
                Vector3 dir = (end - start) + Vector3.up * projectileArcHeight * 4f * (1f - 2f * t);
                if (dir != Vector3.zero)
                    proj.transform.forward = dir.normalized;
            }

            yield return null;
        }

        if (target != null)
        {
            HealthSystem h = target.GetComponentInChildren<HealthSystem>();
            if (h != null && !h.IsDead())
                h.TakeDamage(damage);
        }
        Destroy(proj);
    }

    // ─── Public setters ──────────────────────────────────────────────────

    public void SetDamage(int val) => damage = val;
    public void SetCooldown(float val) => attackCooldown = val;
    public void SetStartupDelay(float val) => attackStartupDelay = val;
    public void SetDetectionRange(float val) => detectionRange = val;
    public void SetAttackRange(float val) => attackRange = val;
    public void SetEnemyLayers(LayerMask mask) => enemyLayers = mask;
    public void SetProjectileSpeed(float val) => projectileSpeed = val;
    public void SetProjectileArcHeight(float val) => projectileArcHeight = val;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, detectionRange);
        Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, attackRange);
        Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}