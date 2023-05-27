using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public string ownerId;
    public int id;

    public float speed = 12f;
    public float damage = 20f;

    public bool byBuilding = false;

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
