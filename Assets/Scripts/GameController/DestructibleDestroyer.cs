using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleDestroyer : MonoBehaviour
{
    public static DestructibleDestroyer Instance;

    public PhotonView photonView;


    private void Awake()
    {
        Instance = this;
    }

    public void DestroyDestructible(int id)
    {
        photonView.RPC("DestroyDestructibleRPC", RpcTarget.All, id);
    }

    [PunRPC]
    void DestroyDestructibleRPC(int id)
    {
        // Get Bullet with id
        try
        {
            GameObject[] bullets = GameObject.FindGameObjectsWithTag("Destructible");
            foreach (GameObject item in bullets)
            {
                if (item.GetComponent<DestructibleScript>().Id == id)
                {
                    Destroy(item);
                    return;
                }
            }
        }
        catch { }
    }
}
