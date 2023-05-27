using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Mathematics;
using UnityEngine;

public class BombPwUpScript : MonoBehaviour
{
    public float expandSpeed = 40f;

    public ParticleSystem explPar;
    public ParticleSystem smokePar;
    public SoundPlayer sound;

    float collSize = 0.5f;
    bool isActive = false;

    void Start()
    {
        DoExplosion();
    }

    private void Update()
    {
        if (isActive)
        {
            collSize = Mathf.Clamp(collSize + expandSpeed * Time.deltaTime, 0, 32);
            GetComponent<SphereCollider>().radius = collSize;
            if(collSize >= 32)
            {
                GetComponent<SphereCollider>().enabled = false;
            }
        }
    }

    public void DoExplosion()
    {

        isActive = true;

        // Effects
        CinemachineShake.Instance.ShakeCamera(40, 1f);
        //sound.PlaySingle();
        StartCoroutine(Effects());

        if (PhotonNetwork.IsMasterClient) StartCoroutine(DestroyWait());
    }

    IEnumerator Effects()
    {
        explPar.Play();

        yield return new WaitForSeconds(0.4f);

        smokePar.Play();
    }

    IEnumerator DestroyWait()
    {
        yield return new WaitForSeconds(5f);
        PhotonNetwork.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject coll = other.gameObject;
            if (coll.tag == "Zombie")
            {
                ZombieManager.Instance.AddDamageToZombie(coll.GetComponent<ZombieHealth>().id, 1000000);
            }
        }        
    }
}
