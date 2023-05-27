using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ZombieMovement : MonoBehaviour
{
    public float speed = 1;
    public float smoothLook = 6f;

    public bool canMove = true;
    public bool canLook = true;

    PowerUpManager powerUpManager;

    private void Start()
    {
        powerUpManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<PowerUpManager>();
    }

    void Update()
    {
        if(canMove && !powerUpManager.isFreeze) MoveUpdate();

        if(canLook) LookToTargetUpdate();
    }

    private void MoveUpdate()
    {
        // Walk to Target
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void LookToTargetUpdate()
    {
        // Finally Look to Mouse
        Vector3 diff = Vector3.zero - transform.position;
        diff.y = 0;
        var rotation = Quaternion.LookRotation(diff);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * smoothLook);
    }
}
