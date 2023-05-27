using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseManager : MonoBehaviourPunCallbacks
{ 
    public float health = 100;

    public TextMeshPro dmgText;
    public SoundPlayer damageSound;
    public SoundPlayer healthSound;

    public ParticleSystem[] healthPar;

    public PhotonView myPhotonView;

    #region damage
    public void AddDamage(float amount)
    {
        // Start RPC to all Players
        myPhotonView.RPC("AddDamageRPC", RpcTarget.All, health - amount);
    }
    [PunRPC]
    void AddDamageRPC(float newHealth)
    {
        // Add Damge to all Players
        this.health = newHealth;
        DamageEffects();
        
        // Check if GameOver -- DAS IST VORRÜBERGEHEND SOLANGE NOCH KEIN GAMEOVER GEADDED IST SONST SOLL DAS DER MARSTER CLIENT ALLES MACHEN
        if (health <= 0)
        {
            Application.Quit();
        }
    }

    void DamageEffects()
    {
        // Damage Effects
        CinemachineShake.Instance.ShakeCamera(15, 0.3f);
        dmgText.text = health.ToString("000");
        damageSound.PlaySingle();
    }
    #endregion


    #region health
    public void AddHealth(float amount)
    {
        // Start RPC to everyone
        myPhotonView.RPC("AddHealthRPC", RpcTarget.MasterClient, amount);
    }
    [PunRPC]
    void AddHealthRPC(float amount)
    {
        // Add
        health += amount;

        // Effects
        HealthEffects();
    }

    void HealthEffects()
    {
        // Health Effects
        healthSound.PlaySingle();
        foreach (ParticleSystem item in healthPar) item.Play();
        dmgText.text = health.ToString("000");
    }
    #endregion

    // When a new player joining the Room, send him the active health (if marsterclient)
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient) myPhotonView.RPC("AddDamageRPC", newPlayer, health);
    }
}
