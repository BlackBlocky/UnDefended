using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseScript : MonoBehaviour
{
    public BaseManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Check if Zombie
            GameObject zombie = other.gameObject;
            if (zombie.tag == "Zombie")
            {
                // Destroy Zombie
                ZombieManager.Instance.DestroyZombie(zombie.GetComponent<ZombieHealth>().id);

                // Add Damage
                manager.AddDamage(10);
            }
        }
    }
}
