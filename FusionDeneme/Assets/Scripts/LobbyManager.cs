using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if TMP_PRESENT
using TMPro;
#endif

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Fusion")]
    public NetworkRunner runner;

    [Header("UI Elements")]
    public InputField joinCodeInput;
    public Button hostButton;
    public Button joinButton;
    public Button startGameButton;
    public Text statusText;

#if TMP_PRESENT
    public TMP_Text playersListTMP;
#endif

    public Text playersListText;

    [Header("Scenes")]
    public string mapSceneName = "MapScene";

    private string sessionName;
    private bool isHost = false;
    private bool callbacksAdded = false;
    private readonly HashSet<PlayerRef> connectedPlayers = new HashSet<PlayerRef>();

    private void Awake()
    {
        if (runner == null)
            runner = FindObjectOfType<NetworkRunner>();

        hostButton.onClick.AddListener(HostGame);
        joinButton.onClick.AddListener(JoinGame);
        startGameButton.onClick.AddListener(StartGameSession);
        startGameButton.gameObject.SetActive(false);

        SafeSetPlayersText("Lobi Oyuncuları:\n(Henüz oyuncu yok)");
    }

    private void EnsureCallbacks()
    {
        if (runner == null)
            runner = FindObjectOfType<NetworkRunner>();

        if (runner != null && !callbacksAdded)
        {
            runner.AddCallbacks(this);
            callbacksAdded = true;
            Debug.Log("[Lobby] Callbacks eklendi.");
        }
    }

    private async void HostGame()
    {
        isHost = true;
        sessionName = Random.Range(1000, 9999).ToString();
        statusText.text = $"Hosting... Join Code: {sessionName}";
        joinCodeInput.text = sessionName;

        EnsureCallbacks();

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
        });

        if (!result.Ok)
        {
            statusText.text = "Host başlatılamadı: " + result.ShutdownReason;
            return;
        }

        startGameButton.gameObject.SetActive(true);
        RebuildListFromRunner();
        UpdatePlayerListUI();
    }

    private async void JoinGame()
    {
        if (string.IsNullOrEmpty(joinCodeInput.text))
            return;

        sessionName = joinCodeInput.text;
        statusText.text = "Joining " + sessionName + "...";

        EnsureCallbacks();

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
        });

        if (!result.Ok)
        {
            statusText.text = "Join başarısız: " + result.ShutdownReason;
            return;
        }

        RebuildListFromRunner();
        UpdatePlayerListUI();
    }

    public async void StartGameSession()
    {
        if (!isHost || runner == null) return;

        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        joinCodeInput.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);

        Debug.Log("Host sahne geçişini başlattı...");
        await runner.LoadScene(mapSceneName);
    }

    // ================== Fusion Callbacks ==================
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!connectedPlayers.Contains(player))
            connectedPlayers.Add(player);

        UpdatePlayerListUI();
        Debug.Log($"[Lobby] Oyuncu katıldı: Player {player.PlayerId}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        connectedPlayers.Remove(player);
        UpdatePlayerListUI();
        Debug.Log($"[Lobby] Oyuncu ayrıldı: Player {player.PlayerId}");
    }

    // ================== Boş Callback'ler ==================
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
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    // ================== UI Helpers ==================
    private void RebuildListFromRunner()
    {
        connectedPlayers.Clear();

        if (runner != null)
        {
            foreach (var p in runner.ActivePlayers)
                connectedPlayers.Add(p);
        }

        UpdatePlayerListUI();
    }

    public void UpdatePlayerListUI()
    {
        if (playersListText == null || runner == null) return;

        playersListText.text = "Lobi Oyuncuları:\n";
        foreach (var player in runner.ActivePlayers)
        {
            playersListText.text += $"- Player {player.PlayerId}\n";
        }
    }

    private void SafeSetPlayersText(string text)
    {
        if (playersListText != null) playersListText.text = text;
#if TMP_PRESENT
        if (playersListTMP != null) playersListTMP.text = text;
#endif
    }
}
