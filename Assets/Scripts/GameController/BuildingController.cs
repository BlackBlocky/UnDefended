using Photon.Pun;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Collections;

public class BuildingController : MonoBehaviour
{
    public GameObject myPlayer;

    public float buildMat = 0;

    public float blockSize = 2f;

    public Transform buildPoint;
    public Transform aimDot;
    public TextMeshPro costTxt;
    
    public LayerMask sortingLayer;

    public SoundPlayer cantBuildSound;
    public SoundPlayer switchBuildingSound;
    public SoundPlayer buildBuildingSound;

    public GameObject[] blueprints;
    public Material[] feedbackMat;
    public MeshRenderer[] myRenderer;
    public string[] blueprintName;
    public int[] blueprintCost;

    public bool isBuilding;
    public bool hasBuild = false;

    int activeBp = 1;

    void Update()
    {
        // Check for Player Input
        if(Input.GetButtonDown("StartBuild") && !buildPoint.gameObject.activeInHierarchy)
        {
            // Reset Active Bp
            activeBp = 1;
            ChangeActiveBuilding(1, true);

            // Start BuildLoop
            isBuilding = true;
            buildPoint.gameObject.SetActive(true);            
        }
        else if(Input.GetButtonDown("StartBuild") && buildPoint.gameObject.activeInHierarchy) { StopBuilding(); }
        else if (Input.GetKeyDown(KeyCode.Mouse1) && buildPoint.gameObject.activeInHierarchy) { StopBuilding(); }

        if (isBuilding) BuildUpdate();
    }

    bool alreadyBuild = false;
    bool triedBuild= false; // Check Bool for controller
    void BuildUpdate()
    {
        // Set Build Point Pos
        Vector3 newPos = new Vector3();
        newPos.x = aimDot.position.x;
        newPos.z = aimDot.position.z;

        buildPoint.position = newPos;

        // Update CostTxt
        costTxt.text = $"{blueprintCost[activeBp].ToString("0")}\n[{buildMat.ToString("0")}]";

        // Check if Building is possible
        bool buildPossible = true;
        Collider[] colliders = Physics.OverlapSphere(buildPoint.position, blockSize, sortingLayer, QueryTriggerInteraction.Ignore);
        if(colliders.Length > 0)
        {
            buildPossible = false;

            ChangeBuildPointMat(2);
        }
        else if(buildMat < blueprintCost[activeBp])
        {
            buildPossible = false;

            ChangeBuildPointMat(1);
        }
        else
        {
            ChangeBuildPointMat(0);
        }

        // Switch Buildings
        int switchVal = 0;
        if (Input.GetButtonDown("PwSwitchLeft")) switchVal = -1;
        if (Input.GetButtonDown("PwSwitchRight")) switchVal = 1;
        if (switchVal != 0)
        {
            ChangeActiveBuilding(switchVal);
            switchBuildingSound.PlaySingle();
        }

        // Build Building
        if(Input.GetButtonDown("Fire1") && buildPossible && !alreadyBuild ||
            Input.GetAxisRaw("Fire1") == 1 && buildPossible && !alreadyBuild)
        {
            // Remove BuildMat
            buildMat -= blueprintCost[activeBp];

            // Build
            alreadyBuild = true;
            var buildPos = buildPoint.position;
            GetComponent<PhotonView>().RPC("BuildRPC", RpcTarget.MasterClient, buildPos.x, buildPos.y, buildPos.z, activeBp);

            StopBuilding();

            buildBuildingSound.PlaySingle();
            CinemachineShake.Instance.ShakeCamera(5, 0.4f);
        }
        else if(Input.GetButtonDown("Fire1") && !buildPossible && !alreadyBuild ||
            Input.GetAxisRaw("Fire1") == 1 && !triedBuild && !buildPossible && !alreadyBuild)// Build isnt Possible
        {
            triedBuild = true;
            cantBuildSound.PlaySingle();
        }

        // resetting Fire1 Axis (for Controller)
        if (Input.GetAxisRaw("Fire1") == 0) triedBuild = false;
    }

    private void ChangeBuildPointMat(int id) // Change the Material from the all Mesh and the Color from the CostTxt
    {
        for (int i = 0; i < myRenderer.Length; i++)
        {
            for (int i1 = 0; i1 < myRenderer[i].materials.Length; i1++)
            {
                myRenderer[i].materials[i1].color = feedbackMat[id].color;
            }
        }

        costTxt.color = feedbackMat[id].color;
    }

    private void ChangeActiveBuilding(int dire, bool set = false)
    {
        // Change activeBuild
        activeBp += dire;
        if (activeBp > blueprints.Length - 1) activeBp = 1;
        else if (activeBp < 1) activeBp = blueprints.Length - 1;

        if (set == true) activeBp = dire;

        // Change Visual
        foreach (GameObject item in blueprints)
        {
            if(item != null) item.SetActive(false);
        }
        blueprints[activeBp].SetActive(true);
    }

    [PunRPC]
    void BuildRPC(float posX, float posY, float posZ, int buildId)
    {
        Vector3 spawnPos = new Vector3(posX, posY, posZ);
        PhotonNetwork.InstantiateRoomObject("Building/" + blueprintName[buildId], spawnPos, Quaternion.identity);
    }

    public void StopBuilding()
    {
        isBuilding = false;
        buildPoint.gameObject.SetActive(false);
        alreadyBuild = false;

        hasBuild = true;
        StartCoroutine(HasBuildReset());
    }

    IEnumerator HasBuildReset()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        hasBuild = false;
    }

    public void AddBuildingCoins(int amount)
    {
        // Add Building Coins and Show a added Text above the Player
        buildMat += amount;

        GameObject addedTxt = GetComponent<QuickLogin>().MYPLAYER.transform.Find("AddedTxt").gameObject;
        GameObject s = Instantiate(addedTxt);
        s.transform.parent = null;
        s.transform.position = addedTxt.transform.position;
        s.transform.LookAt(Camera.main.transform);
        s.transform.Rotate(-180, 0, -180);
        s.GetComponent<TextMeshPro>().text = "+" + amount.ToString("0") + " [" + buildMat + "]";
        s.GetComponent<MeshRenderer>().enabled = true;
        
        StartCoroutine(MoveAddedTextLoop(s, 2f, 0));
    }

    IEnumerator MoveAddedTextLoop(GameObject addedText, float speed, float moveProgress)
    {
        if (moveProgress < 1f)
        {
            yield return new WaitForEndOfFrame();
            moveProgress += Time.deltaTime;
            addedText.transform.position += Vector3.up * speed * Time.deltaTime;
            StartCoroutine(MoveAddedTextLoop(addedText, speed, moveProgress));
        }
        else
        {
            Destroy(addedText);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(buildPoint.position, blockSize);
    }

}
