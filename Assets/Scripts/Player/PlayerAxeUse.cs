using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerAxeUse : MonoBehaviour
{
    public PhotonView photonView;

    public GameObject axeOb;
    public WeaponHandFollow weaponHand;
    public Transform axeCollPoint;

    [Tooltip("Time of Attacking Animation")]
    public float attackTime;
    public float hitRadius = 0.5f;
    public float cooldown = 0.2f;

    public Animator anim;

    GameObject gameController;

    bool isAttacking = false;
    bool hitted = false;
    bool isCooldown = false;

    private void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController");
    }

    void Update()
    {
        // Check for attack Update
        if(Input.GetButton("Axe") && !isCooldown && GetComponent<PhotonView>().IsMine)
        {
            // Start Attack
            isAttacking = true;
            isCooldown = true;           
            photonView.RPC("SyncAttackAnimRPC", RpcTarget.All);            
        }

        // Attack Update
        if (isAttacking && axeOb.activeInHierarchy) AttackUpdate();
    }

    [PunRPC]
    void SyncAttackAnimRPC()
    {
        StartCoroutine(AttackTimeTimer());

        anim.SetBool("attack", true);
        weaponHand.SwitchWeapon(2);
    }

    List<GameObject> hittedZombies;
    void AttackUpdate()
    {
        // Check for Collision
        Collider[] colls = Physics.OverlapSphere(axeCollPoint.position, hitRadius);
        if (colls.Length > 0 && !hitted)
        {
            foreach (Collider item in colls)
            {
                GameObject go = item.gameObject;
                if(go.tag == "Destructible")
                {
                    hitted = true;
                    photonView.RPC("AddDamageToDestructibleRPC", RpcTarget.All, go.GetComponent<DestructibleScript>().Id);

                    CinemachineShake.Instance.ShakeCamera(10, 0.25f);
                    break;
                }
                else if(go.tag == "Zombie" && !hittedZombies.Contains(go))
                {
                    ZombieManager.Instance.AddDamageToZombie(go.GetComponent<ZombieHealth>().id, 90);
                    GetComponent<PhotonView>().RPC("AddKnockbackRPC", RpcTarget.All, go.GetComponent<ZombieHealth>().id); 
                    hittedZombies.Add(go);

                    CinemachineShake.Instance.ShakeCamera(6, 0.1f);
                    break;
                }
            }
        }
        else return;
    }

    [PunRPC]
    void AddDamageToDestructibleRPC(int goId)
    {
        // Get Active Destructible
        GameObject[] destructibles = GameObject.FindGameObjectsWithTag("Destructible");
        GameObject go = null;
        foreach (GameObject item in destructibles) if(item.GetComponent<DestructibleScript>().Id == goId) { go = item; break; }

        // Add Damge or Destroy Destructible
        string result = go.GetComponent<DestructibleScript>().AddDamage(10);

        if (!photonView.IsMine) return; // If not host of the RPC, stop at this point

        // Do this only at the Client, that destroyed the Destructible
        if(result == "Alive") return;
        else if(result == "Dead")
        {            
            // Destrucible Destroyed
            gameController.GetComponent<BuildingController>().AddBuildingCoins(go.GetComponent<DestructibleScript>().materials);
            DestructibleDestroyer.Instance.DestroyDestructible(go.GetComponent<DestructibleScript>().Id);
        }
    }
    
    IEnumerator AttackTimeTimer()
    {
        yield return new WaitForSeconds(attackTime);
        isAttacking = false;
        hitted = false;
        hittedZombies = new List<GameObject>();

        yield return new WaitForSeconds(cooldown); // Little offset as Colldown
        isCooldown = false;
        weaponHand.SwitchWeapon(weaponHand.activePrimary);
    }

    [PunRPC]
    void AddKnockbackRPC(int id)
    {
        GameObject[] allZombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject item in allZombies)
        {
            if(item.GetComponent<ZombieHealth>().id == id)
            {
                item.GetComponent<Rigidbody>().AddForce((transform.forward + Vector3.up) * 3f, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(axeCollPoint.position, hitRadius);
    }
}
