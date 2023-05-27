using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

class ZombieData
{
    public Vector3 pos;
    public int id;
}

public class ZombieManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static ZombieManager Instance;

    public PhotonView myPhotonView;

    public List<GameObject> activeZombies;

    private void Awake()
    {
        Instance = this;
    }


    #region Network Sync
    // If new Player Join, send him Data of Zombies
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Spawn Every Zombie in List
            for (int i = 0; i < activeZombies.Count; i++)
            {
                myPhotonView.RPC("AddZombieToListRPC", newPlayer, activeZombies[i].GetComponent<ZombieHealth>().resourcesName,
                    activeZombies[i].transform.position.x, activeZombies[i].transform.position.y, activeZombies[i].transform.position.z,
                    activeZombies[i].GetComponent<ZombieHealth>().id);
            }            
        }
    }
    [PunRPC]
    void AddZombieToListRPC(string name, float x, float y, float z, int id)
    {
        // Do the Spawn RPC but only the Joined player not all clients
        SpawnZombieRPC(name, x, y, z, id);
    }

    // Sync Zombie Pos
    List<ZombieData> zombieDatas = new List<ZombieData>();
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this
            stream.SendNext(activeZombies.Count);
            for (int i = 0; i < activeZombies.Count; i++)
            {
                stream.SendNext(activeZombies[i].transform.position.x);
                stream.SendNext(activeZombies[i].transform.position.y);
                stream.SendNext(activeZombies[i].transform.position.z);
                stream.SendNext(activeZombies[i].GetComponent<ZombieHealth>().id);
            }
        }
        else
        {
            // We get this from writer
            int listCount = (int)stream.ReceiveNext();

            zombieDatas.Clear();
            for (int i = 0; i < listCount; i++)
            {
                zombieDatas.Add(new ZombieData());
                zombieDatas[i].pos.x = (float)stream.ReceiveNext();
                zombieDatas[i].pos.y = (float)stream.ReceiveNext();
                zombieDatas[i].pos.z = (float)stream.ReceiveNext();
                zombieDatas[i].id = (int)stream.ReceiveNext();
            }

            FixZombiePos();
        }
    }

    void FixZombiePos()
    {
        for (int i = 0; i < zombieDatas.Count; i++)
        {
            try
            {
                ZombieData data = zombieDatas[i];

                // Get Active Zombie
                GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
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
            catch { Debug.LogWarning("Zombie For Pos Sync not found, with id: " + zombieDatas[i].id); }
        }       
    }
    #endregion


    public void SpawnZombie(string name, float x, float y, float z, int id)
    {
        myPhotonView.RPC("SpawnZombieRPC", RpcTarget.AllViaServer, name, x, y, z, id);
    }

    public void DestroyZombie(int id)
    {
        myPhotonView.RPC("DestroyZombieRPC", RpcTarget.All, id);
    }

    public void AddDamageToZombie(int id, float damage)
    {
        myPhotonView.RPC("AddDamageToZombieRPC", RpcTarget.All, id, damage);
    }

    [PunRPC]
    void SpawnZombieRPC(string name, float x, float y, float z, int id)
    {
        // Spawn Zombie at all Clients as Local
        GameObject s = Instantiate(Resources.Load<GameObject>(name), new Vector3(x, y, z), Quaternion.identity);
        s.GetComponent<ZombieHealth>().id = id;
        s.GetComponent<ZombieHealth>().resourcesName = name;

        // Add To List
        activeZombies.Add(s);
    }

    [PunRPC]
    void DestroyZombieRPC(int id)
    {
        // Get Zombie with id
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject item in zombies)
        {
            if (item.GetComponent<ZombieHealth>().id == id)
            {
                activeZombies.Remove(item);
                Destroy(item);
                return;
            }
        }
    }

    [PunRPC]
    void AddDamageToZombieRPC(int id, float damage)
    {
        // Get Active Zombie
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        GameObject go = null;
        foreach (GameObject item in zombies)
        {
            if(item.GetComponent<ZombieHealth>().id == id)
            {
                go = item;
                break;
            }
        }

        // Add Damage
        go.GetComponent<ZombieHealth>().AddDamage(damage);
        if(go.GetComponent<ZombieHealth>().health <= 0)
        {
            go.GetComponent<ZombieHealth>().DeadEffect();
            if(PhotonNetwork.IsMasterClient) DestroyZombie(id);
        }
    }
}
