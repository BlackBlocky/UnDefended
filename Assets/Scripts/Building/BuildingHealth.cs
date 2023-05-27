using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BuildingHealth : MonoBehaviour
{
    public float health = 100;

    [SerializeField] private SoundPlayer damageSound;
    
    private void OnCollisionEnter(Collision collision)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if (collision.gameObject.tag == "Zombie")
            {
                GetComponent<PhotonView>().RPC("AddDamageToBuilding", RpcTarget.All, 20f, collision.gameObject.GetComponent<ZombieHealth>().id);
            }
        }       
    }

    [PunRPC]
    void AddDamageToBuilding(float buildingDamage, int zombieID)
    {
        // Destroy Zombie
        ZombieManager.Instance.DestroyZombie(zombieID);

        // Remove Health
        health -= buildingDamage;
        if(health <= 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }

        DamageEffects();
    }

    void DamageEffects()
    {
        // Effects
        CinemachineShake.Instance.ShakeCamera(10, 0.2f);
        damageSound.PlaySingle();
    }
}
