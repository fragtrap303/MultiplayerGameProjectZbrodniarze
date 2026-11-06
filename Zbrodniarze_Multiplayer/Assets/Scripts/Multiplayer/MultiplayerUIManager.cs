using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class MultiplayerUIManager : MonoBehaviour
{
    [SerializeField] private Button ServerButton;
    [SerializeField] private Button HostButton;
    [SerializeField] private Button ClientButton;

    [SerializeField] private GameObject joinCodeInputPanel;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private Button joinConfirmButton;
    [SerializeField] private Button joinCancelButton;

    [SerializeField] private TextMeshProUGUI joinCodeDisplayText;

    private void Awake()
    {
        ServerButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        HostButton.onClick.AddListener(() =>
        {
            HostRelayGame();
        });

        ClientButton.onClick.AddListener(() =>
        {
            ShowJoinCodeInput();
        });

        // Setup join code panel buttons
        if (joinConfirmButton != null)
        {
            joinConfirmButton.onClick.AddListener(JoinRelayGame);
        }

        if (joinCancelButton != null)
        {
            joinCancelButton.onClick.AddListener(HideJoinCodeInput);
        }

        // Hide input panel initially
        if (joinCodeInputPanel != null)
        {
            joinCodeInputPanel.SetActive(false);
        }

        // Clear display text initially
        if (joinCodeDisplayText != null)
        {
            joinCodeDisplayText.text = "No join code available";
            Debug.Log("joinCodeDisplayText is assigned");
        }
        else
        {
            Debug.LogError("joinCodeDisplayText is NOT assigned in inspector!");
        }
    }

    private void Start()
    {
        // Subscribe to join code events in Start to ensure TestRelay is initialized
        if (TestRelay.Instance != null)
        {
            TestRelay.Instance.OnJoinCodeReceived += DisplayJoinCode;
            Debug.Log("Successfully subscribed to OnJoinCodeReceived event");
        }
        else
        {
            Debug.LogWarning("TestRelay.Instance is null in Start!");
        }
    }

    private void OnDestroy()
    {
        if (TestRelay.Instance != null)
        {
            TestRelay.Instance.OnJoinCodeReceived -= DisplayJoinCode;
        }
    }

    private async void HostRelayGame()
    {
        if (TestRelay.Instance != null)
        {
            await TestRelay.Instance.CreateRelayAsHost();
        }
        else
        {
            Debug.LogError("TestRelay instance not found!");
        }
    }

    private void DisplayJoinCode(string joinCode)
    {
        Debug.Log($"DisplayJoinCode called with: {joinCode}");

        if (joinCodeDisplayText != null)
        {
            joinCodeDisplayText.text = $"Join Code: {joinCode}";
            Debug.Log($"Updated UI text to: Join Code: {joinCode}");
        }
        else
        {
            Debug.LogError("joinCodeDisplayText is null in DisplayJoinCode!");
        }
    }

    private void ShowJoinCodeInput()
    {
        if (joinCodeInputPanel != null)
        {
            joinCodeInputPanel.SetActive(true);
            if (joinCodeInputField != null)
            {
                joinCodeInputField.text = "";
                joinCodeInputField.ActivateInputField();
            }
        }
    }

    private void HideJoinCodeInput()
    {
        if (joinCodeInputPanel != null)
        {
            joinCodeInputPanel.SetActive(false);
        }
    }

    private async void JoinRelayGame()
    {
        if (joinCodeInputField == null || string.IsNullOrEmpty(joinCodeInputField.text))
        {
            Debug.LogWarning("Join code is empty!");
            return;
        }

        string joinCode = joinCodeInputField.text;

        if (TestRelay.Instance != null)
        {
            await TestRelay.Instance.JoinRelayAsClient(joinCode);
            HideJoinCodeInput();
        }
        else
        {
            Debug.LogError("TestRelay instance not found!");
        }
    }
}