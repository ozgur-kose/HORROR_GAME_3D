using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Player Prefab (NetworkObject)")]
    public NetworkObject playerPrefab;

    private NetworkRunner runner;
    private HashSet<PlayerRef> spawnedPlayers = new HashSet<PlayerRef>();
    private bool callbacksAdded = false;

    private void Awake()
    {
        runner = FindObjectOfType<NetworkRunner>();

        if (runner == null)
        {
            Debug.LogError("[Spawner] NetworkRunner sahnede bulunamadı!");
            return;
        }

        if (!callbacksAdded)
        {
            runner.AddCallbacks(this);
            callbacksAdded = true;
            Debug.Log("[Spawner] Callback eklendi.");
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer || SceneManager.GetActiveScene().name != "MapScene") return;

        Debug.Log("MapScene yüklendi, oyuncular spawn ediliyor...");

        foreach (var player in runner.ActivePlayers)
        {
            if (spawnedPlayers.Contains(player))
                continue;

            Vector3 spawnPos = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
            NetworkObject playerObj = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
            runner.SetPlayerObject(player, playerObj);

            spawnedPlayers.Add(player);
            Debug.Log($"✅ Player {player.PlayerId} MapScene'de spawn edildi ve kaydedildi.");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Oyuncu lobiye katıldı: {player.PlayerId}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} ayrıldı.");

        // 🔹 Sahnedeki player objesini bul
        var playerObj = runner.GetPlayerObject(player);

        if (playerObj != null)
        {
            // 🔹 Oyuncunun prefab’ını sil (Despawn)
            runner.Despawn(playerObj);
            Debug.Log($"❌ Player {player.PlayerId} despawn edildi.");
        }
        else
        {
            Debug.LogWarning($"⚠ Player {player.PlayerId} için aktif obje bulunamadı.");
        }

        // HashSet'ten temizle
        spawnedPlayers.Remove(player);
    }



    // Boş callback'ler
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
}