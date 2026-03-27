using System;
using Fusion;
using UnityEngine;

public class GameStateNetwork : NetworkBehaviour
{
    public static GameStateNetwork Instance { get; private set; }

    public override void Spawned()
    {
        if(Instance != null && Instance != this)
        {
            // Despawn duplicate
            if (Runner != null)
                Runner.Despawn(Object);
            return;
        }
        Instance = this;
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartGame()
    {
        Debug.Log("Starting game");
        UIManager.Instance?.HideAllScreens();
    }
}