using UnityEngine;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the connection between Lobby and Relay systems.
/// Coordinates the flow: Lobby -> Load Gameplay Scene -> Relay -> Game
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
    /// Loads gameplay scene first, then initiates relay
    /// </summary>
    public void HostStartGame(Lobby lobby)
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

        Debug.Log("Host rozpoczyna grę! Ładuję scenę gry...");

        // Load gameplay scene first, THEN create relay after scene is loaded
        StartCoroutine(LoadSceneThenCreateRelay());
    }

    /// <summary>
    /// Coroutine that loads the scene and waits for it to load before creating relay
    /// </summary>
    private System.Collections.IEnumerator LoadSceneThenCreateRelay()
    {
        // Load the gameplay scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GAMEPLAY_SCENE);

        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("Gameplay scene loaded! Creating relay allocation...");

        // Small delay to ensure scene is fully initialized
        yield return new WaitForSeconds(0.5f);

        // Now create relay (NetworkManager should exist by now)
        CreateRelayAsHost();
    }

    /// <summary>
    /// Creates relay allocation after scene is loaded
    /// </summary>
    private async void CreateRelayAsHost()
    {
        await TestRelay.Instance.CreateRelayAsHost();

        // After relay is created, get the join code and share it via lobby
        string joinCode = TestRelay.Instance.GetCurrentJoinCode();
        if (!string.IsNullOrEmpty(joinCode))
        {
            await TestLobby.Instance.UpdateLobbyWithRelayCode(joinCode);
            Debug.Log("Relay join code shared with clients!");
        }
    }

    /// <summary>
    /// Called when relay connection is established
    /// </summary>
    private void OnRelayConnected()
    {
        Debug.Log("Relay connection established!");
        // Scene is already loaded, so we're good to go
    }

    /// <summary>
    /// Called by clients when they receive relay join code and need to connect
    /// Loads scene first, then joins relay
    /// </summary>
    public void ClientJoinGame(string relayJoinCode)
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

        // Load gameplay scene first, THEN join relay
        StartCoroutine(LoadSceneThenJoinRelay(relayJoinCode));
    }

    /// <summary>
    /// Coroutine that loads the scene and waits for it to load before joining relay
    /// </summary>
    private System.Collections.IEnumerator LoadSceneThenJoinRelay(string joinCode)
    {
        // Load the gameplay scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(GAMEPLAY_SCENE);

        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("Scena gry wczytana! Dołacz do relaya...");

        // Small delay to ensure scene is fully initialized
        yield return new WaitForSeconds(0.5f);

        // Now join relay (NetworkManager should exist by now)
        JoinRelayAsClient(joinCode);
    }

    /// <summary>
    /// Joins relay as client after scene is loaded
    /// </summary>
    private async void JoinRelayAsClient(string joinCode)
    {
        await TestRelay.Instance.JoinRelayAsClient(joinCode);
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