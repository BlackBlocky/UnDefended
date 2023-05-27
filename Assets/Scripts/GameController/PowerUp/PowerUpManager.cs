using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PowerUpManager : MonoBehaviour
{
    public GameObject myPlayer;
    public int[] itemList;
    public Mesh[] itemMeshes;

    public int activePowUp = 0;   

    public MeshFilter meshFilter;
    public PhotonView photonView;

    public bool isFreeze;


    [Header("Sounds")]
    public SoundPlayer switchPwUpSound;
    public SoundPlayer ammoGotSound;
    public SoundPlayer powUpCollSound;
    public SoundPlayer freezeSound;

    [Header("Pars")]
    public ParticleSystem freezePar;

    private void Awake()
    {
        // Setting up Itemlist
        itemList[0] = 0;
        itemList[1] = 0;
        itemList[2] = 0;
    }

    private void Start()
    {
        meshFilter = myPlayer.transform.Find("PowerUpBox").GetComponent<MeshFilter>();
    }

    private void Update()
    {
        // Keyboard
        if (!GetComponent<BuildingController>().isBuilding && !GetComponent<BuildingController>().hasBuild)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchItem(0, true);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchItem(1, true);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchItem(2, true);

            // Universal
            else if (Input.GetButtonDown("Fire2")) StartPowerUp();

            // Controller
            else if (Input.GetButtonDown("PwSwitchLeft"))
            {
                int newActive = activePowUp;

                if (newActive <= 0) newActive = 2;
                else newActive--;

                SwitchItem(newActive, true);
            }
            else if (Input.GetButtonDown("PwSwitchRight"))
            {
                int newActive = activePowUp;

                if (newActive >= 2) newActive = 0;
                else newActive++;

                SwitchItem(newActive, true);
            }
        }        
    }

    public void RunPowerUp(int id)
    {
        // Search Power Up
        switch (id)
        {
            case 1:
                {
                    AmmoPowUp();
                    break;
                }
            case 2 or 3 or 4 or 5:
                {
                    AddItem(id);
                    break;
                }
            default:
                {
                    Debug.LogError($"Power Up {id} not found");
                    break;
                }
        }
    }
    
    public void StartPowerUp()
    {
        switch (itemList[activePowUp])
        {
            case 0: case 1:
                {
                    return;
                }
            case 2: // Health PowerUp
                {
                    HealthPowerUp();
                    itemList[activePowUp] = 0;
                    SwitchItem(activePowUp, false);
                    break;
                }
            case 3: // Freeze PowerUp
                {
                    photonView.RPC("FreezePowerUpRPC", RpcTarget.All);
                    itemList[activePowUp] = 0;
                    SwitchItem(activePowUp, false);
                    break;
                }
            case 4: // Bomb PowerUp
                {
                    photonView.RPC("BombPowerUpRPC", RpcTarget.MasterClient);
                    itemList[activePowUp] = 0;
                    SwitchItem(activePowUp, false);
                    break;
                }
            case 5: // Flamethrower PowerUp
                {
                    FlamethrowerPowerUp();
                    itemList[activePowUp] = 0;
                    SwitchItem(activePowUp, false);
                    break;
                }
            default:
                {
                    Debug.LogError($"Cant find Power Up {itemList[activePowUp]} in Starting Switch");
                    break;
                }
        }
    }

    public void AddItem(int id)
    {
        // Effects
        powUpCollSound.PlaySingle();

        // Try add Item
        if (itemList[0] == 0)
        {
            itemList[0] = id;
            SwitchItem(0, false);
            return;
        }
        else if (itemList[1] == 0)
        {
            itemList[1] = id;
            SwitchItem(1, false);
            return;
        }
        else if (itemList[2] == 0)
        {
            itemList[2] = id;
            SwitchItem(2, false);
            return;
        }
        
        // Move items and remove last and set first to new
        itemList[2] = itemList[1];
        itemList[1] = itemList[0];
        itemList[0] = id;

        SwitchItem(0, false);
    }

    public void SwitchItem(int index, bool playSound)
    {
        activePowUp = index;

        // Change Mesh
        meshFilter.mesh = itemMeshes[itemList[activePowUp]];

        // Effects
        if (playSound) switchPwUpSound.PlaySingle();
    }

    public void AmmoPowUp()
    {
        // Add Ammo
        myPlayer.GetComponent<PlayerShoot>().ammo += 50;
        myPlayer.GetComponent<PlayerShoot>().RefreshAmmoTxt();
        
        // Effects
        ammoGotSound.PlaySingle();
    }

    public void HealthPowerUp()
    {
        // Add Base Hp
        GetComponent<BaseManager>().AddHealth(30f);
    }

    [PunRPC]
    void FreezePowerUpRPC()
    {
        StartCoroutine(FreezePowerUp());
    }

    public IEnumerator FreezePowerUp()
    {
        isFreeze = true;
        freezeSound.PlaySingle();
        freezePar.Play();

        yield return new WaitForSeconds(4f);

        isFreeze = false;
    }

    [PunRPC]
    public void BombPowerUpRPC()
    {
        PhotonNetwork.InstantiateRoomObject("ActivePwUp/ActiveBombPwUp", Vector3.up * 1.5f, Quaternion.identity);
    }

    public void FlamethrowerPowerUp()
    {
        myPlayer.GetComponent<PlayerFlameThrower>().enabled = true;
        myPlayer.GetComponent<PlayerFlameThrower>().StartFlamethrower();
    }
}
