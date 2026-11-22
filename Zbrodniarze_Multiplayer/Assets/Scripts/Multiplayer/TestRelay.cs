using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using System;

public class TestRelay : MonoBehaviour
{
    public static TestRelay Instance { get; private set; }

    public event Action<string> OnJoinCodeReceived;
    public event Action OnRelayConnected;
    public event Action OnAuthenticationComplete;

    private string currentJoinCode;
    private bool authenticationComplete = false;

    private async void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Oczekaj na autoryzację gracza przed kontynuowaniem
        await InitializeAndAuthenticate();
    }

    private async Task InitializeAndAuthenticate()
    {
        try
        {
            // Test czy Unity Services są już zainicjalizowane
            if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services initialized");
            }

            // Sprawdź, czy gracz jest już zalogowany, jeśli nie, zaloguj anonimowo
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log($"Zalogowano jako: {AuthenticationService.Instance.PlayerId}");
                };

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Player authenticated anonymously");
            }
            else
            {
                Debug.Log($"Player already signed in as: {AuthenticationService.Instance.PlayerId}");
            }

            authenticationComplete = true;
            OnAuthenticationComplete?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Błąd przy inicjalizacji: {e}");
        }
    }

    // Oczekaj na zakończenie autoryzacji gracza przed kontynuowaniem
    public async Task WaitForAuthentication()
    {
        while (!authenticationComplete)
        {
            await Task.Delay(100);
        }
    }

    // Stwórz alokację relaya jako host i uzyskaj kod dołączenia
    public async Task CreateRelayAsHost()
    {
        try
        {
            await WaitForAuthentication();

            // Stwórz alokację relaya dla 3 graczy (host + 2 klientów)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            // Prześlij kod dołączenia, który klienci mogą użyć, aby dołączyć do tej alokacji
            currentJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Kod dołączenia: {currentJoinCode}");

            // Invoke na zdarzenie, aby powiadomić innych o otrzymaniu kodu dołączenia
            OnJoinCodeReceived?.Invoke(currentJoinCode);

            // Configuruj dane serwera relaya w Unity Transport
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Zacznij jako host
            NetworkManager.Singleton.StartHost();

            Debug.Log("Host started successfully!");
            OnRelayConnected?.Invoke();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Błąd przy tworzeniu relaya: {e}");
        }
    }

    // Dołącz do istniejącej alokacji relaya jako klient przy użyciu kodu dołączenia
    public async Task JoinRelayAsClient(string joinCode)
    {
        try
        {
            await WaitForAuthentication();

            Debug.Log($"Dołączono do relaya z kodem: {joinCode}");
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Konfiguruj dane serwera relaya w Unity Transport
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Startuj jako klient
            NetworkManager.Singleton.StartClient();

            Debug.Log("Client started successfully!");
            OnRelayConnected?.Invoke();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Błąd przy dołączaniu do relaya: {e}");
        }
    }

    // Zwraca aktualny kod dołączenia, jeśli host jest aktywny
    public string GetCurrentJoinCode() => currentJoinCode;
}