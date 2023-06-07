using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3f;
    public float smoothLook = 0.1f;

    public float sensi = 1f;
    public float slowSensi = 0.5f;

    public bool canMove = true;
    public bool canLook = true;

    public GameObject aimDot;
    public LayerMask aimDotColliderLayers;

    public Animator anim;

    private void Awake()
    {
        Cursor.visible = false;

        // Check if Player is mine
        if (!GetComponent<PhotonView>().IsMine)
        {
            canMove = false;
            canLook = false;
        }

        // Set Aim Dot
        aimDot = GameObject.Find("AimDot");
    }

    void Update()
    {
        if (canMove) MoveUpdate();
        if (canLook) LookToMouseUpdate();

        // Print active Photon views with name
        /*
        int count = 0;
        foreach (var item in PhotonNetwork.PhotonViewCollection)
        {
            print(item.name);
            count++;
        }
        print(count);
        */
    }

    private void MoveUpdate()
    {
        // -- Move
        // Calc move dire and Move
        Vector3 moveDire = Vector3.zero;
        moveDire += Input.GetAxisRaw("Horizontal") * Vector3.right;
        moveDire += Input.GetAxisRaw("Vertical") * Vector3.forward;
        moveDire.Normalize();
        transform.Translate(-moveDire * speed * Time.deltaTime, Space.World);

        // Animation
        if(moveDire != Vector3.zero) anim.SetInteger("walk", 1);
        else anim.SetInteger("walk", 0);

        // Animation setting multipliyer by dire
        // if (moveDire.z < 0) anim.SetFloat("walkMulti", -1);
        // else anim.SetFloat("walkMulti", 1);
    }

    private void LookToMouseUpdate()
    {
        // Check if Input is Key or Controller
        if(Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            // -- Look to mouse
            // Get Mouse Pos
            Vector3 mWorldPos = Vector3.zero;
            Plane plane = new Plane(Vector3.up, 0);
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out distance))
            {
                mWorldPos = ray.GetPoint(distance);
            }

            // Set Aim Dot pos
            aimDot.transform.position = mWorldPos;
        }
        else if(Input.GetAxis("ConMouse X") != 0 || Input.GetAxis("ConMouse Y") != 0)
        {
            float sensi = this.sensi;
            if (Input.GetAxisRaw("SlowAim") == 1) sensi *= slowSensi;

            Vector3 moveDire = Vector3.zero;
            moveDire.x = -Input.GetAxis("ConMouse X");
            moveDire.z = Input.GetAxis("ConMouse Y");
            moveDire.Normalize();

            aimDot.transform.Translate(moveDire * 25 * sensi * Time.unscaledDeltaTime);
        }

        // Fix aimdot Pos by making it on top of everything
        //RaycastHit hit;
        //Physics.Raycast(aimDot.transform.position + Vector3.up * 100, Vector3.down, out hit, Mathf.Infinity, aimDotColliderLayers, QueryTriggerInteraction.Ignore);
        //aimDot.transform.position = hit.point;

        // Finally Look to Mouse
        Vector3 diff = aimDot.transform.position - transform.position;
        diff.y = 0;
        var rotation = Quaternion.LookRotation(diff);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * smoothLook);
    }

    private void OnDrawGizmos()
    {
        RaycastHit hit;
        Physics.Raycast(aimDot.transform.position + Vector3.up * 100, Vector3.down, out hit, Mathf.Infinity, aimDotColliderLayers, QueryTriggerInteraction.Ignore);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(aimDot.transform.position + Vector3.up * 100, hit.point);
    }
}
