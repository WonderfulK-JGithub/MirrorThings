using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] LobbyNetworkManager manager;

    [SerializeField] GameObject landingPagePanel;
    [SerializeField] TMP_InputField ipAddressInputField;
    [SerializeField] Button joinButton;

    private void OnEnable()
    {
        LobbyNetworkManager.OnClientConnected += HandleClientConnected;
        LobbyNetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        LobbyNetworkManager.OnClientConnected -= HandleClientConnected;
        LobbyNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        string ipAddress = ipAddressInputField.text;

        manager.networkAddress = ipAddress;
        manager.StartClient();

        joinButton.interactable = false;
    }

    void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }
}
