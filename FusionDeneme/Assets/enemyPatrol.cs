/*
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;            // Tüm waypoint'ler (merdiven basamaklarý da dahil)
    [Tooltip("Baþlangýç indeksi (0-based) of the stairs in the waypoints array")]
    public int stairsStartIndex = -1;
    [Tooltip("Bitiþ indeksi (0-based) of the stairs in the waypoints array")]
    public int stairsEndIndex = -1;
    [Tooltip("Hangi waypoint'e dönsün (index) merdiveni tamamlayýnca")]
    public int returnWaypointAfterStairs = 0;

    [Header("Agent settings")]
    public float waitTimeAtWaypoint = 1.2f;
    public float waypointReachedThreshold = 0.4f; // navmesh remainingDistance threshold

    private NavMeshAgent agent;
    private int currentWaypoint = 0;
    private bool waiting = false;
    private bool stairsCompleted = false;     // merdiven tamamlandýktan sonra tekrar merdivene girilmesini engeller
    private float lastStairsExitTime = -999f;
    public float stairsCooldown = 8f;         // merdiven tamamlandýktan sonra tekrar merdivene girmesin (s)

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Waypoints not set on EnemyPatrol.");
            enabled = false;
            return;
        }

        // Güvenlik: index sýnýrlarýný kontrol et
        if (stairsStartIndex >= 0 && stairsEndIndex >= stairsStartIndex)
        {
            stairsStartIndex = Mathf.Clamp(stairsStartIndex, 0, waypoints.Length - 1);
            stairsEndIndex = Mathf.Clamp(stairsEndIndex, 0, waypoints.Length - 1);
        }
        currentWaypoint = 0;
        SetDestinationToCurrent();
    }

    void Update()
    {
        if (agent.pathPending || waiting) return;

        // Eðer agent zaten bir OffMeshLink üzerindese, özel traversal yap
        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(TraverseOffMeshLink());
            return;
        }

        // Kararlý waypoint varýþ kontrolü: remainingDistance kullan, pathPending göz önünde
        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, waypointReachedThreshold))
        {
            // Bazen remainingDistance çok küçük negatif gelebilir. Yine de gelindi kabul et.
            OnArrivedAtWaypoint();
        }
    }

    private void OnArrivedAtWaypoint()
    {
        // Eðer bu waypoint merdivenin son basamaðý ise -> merdiven tamamlandý
        if (IsInStairsRange(currentWaypoint))
        {
            if (currentWaypoint == stairsEndIndex)
            {
                stairsCompleted = true;
                lastStairsExitTime = Time.time;
                // Merdivenden çýktýktan sonra ana devriye waypoint'ine atla
                currentWaypoint = Mathf.Clamp(returnWaypointAfterStairs, 0, waypoints.Length - 1);
                StartCoroutine(WaitAndMove(waitTimeAtWaypoint));
                return;
            }
            else
            {
                // Merdivenin ara basamaðý: devam et (bir sonraki stair waypoint'ine)
                currentWaypoint = Mathf.Min(currentWaypoint + 1, waypoints.Length - 1);
                StartCoroutine(WaitAndMove(waitTimeAtWaypoint));
                return;
            }
        }

        // Normal (merdiven deðil) waypoint davranýþý
        // Eðer merdiven aralýðý varsa ve merdivene tekrar girmeye uygunsa, kontrol et
        if (stairsStartIndex >= 0 && !IsInStairsRange(currentWaypoint))
        {
            // Eðer merdivene girme koþulu (örn. sýradaki waypoint merdiven baþlangýcýysa ve cooldown dolduysa)
            if (currentWaypoint + 1 == stairsStartIndex && (Time.time - lastStairsExitTime) > stairsCooldown)
            {
                // next will be first stair
                currentWaypoint = stairsStartIndex;
                StartCoroutine(WaitAndMove(waitTimeAtWaypoint));
                return;
            }
        }

        // Normal döngü: sonraki waypoint'e geç
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        StartCoroutine(WaitAndMove(waitTimeAtWaypoint));
    }

    private IEnumerator WaitAndMove(float wait)
    {
        waiting = true;
        // animasyon vb. eklemek istersen burada tetikleyebilirsin
        yield return new WaitForSeconds(wait);
        waiting = false;
        SetDestinationToCurrent();
    }

    private void SetDestinationToCurrent()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        agent.SetDestination(waypoints[currentWaypoint].position);
    }

    private bool IsInStairsRange(int index)
    {
        if (stairsStartIndex < 0 || stairsEndIndex < stairsStartIndex) return false;
        return index >= stairsStartIndex && index <= stairsEndIndex;
    }

    private IEnumerator TraverseOffMeshLink()
    {
        // Eðer OffMeshLink kullanýlýyorsa daha düzgün geçiþ için buradan kontrol et
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 start = agent.transform.position;
        Vector3 end = data.endPos + Vector3.up * agent.baseOffset;

        float duration = 0.6f; // isteðe göre arttýr / azalt
        float elapsed = 0f;

        // Basit Lerp traversal; istersen animasyon ile senkronize edebilirsin
        while (elapsed < duration)
        {
            agent.transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.transform.position = end;
        agent.CompleteOffMeshLink();
        // OffMeshLink tamamlandýktan sonra hedefe yönel
        SetDestinationToCurrent();
    }

    // Yardýmcý debug fonksiyonu: inspector'da görünürse kullanýlabilir
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);
            if (i < waypoints.Length - 1)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        if (stairsStartIndex >= 0 && stairsEndIndex >= stairsStartIndex && waypoints.Length > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = stairsStartIndex; i <= stairsEndIndex && i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawCube(waypoints[i].position + Vector3.up * 0.05f, Vector3.one * 0.2f);
            }
        }
    }
}
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))] // AudioSource bileþenini otomatik ekler
public class AdvancedEnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Tired, Attacking }

    [Header("Current State")]
    public EnemyState currentState = EnemyState.Patrol;
    [SerializeField] private Transform currentTarget;

    [Header("Audio Settings")] // YENÝ EKLENEN KISIM
    public AudioClip rageSound; // Buraya Boss_Roar.mp3'ü sürükleyeceksin
    private AudioSource audioSource;

    [Header("Detection & Co-op Settings")]
    public LayerMask playerLayer;
    public float detectionRadius = 5f;
    public int itemCollected = 2;

    private List<Transform> playersInZone = new List<Transform>();
    private SphereCollider detectionCollider;

    [Header("Movement Speeds")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 7.0f;
    public float tiredSpeed = 2.0f;

    [Header("Combat Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    [Header("Stamina System")]
    public float maxChaseDuration = 5f;
    public float tiredRecoveryDuration = 3f;
    private float currentChaseTimer = 0f;
    private float currentTiredTimer = 0f;

    [Header("Waypoints")]
    public Transform[] waypoints;
    public int stairsStartIndex = -1;
    public int stairsEndIndex = -1;
    public int returnWaypointAfterStairs = 0;
    public float waitTimeAtWaypoint = 2.0f;

    // --- Private Variables ---
    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypoint = 0;
    private bool waiting = false;
    private Coroutine patrolCoroutine;

    // Merdiven
    private bool stairsCompleted = false;
    private float lastStairsExitTime = -999f;
    public float stairsCooldown = 8f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        detectionCollider = GetComponent<SphereCollider>();
        detectionCollider.isTrigger = true;

        // AudioSource bileþenini al
        audioSource = GetComponent<AudioSource>();

        UpdateDetectionRadius();

        if (waypoints != null && waypoints.Length > 0)
        {
            agent.speed = patrolSpeed;
            SetDestinationToCurrent();
        }
    }

    void Update()
    {
        // Speed parametresini sürekli güncelle (Blend Tree için kritik)
        animator.SetFloat("Speed", agent.velocity.magnitude);

        UpdateDetectionRadius();

        if (currentTarget == null && currentState != EnemyState.Patrol)
        {
            ReturnToPatrol();
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolLogic();
                break;

            case EnemyState.Chase:
                ChaseLogic();
                break;

            case EnemyState.Tired:
                TiredLogic();
                break;

            case EnemyState.Attacking:
                if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) > attackRange)
                {
                    currentState = EnemyState.Chase;
                    agent.isStopped = false;
                }
                break;
        }
    }

    // --- LOGIC: KOVALAMA & YORULMA ---

    void ChaseLogic()
    {
        if (currentTarget == null) return;

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(currentTarget.position);

        currentChaseTimer += Time.deltaTime;
        if (currentChaseTimer >= maxChaseDuration)
        {
            SwitchToTired();
            return;
        }

        if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            PerformAttack();
        }
    }

    void TiredLogic()
    {
        if (currentTarget == null) return;

        agent.isStopped = false;
        agent.speed = tiredSpeed;
        agent.SetDestination(currentTarget.position);

        currentTiredTimer += Time.deltaTime;
        if (currentTiredTimer >= tiredRecoveryDuration)
        {
            currentState = EnemyState.Chase;
            currentChaseTimer = 0f;
            agent.speed = chaseSpeed;
            animator.SetBool("isTired", false);
        }

        if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            PerformAttack();
        }
    }

    void SwitchToTired()
    {
        currentState = EnemyState.Tired;
        currentTiredTimer = 0f;
        agent.speed = tiredSpeed;
        animator.SetBool("isTired", true);
    }

    // --- LOGIC: SALDIRI ---

    void PerformAttack()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            currentState = EnemyState.Attacking;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            transform.LookAt(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));

            animator.SetTrigger("Attack");

            lastAttackTime = Time.time;
        }
    }

    // --- HEDEF SEÇÝMÝ ---
    void SelectBestTarget()
    {
        if (playersInZone.Count == 0)
        {
            currentTarget = null;
            return;
        }

        Transform bestTarget = null;
        float lowestSanity = 9999f;

        foreach (Transform potentialPlayer in playersInZone)
        {
            if (potentialPlayer == null) continue;

            float dist = Vector3.Distance(transform.position, potentialPlayer.position);
            if (dist < lowestSanity)
            {
                lowestSanity = dist;
                bestTarget = potentialPlayer;
            }
        }

        currentTarget = bestTarget;

        if (currentTarget != null)
        {
            StartChase();
        }
    }

    // --- TRIGGER YÖNETÝMÝ ---
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (!playersInZone.Contains(other.transform))
                playersInZone.Add(other.transform);

            SelectBestTarget();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (playersInZone.Contains(other.transform))
                playersInZone.Remove(other.transform);

            if (currentTarget == other.transform)
            {
                if (playersInZone.Count > 0) SelectBestTarget();
                else ReturnToPatrol();
            }
        }
    }

    void StartChase()
    {
        if (currentState == EnemyState.Patrol)
        {
            currentChaseTimer = 0f;
            animator.SetBool("isRaging", false);
            if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);
            waiting = false;
        }

        if (currentState != EnemyState.Tired)
        {
            currentState = EnemyState.Chase;
            agent.speed = chaseSpeed;
        }
    }

    void ReturnToPatrol()
    {
        currentState = EnemyState.Patrol;
        currentTarget = null;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        animator.SetBool("isTired", false);
        SetDestinationToCurrent();
    }

    public void UpdateDetectionRadius()
    {
        if (detectionCollider == null) return;
        detectionCollider.radius = itemCollected switch
        {
            1 => 3f,
            2 => 5f,
            3 => 8f,
            4 => 12f,
            5 => 16f,
            6 => 25f,
            _ => 5f
        };
    }

    // --- DEVRÝYE VE WAYPOINT MANTIÐI ---
    void PatrolLogic()
    {
        if (agent.pathPending || waiting) return;

        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(TraverseOffMeshLink());
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            OnArrivedAtWaypoint();
    }

    private void OnArrivedAtWaypoint()
    {
        if (IsInStairsRange(currentWaypoint))
        {
            if (currentWaypoint == stairsEndIndex)
            {
                stairsCompleted = true;
                lastStairsExitTime = Time.time;
                currentWaypoint = Mathf.Clamp(returnWaypointAfterStairs, 0, waypoints.Length - 1);
            }
            else
            {
                currentWaypoint = Mathf.Min(currentWaypoint + 1, waypoints.Length - 1);
            }
            patrolCoroutine = StartCoroutine(WaitAndMove(0.5f, false));
        }
        else
        {
            if (stairsStartIndex >= 0 && !IsInStairsRange(currentWaypoint))
            {
                if (currentWaypoint + 1 == stairsStartIndex && (Time.time - lastStairsExitTime) > stairsCooldown)
                {
                    currentWaypoint = stairsStartIndex;
                    patrolCoroutine = StartCoroutine(WaitAndMove(waitTimeAtWaypoint, true));
                    return;
                }
            }

            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            patrolCoroutine = StartCoroutine(WaitAndMove(waitTimeAtWaypoint, true));
        }
    }

    private IEnumerator WaitAndMove(float wait, bool triggerRage)
    {
        waiting = true;
        agent.isStopped = true;

        // RAGE ANÝMASYONU VE SESÝ
        if (triggerRage)
        {
            animator.SetBool("isRaging", true);

            // --- YENÝ EKLENEN SES KODU ---
            if (audioSource != null && rageSound != null)
            {
                audioSource.PlayOneShot(rageSound);
            }
        }

        yield return new WaitForSeconds(wait);

        if (triggerRage) animator.SetBool("isRaging", false);

        waiting = false;

        if (currentState == EnemyState.Patrol)
        {
            agent.isStopped = false;
            SetDestinationToCurrent();
        }
    }

    private void SetDestinationToCurrent()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        agent.SetDestination(waypoints[currentWaypoint].position);
    }

    private bool IsInStairsRange(int index)
    {
        if (stairsStartIndex < 0 || stairsEndIndex < stairsStartIndex) return false;
        return index >= stairsStartIndex && index <= stairsEndIndex;
    }

    private IEnumerator TraverseOffMeshLink()
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 end = data.endPos + Vector3.up * agent.baseOffset;
        agent.CompleteOffMeshLink();
        agent.transform.position = end;
        yield return null;
    }
}