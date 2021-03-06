﻿using System;
using UnityEngine;
using Mirror;

struct PlayerControllerInit : NetworkMessage
{
    public string alias;
}

public class NetworkManagerMultiplayer : NetworkManager
{
    [Header("Extended properties:")]
    [Scene] public string GameScene;
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerControllerInit>(OnPlayerInitRecv);
        ServerChangeScene(GameScene);
    }

    public GameObject playerControllerPrefab;
    public GameObject promptUI;
    public GameManager gameManager;

    public event Action OnClientDisconnectEventFired;
    public event Action OnClientStoppedEventFired;

    public override void OnStopClient()
    {
        base.OnStopClient();
        OnClientStoppedEventFired?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        OnClientStoppedEventFired?.Invoke();
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);

        GameObject uiCanvas = GameObject.FindGameObjectWithTag("GameCanvas");
        GameObject aliasPromptUI = Instantiate(promptUI, uiCanvas.transform);
        gameManager = FindObjectOfType<GameManager>();

        aliasPromptUI.GetComponent<AliasPrompt>().forwardTo = this;
        aliasPromptUI.GetComponent<AliasPrompt>().conn = conn;
    }

    public void SetAliasForPlayer(string alias, NetworkConnection conn)
    {
        PlayerControllerInit values = new PlayerControllerInit { alias = alias };
        conn.Send(values);
    }

    void OnPlayerInitRecv(NetworkConnection conn, PlayerControllerInit message)
    {
        GameObject newController = Instantiate(playerControllerPrefab);

        PlayerController player = newController.GetComponent<PlayerController>();
        player.alias = message.alias;

        NetworkServer.AddPlayerForConnection(conn, newController);
        gameManager.AdmitToGame(newController);
    }
}
