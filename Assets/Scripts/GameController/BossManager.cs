using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using Photon.Realtime;

public class BossManager : MonoBehaviourPunCallbacks, IPunObservable
{
    class BossData
    {
        public Vector3 pos;
        public int id;
    }

    public static BossManager Instance;

    public List<GameObject> activeBosses = new List<GameObject>();

    PhotonView myPhotonView;

    private void Awake()
    {
        Instance = this;
        myPhotonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            if (PhotonNetwork.IsMasterClient) SpawnBoss("HeadBoss", Vector3.up * 10, Random.Range(123123, 1231212313));
        }
    }

    #region DataSync
    // If new Player Join, send him Data of Zombies
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Spawn Every Zombie in List
            for (int i = 0; i < activeBosses.Count; i++)
            {
                myPhotonView.RPC("AddBossToListRPC", newPlayer, activeBosses[i].GetComponent<ZombieHealth>().resourcesName,
                    activeBosses[i].transform.position.x, activeBosses[i].transform.position.y, activeBosses[i].transform.position.z,
                    activeBosses[i].GetComponent<ZombieHealth>().id);
            }
        }
    }
    [PunRPC]
    void AddBossToListRPC(string name, float x, float y, float z, int id)
    {
        // Do the Spawn RPC but only the Joined player not all clients
        SpawnBossRPC(name, x, y, z, id);
    }

    // Sync Zombie Pos
    List<BossData> bossDatas = new List<BossData>();
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this
            stream.SendNext(activeBosses.Count);
            for (int i = 0; i < activeBosses.Count; i++)
            {
                stream.SendNext(activeBosses[i].transform.position.x);
                stream.SendNext(activeBosses[i].transform.position.y);
                stream.SendNext(activeBosses[i].transform.position.z);
                stream.SendNext(activeBosses[i].GetComponent<ZombieHealth>().id);
            }
        }
        else
        {
            // We get this from writer
            int listCount = (int)stream.ReceiveNext();

            bossDatas.Clear();
            for (int i = 0; i < listCount; i++)
            {
                bossDatas.Add(new BossData());
                bossDatas[i].pos.x = (float)stream.ReceiveNext();
                bossDatas[i].pos.y = (float)stream.ReceiveNext();
                bossDatas[i].pos.z = (float)stream.ReceiveNext();
                bossDatas[i].id = (int)stream.ReceiveNext();
            }

            FixBossPos();
        }
    }

    void FixBossPos()
    {
        for (int i = 0; i < bossDatas.Count; i++)
        {
            try
            {
                BossData data = bossDatas[i];
                
                // Get Active Boss
                GameObject[] zombies = GameObject.FindGameObjectsWithTag("Boss");
                GameObject go = null;
                foreach (GameObject item in zombies)
                {
                    if (item.GetComponent<ZombieHealth>().id == data.id)
                    {
                        go = item;
                        break;
                    }
                }

                // Check if offset is to high
                float maxOffset = 0.5f;
                if (Vector3.Distance(go.transform.position, data.pos) > maxOffset)
                {
                    go.transform.position = data.pos;
                }
            }
            catch { Debug.LogWarning("Boss For Pos Sync not found, with id: " + bossDatas[i].id); }
        }
    }
    #endregion

    public void SpawnBoss(string name, Vector3 pos, int id)
    {
        myPhotonView.RPC("SpawnBossRPC", RpcTarget.AllViaServer, name, pos.x, pos.y, pos.z, id);
    }

    public void DestroyBoss(int id, int deadAction)
    {
        myPhotonView.RPC("DestroyBossRPC", RpcTarget.All, id, deadAction);
    }

    public void AddDamageToBoss(int id, float damage, int deadAction)
    {
        myPhotonView.RPC("AddDamageToBossRPC", RpcTarget.All, id, damage, deadAction);
    }

    [PunRPC]
    void SpawnBossRPC(string name, float x, float y, float z, int id)
    {
        // Spawn Boss at all Clients as Local
        GameObject s = Instantiate(Resources.Load<GameObject>("Boss/" + name), new Vector3(x, y, z), Quaternion.identity);
        s.GetComponent<ZombieHealth>().id = id;
        s.GetComponent<ZombieHealth>().resourcesName = name;

        // Add To List
        activeBosses.Add(s);
    }

    [PunRPC]
    void DestroyBossRPC(int id, int deadAction)
    {
        // Get Zombie with id
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        foreach (GameObject item in bosses)
        {
            if (item.GetComponent<ZombieHealth>().id == id)
            {
                switch(deadAction)
                {
                    case 0: // Normal Death
                        activeBosses.Remove(item);
                        Destroy(item);
                        break;
                    case 1: // Head Boss Slpit                       
                        // Spawn the slpits
                        Vector3 rightDire = item.transform.right;
                        Vector3 itemPos = item.transform.position;
                        if (PhotonNetwork.IsMasterClient) myPhotonView.RPC("SpawnHeadBossSplitsRPC", RpcTarget.AllViaServer, itemPos.x, itemPos.y, itemPos.z,
                            rightDire.x, rightDire.y, rightDire.z, Random.Range(-2147483647, 2147483647), Random.Range(-2147483647, 2147483647));

                        // Destroy the BigZombie
                        activeBosses.Remove(item);
                        Destroy(item);
                        break;
                }                
                return;
            }
        }
    }
    IEnumerator BossSpawnSpawnProtection(GameObject s)
    {
        yield return new WaitForSeconds(1f);
        s.GetComponent<BoxCollider>().enabled = true;
    }

    [PunRPC]
    void AddDamageToBossRPC(int id, float damage, int deadAction)
    {
        // Get the Target Boss
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        GameObject go = null;
        foreach (GameObject item in bosses)
        {
            if(item.GetComponent<ZombieHealth>().id == id)
            {
                go = item;
                break;
            }
        }
        if(go == null) // Error
        {
            Debug.LogError("Boss with ID: " + id + " not found - Tried to add Damage to unfound Boss!");
            return;
        }

        // Add Damage to him
        go.GetComponent<ZombieHealth>().AddDamage(damage);
        if (go.GetComponent<ZombieHealth>().health <= 0 && go.GetComponent<ZombieHealth>().isDead == false)
        {
            go.GetComponent<ZombieHealth>().DeadEffect();
            go.GetComponent<ZombieHealth>().isDead = true;
            if (PhotonNetwork.IsMasterClient) DestroyBoss(id, deadAction);
        }
    }



    // Boss Specific Methods
    [PunRPC]
    void SpawnHeadBossSplitsRPC(float rootX, float rootY, float rootZ, float rightX, float rightY, float rightZ, int id1, int id2)
    {
        Vector3 bigZombiePos = new Vector3(rootX, rootY, rootZ);
        

        for (int i = -1; i < 2; i += 2)
        {
            // Spawn the Small HeadBosses
            GameObject s = Instantiate(Resources.Load<GameObject>("Boss/SmallHeadBoss"));
            s.transform.position = bigZombiePos;

            Vector3 rightDire = new Vector3(rightX, rightY, rightZ) * i;
            
            s.transform.Translate(rightDire * 2 + Vector3.up * 2);

            s.GetComponent<Rigidbody>().AddForce(rightDire * 5f, ForceMode.VelocityChange);
            s.GetComponent<Rigidbody>().AddForce(Vector3.up * 15f, ForceMode.VelocityChange);

            if(i == -1) s.GetComponent<ZombieHealth>().id = id1;
            else s.GetComponent<ZombieHealth>().id = id2;
            s.GetComponent<ZombieHealth>().resourcesName = "SmallHeadBoss";

            activeBosses.Add(s);

            s.GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(BossSpawnSpawnProtection(s));
        }
    }
}
