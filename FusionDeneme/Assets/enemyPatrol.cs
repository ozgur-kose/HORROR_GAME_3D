

/*
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    public Transform[] waypoints;        // Sahnedeki waypoint Transform'larý
    public float waitTime = 2f;          // Her waypoint'te bekleme süresi
    private int currentWaypoint = 0;     // Þu an hedeflenen waypoint

    private NavMeshAgent agent;          // NavMeshAgent
    private bool isWaiting = false;      // Coroutine kontrolü
    public Animator animator;            // Animator
    private bool isRagging = false;       // Rage durumu    

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null) Debug.LogError("NavMeshAgent eksik!");
        if (animator == null) Debug.LogError("Animator eksik!");
        if (waypoints == null || waypoints.Length == 0) Debug.LogError("Waypoints eksik!");
        else MoveToWaypoint(currentWaypoint);
    }

    void Update()
    {
        // Link kontrolü (merdiven geçiþleri için)
        if (agent.isOnOffMeshLink)
        {
            Debug.Log("LINKE GÝRDÝ - MERDÝVEN GEÇÝÞÝ BAÞLADI!");
        }

        // Waypointe ulaþýldýysa coroutine baþlat
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) &&
            !isWaiting)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    void MoveToWaypoint(int index)
    {
        if (waypoints.Length == 0) return; // güvenlik

        index = Mathf.Clamp(index, 0, waypoints.Length - 1); // sýnýr kontrolü
        NavMeshHit hit;

        if (NavMesh.SamplePosition(waypoints[index].position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log("Hedef: " + index + " pozisyon: " + hit.position);
        }
        else
        {
            Debug.LogWarning("Waypoint " + index + " NavMesh üzerinde deðil!");
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        isRagging = true;

        // Animator parametresi isRagging ise burayý kullanalým
        if (animator != null)
        {
            animator.SetBool("Rage", true);
        }

        yield return new WaitForSeconds(waitTime);

        // Sonraki waypoint
        currentWaypoint++;
        if (currentWaypoint >= waypoints.Length)
            currentWaypoint = 0; // Döngüsel patrol için

        agent.isStopped = false;
        MoveToWaypoint(currentWaypoint);

        // Rage bitince animasyonu kapat
        if (animator != null)
        {
            animator.SetBool("Rage", false);
        }

        isWaiting = false;
    }
}
*/


/*
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Waypoints & Patrol Settings")]
    public Transform[] waypoints;        // Waypointler
    public float waitTime = 2f;          // Her waypoint'te bekleme süresi
    private int currentWaypoint = 0;

    [Header("Components")]
    private NavMeshAgent agent;
    public Animator animator;

    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null) Debug.LogError("NavMeshAgent eksik!");
        if (animator == null) Debug.LogError("Animator eksik!");
        if (waypoints == null || waypoints.Length == 0) Debug.LogError("Waypoints eksik!");
        else MoveToWaypoint(currentWaypoint);
    }

    void Update()
    {
        // Merdiven (OffMeshLink) kontrolü
        if (agent.isOnOffMeshLink && !isWaiting)
        {
            StartCoroutine(HandleOffMeshLink());
            return; // Coroutine baþladýðýnda diðer waypoint kontrollerini beklet
        }

        // Waypoint kontrolü
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) &&
            !isWaiting)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    void MoveToWaypoint(int index)
    {
        if (waypoints.Length == 0) return;

        index = Mathf.Clamp(index, 0, waypoints.Length - 1);
        NavMeshHit hit;

        if (NavMesh.SamplePosition(waypoints[index].position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log("Hedef: " + index + " pozisyon: " + hit.position);
        }
        else
        {
            Debug.LogWarning("Waypoint " + index + " NavMesh üzerinde deðil!");
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        // Animasyon baþlat
        if (animator != null)
            animator.SetBool("isRagging", true);

        yield return new WaitForSeconds(waitTime);

        // Sonraki waypoint
        currentWaypoint++;
        if (currentWaypoint >= waypoints.Length)
            currentWaypoint = 0; // Döngüsel patrol

        agent.isStopped = false;
        MoveToWaypoint(currentWaypoint);

        // Animasyonu kapat
        if (animator != null)
            animator.SetBool("isRagging", false);

        isWaiting = false;
    }

    IEnumerator HandleOffMeshLink()
    {
        isWaiting = true;
        agent.isStopped = true;
        agent.updatePosition = false; // Lerp ile kontrol

        // Animasyonu aç
        if (animator != null)
            animator.SetBool("isRagging", true);

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = data.endPos;
        float duration = 1.3f; // Merdiven yüksekliðine göre ayarla
        float elapsed = 0f;

        while (elapsed < duration)
        {
            agent.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.CompleteOffMeshLink();
        agent.updatePosition = true;

        // Animasyonu kapat
        if (animator != null)
            animator.SetBool("isRagging", false);

        agent.isStopped = false;
        isWaiting = false;
    }
}
*/



