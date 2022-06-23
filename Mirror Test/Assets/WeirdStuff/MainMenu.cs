using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] LobbyNetworkManager manager;

    [SerializeField] GameObject landingPagePanel;

    public void HostLobby()
    {
        manager.StartHost();

        landingPagePanel.SetActive(false);
    }
}
