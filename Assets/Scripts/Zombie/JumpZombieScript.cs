using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpZombieScript : MonoBehaviour
{
    [SerializeField] private float jumpDelay = 2f;
    [SerializeField] private float jumpStregth = 10f;

    [SerializeField] private Rigidbody rb;

    void Start()
    {
        StartCoroutine(JumpLoop());
    }

    IEnumerator JumpLoop()
    {
        // Wait for next Jump
        yield return new WaitForSeconds(jumpDelay);

        // Jump
        rb.AddForce(Vector3.up * jumpStregth, ForceMode.Impulse);
        StartCoroutine(JumpLoop());
    }
}