using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using Photon.Realtime;
using TMPro;

public class QuickLogin : MonoBehaviourPunCallbacks
{
    public GameObject MYPLAYER; 

    void Start()
    {
        print("App Version: " + Application.version);

        // Connect to Server
        Application.runInBackground = true;
        print("Connecting to Marster...");
        GameObject.Find("WaveText").GetComponent<TextMeshProUGUI>().text = "Connecting...";

        PhotonNetwork.ConnectUsingSettings();       
    }

    public override void OnConnectedToMaster()
    {
        print("Server Version: " + PhotonNetwork.AppVersion);

        print("Joining Lobby...");
        GameObject.Find("WaveText").GetComponent<TextMeshProUGUI>().text = "Connecting to Lobby...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        print("Joining/Create Room");
     
        // Set Nickname
        PhotonNetwork.NickName = "Player " + Random.Range(1, 1000);
        print("Nickname = " + PhotonNetwork.NickName);

        GameObject.Find("WaveText").GetComponent<TextMeshProUGUI>().text = "Joining Room...";

        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        print("Joined Room");

        GameObject s = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        // Setting up vars in GameController with Player
        GetComponent<SkinSelector>().myPlayer = s;
        GetComponent<PowerUpManager>().myPlayer = s;
        GetComponent<BuildingController>().myPlayer = s;

        MYPLAYER = s;

        // Start Compments
        GetComponent<PowerUpSpawner>().enabled = true;
        GetComponent<PowerUpManager>().enabled = true;
        GetComponent<WaveSystem>().enabled = true;
    }
}
