using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class PlayerShoot : MonoBehaviour
{
    public float shootCooldown = 0.6f;
    public float bulletLiveTime = 2f;
    public float ammo = 60;

    public bool canShoot = true;

    public Transform bSpawnPos;
    public TextMeshPro ammoTxt;
    public SoundPlayer soundPlayer;
    public PhotonView photonView;

    float activeCooldown = 0f;
    BuildingController buildingController;

    private void Awake()
    {
        // Check if Player is mine
        if (!GetComponent<PhotonView>().IsMine)
        {
            canShoot = false;
        }

        // Setting up vars
        buildingController = GameObject.FindGameObjectWithTag("GameController").GetComponent<BuildingController>();
    }

    void Update()
    {
        if (canShoot && !buildingController.isBuilding && !buildingController.hasBuild) ShootUpdate();
    }

    bool shooted = false;
    private void ShootUpdate()
    {
        if (activeCooldown == 0 && ammo > 0)
        {
            // Check for Input
            if (Input.GetButtonDown("Fire1") || Input.GetAxisRaw("Fire1") == 1 && !shooted)
            {
                // Spawn bullet
                photonView.RPC("ShootBulletRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.UserId,
                    bSpawnPos.position.x, bSpawnPos.position.y, bSpawnPos.position.z,
                    transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w,
                    Random.Range(-2147483648, 2147483647));

                // Change vars
                ammo--;
                RefreshAmmoTxt();

                // Effects
                CinemachineShake.Instance.ShakeCamera(3, 0.1f);
                soundPlayer.PlaySingle();
                activeCooldown = shootCooldown;

                shooted = true;
            }
            // Reset shooted (for controller)
            else if (Input.GetAxisRaw("Fire1") == 0 && shooted)
            {
                shooted = false;
            }
        }
        else activeCooldown = Mathf.Clamp(activeCooldown - Time.deltaTime, 0, shootCooldown);
    }

    [PunRPC]
    void ShootBulletRPC(string id, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, int bulletId)
    {
        Quaternion spawnRot = new Quaternion(rotX, rotY, rotZ, rotW);
        GameObject s = Instantiate(Resources.Load<GameObject>("Bullet"), new Vector3(posX, posY, posZ), spawnRot);
        s.GetComponent<BulletScript>().damage = 34f;
        s.GetComponent<BulletScript>().ownerId = id;
        s.GetComponent<BulletScript>().id = bulletId;

        Destroy(s, bulletLiveTime);
    }

    int lastFullAmmo;
    public void RefreshAmmoTxt()
    {
        // Check if Refresh is Needed
        if(lastFullAmmo != (int)ammo || ammo <= 0)
        {
            // Make sure that ammo is in the allowed area
            ammo = Mathf.Clamp(ammo, 0, Mathf.Infinity);

            lastFullAmmo = (int)ammo;

            // Refresh
            ammoTxt.text = ammo.ToString("00");
            photonView.RPC("RefreshAmmoTxtRPC", RpcTarget.Others, ammo);
        }        
    }

    [PunRPC]
    void RefreshAmmoTxtRPC(float ammo)
    {
        ammoTxt.text = ammo.ToString("00");
    }
}
