using UnityEngine;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the connection between Lobby and Relay systems.
/// Coordinates the flow: Lobby -> Relay -> Game Scene
/// </summary>
public class LobbyRelayManager : MonoBehaviour
{
    public static LobbyRelayManager Instance { get; private set; }

    // SCENE NAMES - Update these if your scene names change in the future
    private const string MAIN_MENU_SCENE = "MainMenu";  // TODO: Change if scene name changes
    private const string GAMEPLAY_SCENE = "MultiTest";   // TODO: Change if scene name changes

    private bool isTransitioningToGame = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Listen for relay connection events
        if (TestRelay.Instance != null)
        {
            TestRelay.Instance.OnRelayConnected += OnRelayConnected;
        }
    }

    /// <summary>
    /// Called by host when pressing 'G' to start the game
    /// Initiates relay creation and prepares game start
    /// </summary>
    public async void HostStartGame(Lobby lobby)
    {
        if (isTransitioningToGame)
        {
            Debug.LogWarning("Gra już się przechodzi!");
            return;
        }

        if (lobby == null)
        {
            Debug.LogError("Lobby jest null!");
            return;
        }

        isTransitioningToGame = true;

        Debug.Log("Host rozpoczyna grę! Tworzę relay allocation...");

        // Create relay allocation as host
        await TestRelay.Instance.CreateRelayAsHost();

        // From here, OnRelayConnected event will handle scene loading
    }

    /// <summary>
    /// Called when relay connection is established (for both host and clients)
    /// Loads the gameplay scene
    /// </summary>
    private void OnRelayConnected()
    {
        Debug.Log("Relay connection established! Loading gameplay scene...");

        // Small delay to ensure network setup is complete
        Invoke(nameof(LoadGameplayScene), 0.5f);
    }

    /// <summary>
    /// Loads the gameplay scene where actual game happens
    /// </summary>
    private void LoadGameplayScene()
    {
        Debug.Log($"Loading scene: {GAMEPLAY_SCENE}");
        SceneManager.LoadScene(GAMEPLAY_SCENE);
    }

    /// <summary>
    /// Called by clients when they receive relay join code and need to connect
    /// In future, this can be triggered by UI or automatic detection
    /// </summary>
    public async void ClientJoinGame(string relayJoinCode)
    {
        if (isTransitioningToGame)
        {
            Debug.LogWarning("Gra już się przechodzi!");
            return;
        }

        if (string.IsNullOrEmpty(relayJoinCode))
        {
            Debug.LogError("Relay join code jest pusty!");
            return;
        }

        isTransitioningToGame = true;

        Debug.Log($"Client dołącza do gry z kodem: {relayJoinCode}");

        // Join relay as client
        await TestRelay.Instance.JoinRelayAsClient(relayJoinCode);

        // From here, OnRelayConnected event will handle scene loading
    }

    /// <summary>
    /// Resets the manager when returning to main menu
    /// Call this when leaving the game
    /// </summary>
    public void ResetGameTransition()
    {
        isTransitioningToGame = false;
    }

    private void OnDestroy()
    {
        if (TestRelay.Instance != null)
        {
            TestRelay.Instance.OnRelayConnected -= OnRelayConnected;
        }
    }
}