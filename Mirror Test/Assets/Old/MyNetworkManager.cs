using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    public static MyNetworkManager current;

    public override void Awake()
    {
        base.Awake();

        current = this;
    }

    public void EnterScene(int sceneIndex,NetworkConnection conn)
    {
        SceneManager.LoadScene(sceneIndex);

        

        NetworkServer.AddPlayerForConnection(conn, playerPrefab);
    }
}
