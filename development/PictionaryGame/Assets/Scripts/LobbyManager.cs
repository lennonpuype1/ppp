﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Login UI")]
    public InputField playerNameInputField;
    public GameObject ui_login;

    [Header("Lobby UI")]
    public GameObject ui_lobby;
    public GameObject gameButtonList;
    public GameObject gameButton;

    [Header("Connection status UI")]
    public GameObject ui_connectionStatus;
    public Text connectionStatusText;
    public bool showConnectionStatus = false;

    [Header("Popup UI")]
    public GameObject ui_Popup;
    public Text popupReason;

    [Header("Onboarding UI")]
    public GameObject ui_Onboarding;
    public GameObject onboarding1;

    //[Header("Other")]
    //private int hasPlayed = PlayerPrefs.GetInt("HasPlayed");

    #region UNITY Methods
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            //Activating only lobby ui
            ui_lobby.SetActive(true);
            ui_connectionStatus.SetActive(false);

            ui_login.SetActive(false);
        }
        else
        {
            //Activation only login ui since we did not connect to photon yet
            ui_lobby.SetActive(false);
            ui_connectionStatus.SetActive(false);

            ui_login.SetActive(true);
            

            int hasPlayed = PlayerPrefs.GetInt("FirstTimePlaying");

            if (hasPlayed == 0)
            {
                //First time
                ui_login.SetActive(false);

                ui_Onboarding.SetActive(true);
                onboarding1.SetActive(true);
            }
            else
            {
                Debug.Log("Dendezen");
               
                ui_login.SetActive(true);
                
                //Not First Time

                ui_Onboarding.SetActive(false);
            }
        }


        
    }

    

    // Update is called once per frame
    void Update()
    {
        if (showConnectionStatus)
        {
            connectionStatusText.text = "" + PhotonNetwork.NetworkClientState;
        }

        var disconnected = Convert.ToBoolean(PhotonNetwork.LocalPlayer.CustomProperties["Disconnected"]);

        if (disconnected)
        {
            ui_Popup.SetActive(true);
            
            popupReason.text = "You got disconnected :(";
        }
        else
        {
            ui_Popup.SetActive(false);
            popupReason.text = "";
        }
    }

    public void closeDisconnectionMessage()
    {
        ExitGames.Client.Photon.Hashtable isDisconnected = new ExitGames.Client.Photon.Hashtable { { MultiPlayerGame.DISCONNECTED, "false" } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(isDisconnected);
    }

    #endregion

    #region UI Callback Methods
    public void OnEnterGameButtonClicked()
    {
        string playerName = playerNameInputField.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            ui_lobby.SetActive(false);
            ui_login.SetActive(false);

            showConnectionStatus = true;
            ui_connectionStatus.SetActive(true);

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;

                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.Log("Playername is invalid or empty!");
        }
    }

    public void onAddGameClicked()
    {
        createAndJoinRoom();
    }

    public void createAndJoinRoom()
    {
        string randomRoomName = "Game" + UnityEngine.Random.Range(0, 1000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add("GameStarted", false);
        roomOptions.CustomRoomProperties.Add("CustomRoomName", randomRoomName);

        PhotonNetwork.CreateRoom(randomRoomName, roomOptions);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        createAndJoinRoom();
    }

    public override void OnJoinedRoom()
    {
        SceneLoader.Instance.LoadScene("GameScene");

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            Debug.Log("Joined to " + PhotonNetwork.CurrentRoom.Name + ". Waiting for more players");
            
        }
        else
        {
            Debug.Log("Joined to " + PhotonNetwork.CurrentRoom.Name + "You can play since there are " + PhotonNetwork.CurrentRoom.PlayerCount + " players");
            
            //StartCoroutine(DeactivateAfterSeconds(informPanelGameObject, 2.0f));

        }

        Debug.Log(" joined to " + PhotonNetwork.CurrentRoom.Name);
    }

    #endregion

    #region PHOTON Callback Methods
    public override void OnConnected()
    {
        Debug.Log("We connected to internet");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ExitGames.Client.Photon.Hashtable isDisconnected = new ExitGames.Client.Photon.Hashtable { { MultiPlayerGame.DISCONNECTED, "true" } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(isDisconnected);
        SceneLoader.Instance.LoadScene("LobbyScene");
    }

    public override void OnConnectedToMaster()
    {
        ui_connectionStatus.SetActive(false);
        ui_login.SetActive(false);

        ui_lobby.SetActive(true);

        PhotonNetwork.JoinLobby();
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to photon network");
    }

    //public override void OnDisconnected(DisconnectCause cause)
    //{
    //    SceneLoader.Instance.LoadScene("LobbyScene");
    //    ui_Popup.SetActive(true);
    //    popupReason.text = Convert.ToString(cause);
    //}

    #endregion

    IEnumerator DeactivateAfterSeconds(GameObject gameObject, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }
}
