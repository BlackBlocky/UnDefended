using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletDestroyer : MonoBehaviour
{
    public static BulletDestroyer Instance;

    public PhotonView photonView;


    private void Awake()
    {
        Instance = this;
    }

    public void DestroyBullet(int id)
    {
        photonView.RPC("DestroyBulletRPC", RpcTarget.All, id);
    }

    [PunRPC]
    void DestroyBulletRPC(int id)
    {
        // Get Bullet with id
        try
        {
            GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
            foreach (GameObject item in bullets)
            {
                if (item.GetComponent<BulletScript>().id == id)
                {
                    Destroy(item);
                    return;
                }
            }
        }
        catch { }
    }
}
