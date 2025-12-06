using UnityEngine;
using UnityEngine.AI;

public class EnemyDebug : MonoBehaviour
{
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("❌ NavMeshAgent component bulunamadı!");
    }

    void Update()
    {
        if (agent != null)
        {
            Debug.Log($"➡ Hedefe kalan mesafe: {agent.remainingDistance}  |  " +
                      $"⛔ Yol var mı: {!agent.pathPending}  |  " +
                      $"✅ Geçerli yol: {agent.hasPath}  |  " +
                      $"⚠ Engellendi mi: {agent.isStopped}");
        }
    }
}
