using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpScript : MonoBehaviour
{
    public int id = 0;
    /*
     * 0 = nothing
     * 1 = ammo
     * 2 = heal
     * 3 = freeze
     * 4 = bomb
     * 5 = flamethrower
     */

    public PhotonView photonView;
    PowerUpManager manager;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<PowerUpManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if Player
        GameObject coll = collision.gameObject;
        if(coll.tag == "Player")
        {
            // Check if it is my Player
            if (coll.GetComponent<PhotonView>().IsMine)
            {
                manager.RunPowerUp(id);
                photonView.RPC("DestroyPowerUp", RpcTarget.MasterClient);
            }
        }
    }

    [PunRPC]
    void DestroyPowerUp()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
