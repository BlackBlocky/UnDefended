using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class InGameMenuScript : MonoBehaviour
{
    public bool canMusicPlay = true;
    public bool canSoundPlay = true;

    [SerializeField] private TMP_Dropdown qualDropdown;
    [SerializeField] private GameObject menu;


    private void Start()
    {
        qualDropdown.value = QualitySettings.GetQualityLevel();
    }

    private void Update()
    {
        CheckMenuSwitch();
    }

    private void CheckMenuSwitch()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        bool menuActive = !menu.activeInHierarchy;
        Cursor.visible = menuActive;
        menu.SetActive(menuActive);
    }

    public void ResumeGame()
    {
        menu.SetActive(false);
    }

    public void ChangeQuality()
    {
        QualitySettings.SetQualityLevel(qualDropdown.value);
    }

    public void SwitchMusic()
    {
        canMusicPlay = !canMusicPlay;
        SwitchSources(canMusicPlay, "music");
    }

    public void SwitchSound()
    {
        canSoundPlay = !canSoundPlay;
        SwitchSources(canSoundPlay, "sound");
    }

    #region speed
    public void SetSpeedToX(float speed)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("SetGameSpeed", RpcTarget.AllBuffered, speed);
        }
    }

    [PunRPC]
    void SetGameSpeed(float speed)
    {
        Time.timeScale = speed;
    }
    #endregion

    private void SwitchSources(bool b, string tag)
    {
        GameObject[] allSources = UnityEngine.Object.FindObjectsOfType<GameObject>();

        // Checks if active Gameobject have a Soundplayer,
        // then if it has the correct tag and then change canPlay
        foreach (GameObject item in allSources)
        {
            if (item.GetComponent<SoundPlayer>() == null) continue;
            if (item.GetComponent<SoundPlayer>().SoundType != tag) continue;

            item.GetComponent<SoundPlayer>().canPlay = b;
        }
    }
}
