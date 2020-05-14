using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Manager : MonoBehaviour
{
    public string playerPrefab;
    public Transform[] spawnPoints;
    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        PhotonNetwork.Instantiate(playerPrefab, spawn.position, spawn.rotation);
    }
}
