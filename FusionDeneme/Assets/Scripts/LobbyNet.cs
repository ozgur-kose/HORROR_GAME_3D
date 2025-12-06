/*
 using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class LobbyNet : NetworkBehaviour
{
    public static Dictionary<int, string> NameTable = new Dictionary<int, string>();

    // 🔹 Herkes kendi adını host'a gönderir, host herkese yayar
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SubmitName(string name, RpcInfo info = default)
    {
        var sender = info.Source;

        if (sender == default)
        {
            Debug.LogWarning("⚠️ info.Source null geldi, Runner.LocalPlayer kullanılacak.");
            sender = Runner.LocalPlayer;
        }

        Debug.Log($"📨 RPC_SubmitName geldi: {sender.PlayerId} → {name}");

        // 🔹 StateAuthority (host) çalışıyor mu kontrol et
        if (!Object.HasStateAuthority)
        {
            Debug.Log("🚫 StateAuthority bizde değil, isim yayını yapılmayacak (yalnızca host yapar).");
            return;
        }

        // 🔹 Host tabloyu günceller
        NameTable[sender.PlayerId] = name;

        // 🔹 Herkese yayın yap
        RPC_BroadcastName(sender, name);
    }

    // 🔹 Herkese yay (hem host hem client alır)
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_BroadcastName(PlayerRef player, string name, RpcInfo info = default)
    {
        Debug.Log($"📢 İsim yayını: ({player.PlayerId}) → {name}");
        NameTable[player.PlayerId] = name;

        var lobby = FindObjectOfType<LobbyManager>();
        if (lobby != null)
        {
            lobby.SetPlayerDisplayName(player, name);
            lobby.UpdatePlayerListUI(); // UI'yi güncel tut
        }
    }
}
*/