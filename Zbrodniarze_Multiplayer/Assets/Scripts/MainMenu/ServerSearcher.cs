using System.Security.Cryptography;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;
using System.Runtime.CompilerServices;
using System.Drawing;

public class ServerSearcher : MonoBehaviour
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

    public GameObject serverButton;
    private TextMeshPro serverNameDisplay;

    private string serverName;
    private bool isPassworded;
    private bool isInProgress;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            TextMeshProUGUI serverNameDisplay = serverButton.GetComponentInChildren<TextMeshProUGUI>(true);

            Image passwordIcon = serverButton.transform.Find("Button/StatusIcon")?.GetComponent<Image>();
            if(isPassworded)
                passwordIcon.color = new Color32(255, 255, 255, 100);
            else
                passwordIcon.color = new Color32(255, 255, 255, 0);

            Image progressIcon = serverButton.transform.Find("Button/PasswordIcon")?.GetComponent<Image>();
            if(isInProgress)
                progressIcon.color = new Color32(255, 255, 255, 100);
            else
                progressIcon.color = new Color32(255, 255, 255, 0);
    
            serverName = "chuj";
            serverNameDisplay.text = serverName.ToString();
            Instantiate(serverButton, gameObject.transform);
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
    }
    void Awake()
    {
        ListLobbies();
    }
    void CreateServer()
    {
            TextMeshProUGUI serverNameDisplay = serverButton.GetComponentInChildren<TextMeshProUGUI>(true);

            Image passwordIcon = serverButton.transform.Find("Button/StatusIcon")?.GetComponent<Image>();
            if(isPassworded)
                passwordIcon.color = new Color32(255, 255, 255, 100);
            else
                passwordIcon.color = new Color32(255, 255, 255, 0);

            Image progressIcon = serverButton.transform.Find("Button/PasswordIcon")?.GetComponent<Image>();
            if(isInProgress)
                progressIcon.color = new Color32(255, 255, 255, 100);
            else
                progressIcon.color = new Color32(255, 255, 255, 0);
    
            serverName = "chuj";
            serverNameDisplay.text = serverName.ToString();
            Instantiate(serverButton, gameObject.transform);
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
                CreateServer();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Błąd przy liście lobby: " + e);
        }
    }
}
