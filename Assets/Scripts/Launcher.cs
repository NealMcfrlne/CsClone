using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Launcher : MonoBehaviourPunCallbacks
{
    //called on startup
    public void Awake()
    {
        //Syncs client scene to host scene
        PhotonNetwork.AutomaticallySyncScene = true;
        Connect();
    }

    //called as soon as you connect to master
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected!");
        Join();
        base.OnConnectedToMaster();
    }

    public override void OnJoinedRoom()
    {
        StartGame();
        base.OnJoinedRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Create();
        base.OnJoinRandomFailed(returnCode, message);
    }

    public void Connect()
    {
        //Only connect to servers with this version
        Debug.Log("Trying to connect....");
        PhotonNetwork.GameVersion = "0.0.0";
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Join()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void Create()
    {
        PhotonNetwork.CreateRoom("");
    }

    public void StartGame()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount==1)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }
}
