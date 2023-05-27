using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandFollow : MonoBehaviour
{
    public Transform targetHand;
    public GameObject[] weapons;

    public int activePrimary = 1;

    void Update()
    {
        if (targetHand != null)
        {
            transform.SetPositionAndRotation(targetHand.position, targetHand.rotation);
        }
    }

    public void SwitchWeapon(int id)
    {
        /*
         * 0 = Hand
         * 1 = Pistol
         * 2 = Axe
         * 3 = Flamethrower
         */

        foreach (GameObject item in weapons)
        {
            item.SetActive(false);
        }
        weapons[id].SetActive(true);
    }
}
