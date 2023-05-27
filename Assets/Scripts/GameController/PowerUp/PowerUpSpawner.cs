using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public float ammoSpawnTime = 20;
    public float powUpSpawnTime = 50;

    public string ammoPowerUp;
    public string[] powerUp;

    void Start()
    {
        // Starting IEnumerators
        StartCoroutine(AmmoLoop());
        StartCoroutine(PowerUpLoop());
    }

    IEnumerator AmmoLoop()
    {
        yield return new WaitForSeconds(ammoSpawnTime + Random.Range(-2.5f, 6f));

        // Calculate spawn pos and rot
        Vector3 spawnPos = Vector3.up * 10;
        spawnPos.x = Random.Range(-3.70f, 3.70f);
        spawnPos.z = Random.Range(-3.70f, 3.70f);

        Quaternion spawnRot = Quaternion.identity;
        spawnRot.x = Random.Range(0f, 360f);
        spawnRot.y = Random.Range(0f, 360f);
        spawnRot.z = Random.Range(0f, 360f);

        // Spawn if Marster Client
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.InstantiateRoomObject("PowerUp/" + ammoPowerUp, spawnPos, spawnRot);

        StartCoroutine(AmmoLoop());
    }

    IEnumerator PowerUpLoop()
    {
        yield return new WaitForSeconds(powUpSpawnTime + Random.Range(-10f, 11f));

        // Calculate spawn pos and rot
        Vector3 spawnPos = Vector3.up * 10;
        spawnPos.x = Random.Range(-3.70f, 3.70f);
        spawnPos.z = Random.Range(-3.70f, 3.70f);

        Quaternion spawnRot = Quaternion.identity;
        spawnRot.x = Random.Range(0f, 360f);
        spawnRot.y = Random.Range(0f, 360f);
        spawnRot.z = Random.Range(0f, 360f);

        // Select Random Power up
        string rndPowUp = powerUp[Random.Range(0, powerUp.Length)];

        // Spawn if Marster Client
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.InstantiateRoomObject("PowerUp/" + rndPowUp, spawnPos, spawnRot);

        StartCoroutine(PowerUpLoop());
    }
}
