using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public PhotonView photonView;

    public float spawnSpeed = 2f;
    public string[] zombieNames;

    public bool canSpawn = true;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        spawnSpeed = Mathf.Clamp(spawnSpeed -= 0.007f * Time.deltaTime, 0.25f, 2);
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(spawnSpeed);
        if (canSpawn && PhotonNetwork.IsMasterClient)
        {
            // Find Spawns
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("ZombieSpawn");

            // Spawn Zombie
            var spawnPos = spawns[Random.Range(0, spawns.Length)].transform.position;
            ZombieManager.Instance.SpawnZombie(zombieNames[Random.Range(0, zombieNames.Length)],
                spawnPos.x, spawnPos.y, spawnPos.z, Random.Range(-2147483648, 2147483647));
        }
        
        StartCoroutine(SpawnLoop());
    }
}
