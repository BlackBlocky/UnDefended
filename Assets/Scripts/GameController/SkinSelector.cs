using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinSelector : MonoBehaviour
{
    public GameObject myPlayer;
    WeaponHandFollow weaponHand;

    public PhotonView photonView;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (Input.GetKey(KeyCode.Alpha1)) ChangeSkin(1);
            else if (Input.GetKey(KeyCode.Alpha2)) ChangeSkin(2);
            else if (Input.GetKey(KeyCode.Alpha3)) ChangeSkin(3);
        }       
    }

    public void ChangeSkin(int id)
    {
        // Update Vars
        weaponHand = myPlayer.transform.Find("Weapons").GetComponent<WeaponHandFollow>();

        // Make sure id isnt 0
        if(id == 0)
        {
            Debug.LogError("Skin Selected with id 0");
            return;
        }

        // Switch Skin
        //GameObject activeSkin;
        switch (id)
        {
            case 1: case 2: case 3:
                {
                    ChangeSkinIndex(id);
                    break;
                }

            default:
                Debug.LogError("Skin id " + id + " not found");
                return;
        }

    }
    
    void ChangeSkinIndex(int index)
    {
        // Deactive all Skins
        foreach ( GameObject item in myPlayer.GetComponent<PlayerSkinsData>().skin)
        {
            if(item != null) item.SetActive(false);
        }

        GameObject activeSkin;
        activeSkin = myPlayer.GetComponent<PlayerSkinsData>().skin[index];
        SkinData data = activeSkin.GetComponent<SkinData>();

        myPlayer.GetComponent<PlayerMovement>().anim = activeSkin.GetComponent<Animator>();
        myPlayer.GetComponent<PlayerShoot>().ammoTxt = data.ammoTxt;

        myPlayer.GetComponent<PlayerAxeUse>().anim = activeSkin.GetComponent<Animator>();

        weaponHand.targetHand = activeSkin.GetComponent<SkinData>().myHand;

        activeSkin.SetActive(true);

        photonView.RPC("ChangeSkinRPC", RpcTarget.OthersBuffered, myPlayer.GetComponent<PhotonView>().ViewID, index);
    }

    [PunRPC]
    void ChangeSkinRPC(int viewID, int index)
    {
        GameObject targetPlayer = PhotonView.Find(viewID).gameObject;
        WeaponHandFollow targetWeaponHand = targetPlayer.transform.Find("Weapons").GetComponent<WeaponHandFollow>();

        foreach (GameObject item in targetPlayer.GetComponent<PlayerSkinsData>().skin)
        {
            if (item != null) item.SetActive(false);
        }

        GameObject activeSkin = targetPlayer.GetComponent<PlayerSkinsData>().skin[index];

        targetPlayer.GetComponent<PlayerShoot>().ammoTxt = activeSkin.GetComponent<SkinData>().ammoTxt;
        targetPlayer.GetComponent<PlayerAxeUse>().anim = activeSkin.GetComponent<Animator>();

        targetWeaponHand.targetHand = activeSkin.GetComponent<SkinData>().myHand;

        activeSkin.SetActive(true);

    }
}
