using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    public static TestLobby Instance { get; private set; }

    private Lobby hostLobby;
    private Lobby currentLobby;
    private float heartbeatTimer;
    private string playerName;
    private bool isHost = false;
    private float lobbyRefreshTimer = 0f;
    private const float LOBBY_REFRESH_INTERVAL = 1.5f; // Refresh lobby every 1.5 seconds
    private string lastRelayCode = ""; // Track if relay code changed

    private async void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Poczekaj na autoryzację gracza przed kontynuowaniem
        if (TestRelay.Instance != null)
        {
            await TestRelay.Instance.WaitForAuthentication();
        }

        playerName = "Gracz" + Random.Range(1, 1000);
        Debug.Log("Nazwa gracza: " + playerName);
    }

    private void Update()
    {
        // Auto-refresh lobby every few seconds to see updated player list
        if (currentLobby != null)
        {
            lobbyRefreshTimer += Time.deltaTime;
            if (lobbyRefreshTimer >= LOBBY_REFRESH_INTERVAL)
            {
                lobbyRefreshTimer = 0f;
                RefreshLobby();
            }
        }

        // Placeholder funkcje do testowania - zmień w przyszłości na UI
        // C - Create Lobby (Host)
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateLobby();
        }
        // L - List available lobbies
        if (Input.GetKeyDown(KeyCode.L))
        {
            ListLobbies();
        }
        // J - Join lobby by code (for testing)
        if (Input.GetKeyDown(KeyCode.J))
        {
            JoinLobbyByCode("test");
        }
        // Q - Quick join any available lobby
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuickJoinLobby();
        }
        // E - Leave current lobby
        if (Input.GetKeyDown(KeyCode.E))
        {
            LeaveLobby();
        }
        // P - Print current players in lobby
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintPlayers(currentLobby);
        }
        // G - Start game (Host only) - Initiates relay and loads gameplay scene
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (isHost && currentLobby != null)
            {
                LobbyRelayManager.Instance.HostStartGame(currentLobby);
            }
            else if (!isHost && currentLobby != null)
            {
                Debug.LogWarning("Czekaj aż host zaplanuje grę!");
            }
            else
            {
                Debug.LogWarning("Musisz być w lobby!");
            }
        }

        HandleLobbyHeartbeat();
    }

    private async Task RefreshLobby()
    {
        if (currentLobby == null) return;

        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

            // Sprawdź czy kod relay się pojawił w danych lobby (client detection)
            if (!isHost && currentLobby.Data.ContainsKey("RelayJoinCode"))
            {
                string relayCode = currentLobby.Data["RelayJoinCode"].Value;

                // Test czy kod relay się zmienił i nie jest pusty
                if (!string.IsNullOrEmpty(relayCode) && relayCode != lastRelayCode)
                {
                    lastRelayCode = relayCode;
                    Debug.Log($"Client detected relay code: {relayCode}. Joining game...");

                    // Klient dołącza do gry z kodem relay
                    LobbyRelayManager.Instance.ClientJoinGame(relayCode);
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Błąd przy odświeżaniu lobby: " + e);
        }
    }

    private async Task HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer += Time.deltaTime;
            if (heartbeatTimer >= 15f)
            {
                heartbeatTimer = 0f;
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError("Błąd przy heartbeat: " + e);
                }
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            string LobbyName = "TestLobby";
            int MaxPlayers = 2;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "NumberOfRounds", new DataObject(DataObject.VisibilityOptions.Public, "5", DataObject.IndexOptions.S1) },
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, "", DataObject.IndexOptions.S1) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers, createLobbyOptions);

            hostLobby = lobby;
            currentLobby = lobby;
            isHost = true;

            Debug.Log("Stworzyłem lobby: " + lobby.Name);
            PrintPlayers(currentLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Błąd przy tworzeniu lobby: " + e);
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Lobby: " + lobby.Name + " (Liczba rund: " + lobby.Data["NumberOfRounds"].Value + ")");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Błąd przy liście lobby: " + e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            Lobby joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
            currentLobby = joinedLobby;
            isHost = false;

            Debug.Log("Dołączyłem do lobby: " + lobbyCode);
            PrintPlayers(currentLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Błąd przy dołączaniu do lobby: " + e);
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };
            Lobby joinedLobby = await Lobbies.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            currentLobby = joinedLobby;
            isHost = false;

            Debug.Log("Dołączyłem do lobby " + joinedLobby.Name);

            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Błąd przy szybkim dołączaniu do lobby: " + e);
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            if (currentLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
                Debug.Log("Opuszczono lobby: " + currentLobby.Name);
                currentLobby = null;
                hostLobby = null;
                isHost = false;
            }
            else
            {
                Debug.LogWarning("Nie jesteś w żadnym lobby.");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Błąd przy opuszczaniu lobby: " + e);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    private void PrintPlayers(Lobby lobby)
    {
        if (lobby == null) return;

        Debug.Log("Gracze w lobby: " + lobby.Name + " (ID: " + lobby.Id + ")" + " (Liczba rund: " + lobby.Data["NumberOfRounds"].Value + ")");
        foreach (Player player in lobby.Players)
        {
            Debug.Log("Gracz: " + player.Data["PlayerName"].Value + " (ID: " + player.Id + ")");
        }
    }

    // Wezwnae przez hosta po utworzeniu relay, aby zaktualizować lobby z kodem dołączenia
    public async Task UpdateLobbyWithRelayCode(string relayCode)
    {
        if (!isHost || currentLobby == null)
        {
            Debug.LogError("Tylko host może aktualizować lobby!");
            return;
        }

        try
        {
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode, DataObject.IndexOptions.S1) }
                }
            };

            currentLobby = await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateOptions);
            Debug.Log($"Lobby updated with relay code: {relayCode}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Błąd przy aktualizacji lobby: " + e);
        }
    }

    // Public accessors for LobbyRelayManager
    public Lobby GetCurrentLobby() => currentLobby;
    public bool IsHost() => isHost;
    public string GetPlayerName() => playerName;
}
