using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using Mirror;

public class LobbyNetworkManager : NetworkManager
{
    [Header("Room")]
    [Scene] [SerializeField] string menuScene;
    [SerializeField] NetworkRoomPlayerLobby roomPlayerPrefab;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    public override void OnStartServer()//Server loads objects
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("Prefabs").ToList();
    }
    public override void OnStartClient()//client loads objects
    {
        GameObject[] _spawnPrefabs = Resources.LoadAll<GameObject>("Prefabs");

        foreach (var _prefab in _spawnPrefabs)
        {
            NetworkClient.RegisterPrefab(_prefab);
        }
    }


    public override void OnClientConnect()
    {
        base.OnClientConnect();

        OnClientConnected?.Invoke();
    }
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if(numPlayers > maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if(SceneManager.GetActiveScene().name != menuScene)
        {
            conn.Disconnect();
        }

        
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if(SceneManager.GetActiveScene().name == menuScene)
        {
            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);

            
        }
    }
}
