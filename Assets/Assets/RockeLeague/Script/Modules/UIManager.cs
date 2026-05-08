using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using UnityEngine.SceneManagement;

/// <summary>
/// Flow: Main Menu → Host/Join → Room Setup → Waiting Room → Gameplay → Result Screen → Main Menu
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ── Screens ───────────────────────────────────────────────────────────────
    [Header("Screens")]
    [SerializeField] private GameObject screenMain;
    [SerializeField] private GameObject screenLobby;
    [SerializeField] private GameObject screenRoom;
    [SerializeField] private GameObject screenWaiting;
    [SerializeField] private GameObject screenResult;

    // ── Main ──────────────────────────────────────────────────────────────────
    [Header("Main Menu")]
    [SerializeField] private Button btnPlay;

    // ── Lobby ─────────────────────────────────────────────────────────────────
    [Header("Lobby")]
    [SerializeField] private Button btnHost;
    [SerializeField] private Button btnJoin;

    // ── Room Setup ────────────────────────────────────────────────────────────
    [Header("Room Setup")]
    [SerializeField] private TMP_InputField inputRoomName;
    [SerializeField] private Toggle         toggle1v1;
    [SerializeField] private Toggle         toggle2v2;
    [SerializeField] private Button         btnConfirm;     // "Create" or "Join"
    [SerializeField] private TextMeshProUGUI txtConfirmLabel;
    [SerializeField] private TextMeshProUGUI txtStatus;

    // ── Waiting Room ──────────────────────────────────────────────────────────
    [Header("Waiting Room")]
    [SerializeField] private TextMeshProUGUI txtRoomName;
    [SerializeField] private TextMeshProUGUI txtPlayerCount;
    [SerializeField] private Button          btnStart;      // host-only, always visible
    [SerializeField] private Button          btnLeave;

    // ── Result Screen ─────────────────────────────────────────────────────────
    [Header("Result Screen")]
    [SerializeField] private TextMeshProUGUI txtResultWinner;
    [SerializeField] private TextMeshProUGUI txtResultScore;
    [SerializeField] private Button          btnLeaveToMenu;

    // ── Gameplay Scene ────────────────────────────────────────────────────────
    [Header("Gameplay")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private int gameplaySceneIndex = 1;

    // ── State ─────────────────────────────────────────────────────────────────
    private bool   _isHosting;
    private int    _playerCount = 0;
    private int    _required    = 2;

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _playerCount = 0;
        
        // Main Menu
        if (btnPlay != null)
            btnPlay.onClick.AddListener(OnPlay);
        
        // Lobby
        if (btnHost != null)
            btnHost.onClick.AddListener(OnHost);
        if (btnJoin != null)
            btnJoin.onClick.AddListener(OnJoin);
        
        // Room
        if (btnConfirm != null)
            btnConfirm.onClick.AddListener(OnConfirm);
        
        // Waiting Room
        if (btnStart != null)
            btnStart.onClick.AddListener(OnStart);
        if (btnLeave != null)
            btnLeave.onClick.AddListener(OnLeave);
        
        // Result Screen
        if (btnLeaveToMenu != null)
            btnLeaveToMenu.onClick.AddListener(OnLeaveToMenu);

        // Subscribe to network events only if NetworkManager exists
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnSessionStarted    += HandleSessionStarted;
            NetworkManager.Instance.OnConnectionFailed  += HandleConnectionFailed;
            NetworkManager.Instance.OnPlayerJoined_Event += HandlePlayerJoined;
            NetworkManager.Instance.OnPlayerLeft_Event   += HandlePlayerLeft;
        }
        
    }

    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnSessionStarted     -= HandleSessionStarted;
            NetworkManager.Instance.OnConnectionFailed   -= HandleConnectionFailed;
            NetworkManager.Instance.OnPlayerJoined_Event -= HandlePlayerJoined;
            NetworkManager.Instance.OnPlayerLeft_Event   -= HandlePlayerLeft;
        }
    }

    // ── Button Handlers ───────────────────────────────────────────────────────

    private void OnPlay() => Show(screenLobby);

    private void OnHost()
    {
        _isHosting = true;
        if (txtConfirmLabel != null)
            txtConfirmLabel.text = "Create Room";
        if (txtStatus != null)
            txtStatus.text = "";
        Show(screenRoom);
    }

    private void OnJoin()
    {
        _isHosting = false;
        if (txtConfirmLabel != null)
            txtConfirmLabel.text = "Join Room";
        if (txtStatus != null)
            txtStatus.text = "";
        Show(screenRoom);
    }

    private async void OnConfirm()
    {
        if (inputRoomName == null) return;

        string room = inputRoomName.text.Trim();
        if (string.IsNullOrEmpty(room))
        {
            if (txtStatus != null)
                txtStatus.text = "Enter a room name.";
            return;
        }

        // Read match mode and set in GameSessionManager
        _required = (toggle2v2 != null && toggle2v2.isOn) ? 4 : 2;
        MatchMode mode = (toggle2v2 != null && toggle2v2.isOn) ? MatchMode.TwoVsTwo : MatchMode.OneVsOne;
        
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.SetMatchMode(mode);
        }

        if (btnConfirm != null)
            btnConfirm.interactable = false;
        
        if (txtStatus != null)
            txtStatus.text = _isHosting ? "Creating..." : "Joining...";

        // Ensure NetworkManager exists
        if (NetworkManager.Instance == null)
        {
            var go = new GameObject("NetworkManager");
            go.AddComponent<NetworkManager>();
            
            // Re-subscribe to events
            NetworkManager.Instance.OnSessionStarted    += HandleSessionStarted;
            NetworkManager.Instance.OnConnectionFailed  += HandleConnectionFailed;
            NetworkManager.Instance.OnPlayerJoined_Event += HandlePlayerJoined;
            NetworkManager.Instance.OnPlayerLeft_Event   += HandlePlayerLeft;
        }

        if (_isHosting)
            await NetworkManager.Instance.StartHost(room, GameMode.Host);
        else
            await NetworkManager.Instance.StartClient(room);
    }

    private void OnStart()
    {
        if (GameStateNetwork.Instance != null)
        {
            Debug.Log("start");
            NetworkManager.Instance?.LockRoom();
            GameStateNetwork.Instance.RPC_StartGame();
        }
    }
    private void OnLeave()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.Disconnect();
        }
        _playerCount = 0;
        Show(screenMain);
    }

    private void OnLeaveToMenu()
    { if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.Disconnect();
            Debug.Log("[UIManager] Disconnected from room.");
        }
 
        // 2. Clear match session data (scores, timer, car map, etc.)
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.ClearSession();
            Debug.Log("[UIManager] Session cleared.");
        }
 
        // 3. Reset local UI state so the lobby is fresh next time
        _playerCount = 0;
        _isHosting   = false;
 
        if (btnConfirm != null)  btnConfirm.interactable = true;
        if (txtStatus != null)   txtStatus.text = "";
        if (inputRoomName != null) inputRoomName.text = "";
 
        // 4. Back to Main Menu — same scene, just swap screens
        Show(screenMain);
        Debug.Log("[UIManager] Returned to Main Menu.");
        
    }

    // ── Network Callbacks ─────────────────────────────────────────────────────

    private void HandleSessionStarted()
    {
        if (btnConfirm != null)
            btnConfirm.interactable = true;

        if (txtRoomName != null && inputRoomName != null)
            txtRoomName.text = $"Room: {inputRoomName.text.Trim()}";

        // Only host sees Start button
        if (btnStart != null)
            btnStart.gameObject.SetActive(_isHosting);

        RefreshPlayerCount();
        Show(screenWaiting);
    }

    private void HandleConnectionFailed(string reason)
    {
        if (btnConfirm != null)
            btnConfirm.interactable = true;
        if (txtStatus != null)
            txtStatus.text = $"Failed: {reason}";
    }

    private void HandlePlayerJoined(PlayerRef player)
    {
        _playerCount++;
        RefreshPlayerCount();

        // Auto-start when full (only host triggers scene load)
        // if (_playerCount >= _required)
        //     StartCoroutine(LoadGameplay());
    }

    private void HandlePlayerLeft(PlayerRef player)
    {
        _playerCount = Mathf.Max(1, _playerCount - 1);
        RefreshPlayerCount();
    }

    // ── Public Methods ────────────────────────────────────────────────────────

    public void ShowResultScreen(int redScore, int blueScore, string winner)
    {
        if (txtResultWinner != null)
        {
            if (winner == "Draw")
            {
                txtResultWinner.text = "<color=yellow>DRAW!</color>";
            }
            else
            {
                txtResultWinner.text = $"<color=yellow>{winner.ToUpper()} WINS!</color>";
            }
        }

        if (txtResultScore != null)
        {
            txtResultScore.text = $"Final Score\n<color=red>RED {redScore}</color> - <color=blue>BLUE {blueScore}</color>";
        }

        Show(screenResult);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void RefreshPlayerCount()
    {
        if (txtPlayerCount != null)
            txtPlayerCount.text = $"{_playerCount}/{_required} players";
    }
    private void Show(GameObject target)
    {
        if (screenMain != null)
            screenMain.SetActive(target == screenMain);
        if (screenLobby != null)
            screenLobby.SetActive(target == screenLobby);
        if (screenRoom != null)
            screenRoom.SetActive(target == screenRoom);
        if (screenWaiting != null)
            screenWaiting.SetActive(target == screenWaiting);
        if (screenResult != null)
            screenResult.SetActive(target == screenResult);
    }

    public  void HideAllScreens()
    {
        if (screenMain != null)
            screenMain.SetActive(false);
        if (screenLobby != null)
            screenLobby.SetActive(false);
        if (screenRoom != null)
            screenRoom.SetActive(false);
        if (screenWaiting != null)
            screenWaiting.SetActive(false);
        if (screenResult != null)
            screenResult.SetActive(false);
    }
}