/*

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Waypoints & Patrol")]
    public Transform[] waypoints;          // Normal patrol waypointleri
    private int currentWaypoint = 0;

    [Header("Stair Waypoints")]
    public Transform[] stairWaypoints1;    // Merdiven 1
    public Transform[] stairWaypoints2;    // Merdiven 2
    public Transform[] stairWaypoints3;    // Merdiven 3
    private bool isClimbingStairs = false;

    [Header("Components")]
    private NavMeshAgent agent;
    public Animator animator;
    public float waitTime = 2f;
    public float stairSpeed = 2f;          // Merdiven Lerp hýzý
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null) Debug.LogError("NavMeshAgent eksik!");
        if (animator == null) Debug.LogError("Animator eksik!");
        if (waypoints.Length == 0) Debug.LogError("Waypoints eksik!");
        else MoveToWaypoint(currentWaypoint);
    }

    void Update()
    {
        if (isClimbingStairs) return; // Merdiven sýrasýnda normal update durdur

        // Normal patrol waypoint kontrolü
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) &&
            !isWaiting)
        {
            StartCoroutine(WaitAtWaypoint());
        }

        // Merdiven baþýna yaklaþýnca doðru diziyi kullan
        CheckAndClimbStairs(stairWaypoints1);
        CheckAndClimbStairs(stairWaypoints2);
        CheckAndClimbStairs(stairWaypoints3);
    }

    void CheckAndClimbStairs(Transform[] stairWaypoints)
    {
        if (stairWaypoints.Length == 0 || isClimbingStairs) return;

        float distanceToStart = Vector3.Distance(transform.position, stairWaypoints[0].position);
        float distanceToEnd = Vector3.Distance(transform.position, stairWaypoints[stairWaypoints.Length - 1].position);

        if (distanceToStart < 1f) // çýkýþ
            StartCoroutine(ClimbStairs(stairWaypoints, true));
        else if (distanceToEnd < 1f) // iniþ
            StartCoroutine(ClimbStairs(stairWaypoints, false));
    }

    void MoveToWaypoint(int index)
    {
        if (waypoints.Length == 0) return;
        index = Mathf.Clamp(index, 0, waypoints.Length - 1);
        agent.SetDestination(waypoints[index].position);
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        if (animator != null)
            animator.SetBool("isRagging", true);

        yield return new WaitForSeconds(waitTime);

        currentWaypoint++;
        if (currentWaypoint >= waypoints.Length)
            currentWaypoint = 0;

        agent.isStopped = false;
        MoveToWaypoint(currentWaypoint);

        if (animator != null)
            animator.SetBool("isRagging", false);

        isWaiting = false;
    }

    IEnumerator ClimbStairs(Transform[] waypoints, bool goingUp)
    {
        isClimbingStairs = true;
        agent.isStopped = true;
        agent.updatePosition = false;

        if (animator != null)
            animator.SetBool("isRagging", true);

        int start = goingUp ? 0 : waypoints.Length - 1;
        int end = goingUp ? waypoints.Length : -1;
        int step = goingUp ? 1 : -1;

        for (int i = start; i != end; i += step)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = waypoints[i].position;
            float distance = Vector3.Distance(startPos, endPos);
            float elapsed = 0f;
            float duration = distance / stairSpeed;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos; // pozisyonu kesinleþtir
        }

        if (animator != null)
            animator.SetBool("isRagging", false);

        agent.isStopped = false;
        agent.updatePosition = true;
        isClimbingStairs = false;

        MoveToWaypoint(currentWaypoint);
    }

    // Scene görünürlüðü için Gizmos
    void OnDrawGizmos()
    {
        DrawStairGizmos(stairWaypoints1, Color.cyan);
        DrawStairGizmos(stairWaypoints2, Color.magenta);
        DrawStairGizmos(stairWaypoints3, Color.yellow);
    }

    void DrawStairGizmos(Transform[] stairWaypoints, Color color)
    {
        if (stairWaypoints == null || stairWaypoints.Length == 0) return;

        Gizmos.color = color;
        for (int i = 0; i < stairWaypoints.Length; i++)
        {
            if (stairWaypoints[i] != null)
                Gizmos.DrawSphere(stairWaypoints[i].position, 0.1f);

            if (i < stairWaypoints.Length - 1 && stairWaypoints[i + 1] != null)
                Gizmos.DrawLine(stairWaypoints[i].position, stairWaypoints[i + 1].position);
        }
    }
}
*/using System.Collections;
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
