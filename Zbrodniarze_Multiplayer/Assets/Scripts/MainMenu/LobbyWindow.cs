using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;

public class LobbyWindow : MonoBehaviour
{
    public GameObject lobbyWindow;

    public void CloseWindow()
    {
        lobbyWindow.SetActive(false);
    }

    void Start()
    {
        UnityServices.InitializeAsync();
    }
}
