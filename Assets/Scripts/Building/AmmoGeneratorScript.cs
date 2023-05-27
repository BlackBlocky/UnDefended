using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AmmoGeneratorScript : MonoBehaviour
{
    public float makeTime = 30f;
    public float collectDistance = 2f;
    public bool isRunning = true;

    public SoundPlayer ammoGotSound;
    public SoundPlayer gotCollectibleSound;
    public Animator anim;
    public MeshRenderer progressMesh;

    bool isCollectible = false;
    float timeLeft;

    GameObject myPlayer;

    
    void Awake()
    {
        timeLeft = makeTime;
    }

    private void Start()
    {
        // Get LocalPlayer PlayerOb
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i].GetComponent<PhotonView>().Owner.UserId == PhotonNetwork.LocalPlayer.UserId)
            {
                myPlayer = allPlayers[i];
                break;
            }
        }
    }

    void Update()
    {
        if (isRunning) WorkingUpdate();
        else if (isCollectible) IdleUpdate(); 
    }

    void WorkingUpdate()
    {
        // Update Timer
        timeLeft -= Time.deltaTime;
        if(timeLeft <= 0)
        {
            if(PhotonNetwork.IsMasterClient) GetComponent<PhotonView>().RPC("OnGotCollectibleRPC", RpcTarget.All);           
        }

        // Update Color
        Color newColor = new Color();
        float colorVal = timeLeft / makeTime;

        newColor.g = (1f - colorVal) * 2;
        newColor.r = colorVal * 2;
        newColor.b = 0f;
        newColor.a = 1f;

        progressMesh.material.color = newColor;
    }

    [PunRPC]
    void OnGotCollectibleRPC()
    {
        // This Happens, when the AmmoGen are ready to collect
        isCollectible = true;
        isRunning = false;

        gotCollectibleSound.PlaySingle();

        anim.SetBool("isWorking", false);
        StartCoroutine(CollectibleBlink());
    }

    void IdleUpdate()
    {
        if(Vector3.Distance(transform.position, myPlayer.transform.position) <= collectDistance)
        {
            myPlayer.GetComponent<PlayerShoot>().ammo += 50;
            myPlayer.GetComponent<PlayerShoot>().RefreshAmmoTxt();

            ammoGotSound.PlaySingle();
            GetComponent<PhotonView>().RPC("RestartGenRPC", RpcTarget.All);
        }
    }

    IEnumerator CollectibleBlink()
    {
        Color newColor = new Color();
        newColor.g = 2;
        newColor.r = 0;
        newColor.b = 0;
        newColor.a = 1;
        progressMesh.material.color = newColor;

        yield return new WaitForSeconds(0.5f);
        newColor = new Color();
        newColor.g = 2;
        newColor.r = 2;
        newColor.b = 2;
        newColor.a = 1;
        progressMesh.material.color = newColor;

        yield return new WaitForSeconds(0.5f);
        if(isCollectible) StartCoroutine(CollectibleBlink());
    }

    [PunRPC]
    void RestartGenRPC()
    {
        timeLeft = makeTime;

        isCollectible = false;
        isRunning = true;

        anim.SetBool("isWorking", true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectDistance);
    }
}
