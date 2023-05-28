using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;

public class TurretScript : MonoBehaviour
{
    public float lookSpeed = 8f;
    public float cooldown = 3f;
    public float ammo = 50;

    public float ammoAddSpeed = 5f;
    public float ammoAddRange = 2.5f;

    public Transform lookBone;
    public Transform bulletSpawn;
    public PhotonView photonView;
    public SkinnedMeshRenderer myRenderer;

    public SoundPlayer shootSound;
    public SoundPlayer ammoOutSound;
    public SoundPlayer ammoAddSound;
    public float shootAnimSpeed = 1f;
    //public Animator anim;

    Transform target;
    float activeCooldown;
    float maxAmmo;

    public bool canLook = true;
    public bool canShoot = true;
    public bool canReload = true;

    private void Start()
    {
        activeCooldown = cooldown;
        maxAmmo = ammo;
    }

    void Update()
    {
        // Check if Target exists
        if (PhotonNetwork.IsMasterClient)
        {
            if (target == null) try { target = FindNewTarget().transform; } catch { }
            else
            {
                if (canLook && ammo > 0) LookUpdate();
                if (canShoot && ammo > 0) ShootUpdate();                
            }
        }

        // Reload (any Player)
        if (canReload && ammo < maxAmmo) AmmoAddUpdate();
    }

    void LookUpdate()
    {
        // Look To target
        Vector3 diff = target.transform.position - lookBone.position;
        diff.y = 0;
        var rotation = Quaternion.LookRotation(diff);
        lookBone.rotation = Quaternion.Slerp(lookBone.rotation, rotation, Time.deltaTime * lookSpeed);
    }

    void ShootUpdate()
    {
        if (activeCooldown == 0 && ammo > 0)
        {
            // Shoot
            photonView.RPC("ShootBulletRPC", RpcTarget.All, PhotonNetwork.LocalPlayer.UserId,
                bulletSpawn.position.x, bulletSpawn.position.y, bulletSpawn.position.z,
                lookBone.rotation.x, lookBone.rotation.y, lookBone.rotation.z, lookBone.rotation.w,
                 Random.Range(-2147483648, 2147483647));          

            // Effects          
            photonView.RPC("DoShootAimRPC", RpcTarget.All);

            // Sync
            photonView.RPC("RefreshAmmoToOthersRPC", RpcTarget.Others, ammo);

            // Do Stuff
            activeCooldown = cooldown;

            ammo = Mathf.Clamp(ammo -= 1, 0, maxAmmo);
            photonView.RPC("RecolorAmmoBoxRPC", RpcTarget.All, ammo, maxAmmo);
            if (ammo <= 0) ammoOutSound.PlaySingle();

        }
        else activeCooldown = Mathf.Clamp(activeCooldown - Time.deltaTime, 0, cooldown);
    }

    int lastAmmoInt = 0;
    void AmmoAddUpdate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ammoAddRange);
        foreach (var item in hitColliders)
        {
            GameObject activeOb = item.gameObject;
            if(activeOb.tag == "Player")
            {
                if (activeOb.GetComponent<PhotonView>().IsMine && activeOb.GetComponent<PlayerShoot>().ammo > 0)
                {
                    // Remove Ammo from Player
                    activeOb.GetComponent<PlayerShoot>().ammo -= ammoAddSpeed * Time.deltaTime;
                    activeOb.GetComponent<PlayerShoot>().RefreshAmmoTxt();

                    // Effects
                    if (lastAmmoInt != (int)ammo)
                    {
                        ammoAddSound.PlaySingle();
                        lastAmmoInt = (int)ammo;
                    }

                    // Add Ammo at Marster Client
                    if (PhotonNetwork.IsMasterClient == false)
                    {
                        photonView.RPC("AddAmmoRPC", RpcTarget.MasterClient);
                        return;
                    }
                    else
                    {
                        AddAmmoRPC();
                        return;
                    }                   
                }
            }
        }
    }

    [PunRPC]
    void AddAmmoRPC()
    {
        // Add Ammo
        Mathf.Clamp(ammo += ammoAddSpeed * Time.deltaTime, 0, maxAmmo);
        photonView.RPC("RecolorAmmoBoxRPC", RpcTarget.All, ammo, maxAmmo);

        // Refresh ammo at others
        photonView.RPC("RefreshAmmoToOthersRPC", RpcTarget.Others, ammo);
    }

    [PunRPC]
    void RefreshAmmoToOthersRPC(float newAmmo)
    {
        ammo = newAmmo;
    }

    [PunRPC]
    void RecolorAmmoBoxRPC(float myAmmo, float myMaxAmmo)
    {   
        Color color = new Color(1 - myAmmo / myMaxAmmo, myAmmo / myMaxAmmo, 0);
        myRenderer.materials[1].color = color;
    }

    [PunRPC]
    void ShootBulletRPC(string id, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW, int bulletId)
    {
        Quaternion spawnRot = new Quaternion(rotX, rotY, rotZ, rotW);
        GameObject s = Instantiate(Resources.Load<GameObject>("Bullet"), new Vector3(posX, posY, posZ), spawnRot);
        s.GetComponent<BulletScript>().damage = 34f;
        s.GetComponent<BulletScript>().ownerId = id;
        s.GetComponent<BulletScript>().id = bulletId;
        s.GetComponent<BulletScript>().byBuilding = true;

        Destroy(s, 3f);
    }

    [PunRPC]
    void DoShootAimRPC()
    {
        shootSound.PlaySingle();

        shootAnimState = false;
        if (shootAnimStartPos == Vector3.zero) shootAnimStartPos = lookBone.transform.localPosition;
        StartCoroutine(ShootAnim(shootAnimSpeed));
    }

    bool shootAnimState = false;
    Vector3 shootAnimStartPos;
    IEnumerator ShootAnim(float leght)
    {
        if (!shootAnimState)
        {
            Vector3 targetVector = shootAnimStartPos + (-lookBone.forward * 0.4f);

            lookBone.localPosition = Vector3.Lerp(lookBone.localPosition, targetVector, leght * Time.deltaTime);
            if (V3Equal(lookBone.localPosition, targetVector)) shootAnimState=true;
        }
        else
        {
            lookBone.localPosition = Vector3.Lerp(lookBone.localPosition, shootAnimStartPos, leght * Time.deltaTime);
            if (lookBone.localPosition == shootAnimStartPos) yield break;
        }

        yield return new WaitForEndOfFrame();
        StartCoroutine(ShootAnim(leght));
    }

    // Looks if 2 Vector approximately equal
    public bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.01;
    }

    GameObject FindNewTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Zombie");
        targets = targets.Concat(GameObject.FindGameObjectsWithTag("Boss")).ToArray(); // Add the Bosses to the array

        float shortDis = Mathf.Infinity;
        GameObject shortTarget = null;

        foreach (GameObject item in targets)
        {
            float distance = Vector3.Distance(transform.position, item.transform.position);
            if(distance < shortDis)
            {
                shortDis = distance;
                shortTarget = item;
            }
        }

        return shortTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ammoAddRange);
    }
}
