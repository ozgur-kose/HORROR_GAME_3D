using Fusion;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    [Networked] public string PlayerName { get; set; }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetName(string newName)
    {
        PlayerName = newName;
        Debug.Log($"Oyuncu adı güncellendi: {newName}");

        // 🔹 RPC çağrıldığında UI'yı hemen yenile
        var manager = FindObjectOfType<LobbyManager>();
        if (manager != null)
            manager.UpdatePlayerListUI();
    }
}
