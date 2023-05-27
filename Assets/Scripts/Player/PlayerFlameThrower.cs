using Photon.Pun;
using UnityEngine;

public class PlayerFlameThrower : MonoBehaviour
{  
    public bool canShoot = false;

    [Header("Linked Compoments")]
    [SerializeField] PlayerShoot playerShoot;
    [SerializeField] private PhotonView photonView;

    [Header("Effects")]
    [SerializeField] private SoundPlayer shootingSound;  
    [SerializeField] private ParticleSystem shootPar;

    [Header("WeaponsHand")]
    [SerializeField] private WeaponHandFollow weaponHand;

    BuildingController buildingController; // For Checking if player is building

    private float maxFuel = 8f;
    private float fuel = 0f;


    private void Awake()
    {
        // Setting up vars
        buildingController = GameObject.FindGameObjectWithTag("GameController").GetComponent<BuildingController>();
    }

    private void OnEnable()
    {
        // resetting Fuel
        fuel = maxFuel;
    }


    public void StartFlamethrower()
    {
        playerShoot.enabled = false;
        photonView.RPC("StartFlamethrowerRPC", RpcTarget.All);
    }
    [PunRPC]
    void StartFlamethrowerRPC()
    {
        // Start/activate the Flamethrower here
        weaponHand.SwitchWeapon(3);
        weaponHand.activePrimary = 3;
    }


    private void Update()
    {
        if (canShoot && !buildingController.isBuilding && !buildingController.hasBuild) ShootUpdate();
    }

    void ShootUpdate()
    {
        // Start Flamethrower
        if(Input.GetButton("Fire1"))
        {
            photonView.RPC("UseFlamethrowerRPC", RpcTarget.All);
        }
        // Stop Flamethrower
        else if (Input.GetButtonUp("Fire1"))
        {
            photonView.RPC("StopUseFlamethrowerRPC", RpcTarget.All);
        }

        // Check if Flamethrower is empy
        if (fuel <= 0)
        {
            this.enabled = false;
            photonView.RPC("StopUseFlamethrowerRPC", RpcTarget.All);

            playerShoot.enabled = true;
            photonView.RPC("StopFlamethrowerRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    void UseFlamethrowerRPC()
    {
        // Start Flame Thrower
        if (!shootPar.isPlaying) shootPar.Play();
        fuel -= Time.deltaTime;

        // Effects
        CinemachineShake.Instance.ShakeCamera(4f, 0.1f);
        shootingSound.playing = true;
        shootingSound.source.pitch = 1 + ((1 - (fuel / maxFuel)) * 3);
    }

    [PunRPC]
    void StopUseFlamethrowerRPC()
    {
        shootPar.Stop();
        shootingSound.Stop();
        shootingSound.playing = false;      
    }

    [PunRPC]
    void StopFlamethrowerRPC()
    {
        // Finally End the Flamethrower for all Players
        weaponHand.SwitchWeapon(1);
        weaponHand.activePrimary = 1;
    }
}
