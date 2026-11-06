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

    private Lobby hostLobby;
    private float heartbeatTimer;
    private string playerName;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Zalogowano jako: {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Gracz" + Random.Range(1, 1000);
        Debug.Log("Nazwa gracza: " + playerName);
    }
    //PLACEHOLDER FUNKCJE DO TESTOWANIA LOBBY WYJĄĆ W PRZYSZŁOŚCI \/
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateLobby();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ListLobbies();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            JoinLobbyByCode("test");
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuickJoinLobby();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            LeaveLobby();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintPlayers(hostLobby);
        }
        //PLACEHOLDER FUNKCJE DO TESTOWANIA LOBBY WYJĄĆ W PRZYSZŁOŚCI /\

        HandleLobbyHeartbeat();
    }

    private async Task HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer += Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
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
                     {"NumberOfRounds", new DataObject(DataObject.VisibilityOptions.Public, "5", DataObject.IndexOptions.S1) }
                 }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers, createLobbyOptions);

             hostLobby = lobby;

            Debug.Log("Stworzyłem lobby: " + lobby.Name);
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
            Debug.LogError("Błąd przy tworzeniu lobby: " + e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);

            Debug.Log("Dołączyłem do lobby: " + lobbyCode);
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
            Lobby joinedLobby =  await Lobbies.Instance.QuickJoinLobbyAsync();

            Debug.Log("Dołączyłem do lobby pierwszego lobby ");

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
            if (hostLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(hostLobby.Id, AuthenticationService.Instance.PlayerId);
                Debug.Log("Opuszczono lobby: " + hostLobby.Name);
                hostLobby = null;
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
}
