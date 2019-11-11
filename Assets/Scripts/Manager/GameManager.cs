﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    private float matchTime = 20f * 60;
    private float timer;
    [SerializeField]private Text timeText;
    private float matchTimerStart = 5f;
    private bool hasGameStarted = false;

    public static GameManager Instance { get; private set; }
    void Awake() {

        if (!Instance) {
            Instance = this;
        }

        if (PhotonNetwork.IsMasterClient) {
            timer = matchTime;
            timeText.text = timer.ToString();
            photonView.RPC("Send_Timer", RpcTarget.AllViaServer, timer);
        }
        else if (!PhotonNetwork.IsConnected) {
            timer = matchTime;
            timeText.text = timer.ToString();
        }
    }

    private void Start() {
        if (PhotonNetwork.IsMasterClient) {
            StartCoroutine(Start_Match_Timer(2f));
        }
    }

    IEnumerator Start_Match_Timer(float timer) {
        Debug.Log("Timer has started " + timer);
        photonView.RPC("Spawn_Player_Self", RpcTarget.AllViaServer);
        yield return new WaitForSeconds(timer);
        photonView.RPC("Match_Has_Started", RpcTarget.AllViaServer);
    }

    private void Update() {
        if(hasGameStarted)
            Match_Countdown_Timer();
    }

    // Update is called once per frame
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player other) {

        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("Send_Timer", RpcTarget.Others, timer);
    }

    [PunRPC]
    void Send_Timer(float timeIn) {
        timer = timeIn;
    }

    void Match_Countdown_Timer() {
        if (timer > 0.0f) {
            timer -= Time.deltaTime;
            int seconds = (int)(timer) % 60;
            int minutes = (int)(timer / 60) % 60;
            string timerText = string.Format("{0:00}:{1:00}", minutes, seconds);
            timeText.text = timerText;
        }
        else {
            Debug.Log("GAME OVER");
        }
    }

    [PunRPC]
    void Spawn_Player_Self() {

        foreach (Photon.Realtime.Player punPlayer in PhotonNetwork.PlayerList) {

            if(punPlayer != PhotonNetwork.LocalPlayer) {
                continue;
            }
            GameObject playerObject = punPlayer.TagObject as GameObject;
            if (playerObject != null) {
                Player player = playerObject.GetComponent<Player>();
                if (!player) {
                    Debug.LogError("This object doesn't have player component");
                }
                SpawnManager.Instance.Spawn_Player(player.playerTeamNum, player);
            }
            else {
                Debug.LogError("Object is null for some reason");
                Debug.LogError("Object is equal to " + punPlayer.TagObject);
            }
        }
    }

    [PunRPC]
    void Match_Has_Started() {
        hasGameStarted = true;
    }

    public float Get_Timer() {
        return timer;
    }

    public float Get_Match_Time() {
        return matchTime;
    }
}
