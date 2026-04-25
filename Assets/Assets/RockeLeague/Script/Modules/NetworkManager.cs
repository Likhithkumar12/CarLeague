using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private NetworkPrefabRef[] carPrefabs; // Index matches character selection
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private NetworkPrefabRef gameStateNetworkPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints1v1Team1;
    [SerializeField] private Transform[] spawnPoints1v1Team2;
    [SerializeField] private Transform[] spawnPoints2v2Team1;
    [SerializeField] private Transform[] spawnPoints2v2Team2;

    public NetworkRunner Runner { get; private set; }

    private Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new();
    private NetworkObject spawnedGameState;
    private int playerCount = 0;

    // Events
    public event Action OnSessionStarted;
    public event Action<string> OnConnectionFailed;
    public event Action<PlayerRef> OnPlayerJoined_Event;
    public event Action<PlayerRef> OnPlayerLeft_Event;
    private CarInput _carInput;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _carInput = new CarInput();
        _carInput.Enable();
    }

    public async Task StartHost(string roomName, GameMode gameMode)
    {
        // Clean up any existing runner first
        if (Runner != null)
        {
            await Runner.Shutdown();
            Destroy(Runner);
            Runner = null;
        }

        Runner = gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;
        Runner.AddCallbacks(this); // explicitly register callbacks

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        var startArgs = new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
        };

        var result = await Runner.StartGame(startArgs);

        if (result.Ok)
        {
            OnSessionStarted?.Invoke();
        }
        else
        {
            OnConnectionFailed?.Invoke(result.ShutdownReason.ToString());
        }
    }

    public async Task StartClient(string roomName)
    {
        if (Runner != null)
        {
            await Runner.Shutdown();
            Destroy(Runner);
            Runner = null;
        }

        Runner = gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;
        Runner.AddCallbacks(this);

        var startArgs = new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = roomName,
        };

        var result = await Runner.StartGame(startArgs);

        if (result.Ok)
        {
            OnSessionStarted?.Invoke();
        }
        else
        {
            OnConnectionFailed?.Invoke(result.ShutdownReason.ToString());
        }
    }

    public void Disconnect()
    {
        if (Runner != null)
        {
            Runner.Shutdown();
        }
    }

    // ─── INetworkRunnerCallbacks ───────────────────────────────────────────────

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        playerCount++;
        OnPlayerJoined_Event?.Invoke(player);

        if (runner.IsServer)
        {
            if (spawnedGameState == null && gameStateNetworkPrefab != null)
            {
                spawnedGameState = runner.Spawn(gameStateNetworkPrefab, Vector3.zero, Quaternion.identity);
            }
            SpawnPlayer(runner, player);
        }

        Debug.Log($"[NetworkManager] Player joined: {player} | Total: {playerCount}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        playerCount--;
        OnPlayerLeft_Event?.Invoke(player);

        if (spawnedPlayers.TryGetValue(player, out NetworkObject netObj))
        {
            runner.Despawn(netObj);
            spawnedPlayers.Remove(player);
        }

        Debug.Log($"[NetworkManager] Player left: {player} | Total: {playerCount}");
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        // Determine match mode from GameSessionManager
        MatchMode mode = GameSessionManager.Instance != null
            ? GameSessionManager.Instance.CurrentMatchMode
            : MatchMode.OneVsOne;

        // Assign team based on join order
        int playerIndex = spawnedPlayers.Count; // 0-based
        int team = playerIndex % 2; // 0 = Team1, 1 = Team2

        Transform spawnPoint = GetSpawnPoint(mode, team, playerIndex);

        // FIXED: Assign car based on join order (player 0 gets car 0, player 1 gets car 1, etc.)
        // This cycles through available cars if there are more players than car prefabs
        int carIndex = playerIndex % carPrefabs.Length;
    
        // Register the car index for this player
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.RegisterPlayerCar(player, carIndex);
        }

        carIndex = Mathf.Clamp(carIndex, 0, carPrefabs.Length - 1);

        // Validate spawn point
        if (spawnPoint == null)
        {
            Debug.LogError($"[NetworkManager] Spawn point is NULL for player {player}! Using fallback position.");
            var temp = new GameObject("TempSpawnPoint").transform;
            temp.position = new Vector3(playerIndex * 5f, 2f, 0); // Spread out players
            spawnPoint = temp;
        }
        Debug.Log("Spawn point: " + spawnPoint);

        NetworkObject networkPlayerObject = Runner.Spawn(
            carPrefabs[carIndex],
            spawnPoint.position,
            spawnPoint.rotation,
            player
        );

        spawnedPlayers[player] = networkPlayerObject;

       
        if (networkPlayerObject.TryGetComponent<CarTeamAssignment>(out var teamComp))
        {
            teamComp.SetTeam(team);

         
            if (player == runner.LocalPlayer)
            {
                GameSessionManager.Instance?.SetLocalPlayerTeam(team);
            }
        }


        Debug.Log($"[NetworkManager] Spawned player {player} | PlayerIndex: {playerIndex} | Car: {carIndex} | Team: {team} | Pos: {spawnPoint.position}");
    }


    private Transform GetSpawnPoint(MatchMode mode, int team, int playerIndex)
    {
        Transform[] points = null;
    
        if (mode == MatchMode.TwoVsTwo)
        {
            points = team == 0 ? spawnPoints2v2Team1 : spawnPoints2v2Team2;
        }
        else
        {
            points = team == 0 ? spawnPoints1v1Team1 : spawnPoints1v1Team2;
        }

        // Validate points array
        if (points == null || points.Length == 0)
        {
            Debug.LogError($"[NetworkManager] Spawn points array is NULL or EMPTY! Mode: {mode}, Team: {team}");
            return null;
        }

        // FIXED: Calculate slot based on team position (how many players on this team)
        // For example: player 0 (team 0) -> slot 0, player 1 (team 1) -> slot 0, player 2 (team 0) -> slot 1
        int teamPlayerIndex = playerIndex / 2; // 0,1 -> 0; 2,3 -> 1; 4,5 -> 2
        int slot = teamPlayerIndex % points.Length;
    
        // Additional validation
        if (points[slot] == null)
        {
            Debug.LogError($"[NetworkManager] Spawn point at index {slot} is NULL!");
            return null;
        }

        Debug.Log($"[NetworkManager] GetSpawnPoint: mode={mode}, team={team}, playerIndex={playerIndex}, teamPlayerIndex={teamPlayerIndex}, slot={slot}, pos={points[slot].position}");
        return points[slot];
    }


    // ─── Unused but required callbacks ────────────────────────────────────────

    // In NetworkManager.cs — replace the empty OnInput
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new CarInputData
        {
            Move  = _carInput.car.Move.ReadValue<Vector2>(),
            Roll  = _carInput.car.Roll.ReadValue<float>(),
            Jump  = _carInput.car.Jump.IsPressed(),
            Boost = _carInput.car.Boost.IsPressed(),
            Drift = _carInput.car.Drift.IsPressed(),
        };
        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[NetworkManager] Runner shutdown: {shutdownReason}");
        playerCount = 0;
        spawnedPlayers.Clear();

        if (Runner != null)
        {
            Destroy(Runner);
            Runner = null;
        }
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        OnConnectionFailed?.Invoke(reason.ToString());
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
    {
        throw new NotImplementedException();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}

public enum MatchMode { OneVsOne, TwoVsTwo }