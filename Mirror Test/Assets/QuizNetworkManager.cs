using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Linq;
using Mirror;
public class QuizNetworkManager : NetworkManager
{
    public static PlayerQuizData myPlayer;

    public static QuizNetworkManager current;

    [SerializeField] GameObject playerDataPrefab;

    [HideInInspector] public bool allowJoining = true;

    [HideInInspector] public List<PlayerQuizData> playerList = new List<PlayerQuizData>();

    

    public override void Awake()
    {
        current = this;

        base.Awake();
    }
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
    }


    public override void OnServerConnect(NetworkConnection conn)
    {
        if (!allowJoining)
        {
            conn.Disconnect();
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject player = Instantiate(playerDataPrefab);
        
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);

        if (playerList.Count == 0) player.GetComponent<PlayerQuizData>().isLeader = true;

        playerList.Add(player.GetComponent<PlayerQuizData>());
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        PlayerQuizData _player = conn.identity.GetComponent<PlayerQuizData>();
        if(_player != null && playerList.Contains(_player))
        {
            playerList.Remove(_player);
        }

        
        base.OnServerDisconnect(conn);
    }


    public static int SortPlayersByScore(PlayerQuizData a, PlayerQuizData b)
    {
        if (a.score > b.score)
        {
            return -1;
        }
        else if (a.score < b.score)
        {
            return 1;
        }

        return 0;
    }
}
