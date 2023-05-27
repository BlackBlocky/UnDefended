using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleScript : MonoBehaviour
{
    public int Id;
    public int materials = 5;
    public float health = 100;
    public float sizePerHit = -.05f;
    public Animator myAnimator;
    public SoundPlayer damageSound;

    public GameObject deadGo;

    private void Awake()
    {
        //Id = Random.Range(-2147483648, 2147483647);
        Id = transform.GetSiblingIndex();
    }

    public string AddDamage(float damage)
    {
        // Add Damage and Retrun if dead or alive
        health -= damage;
        myAnimator.SetBool("damage", true);
        damageSound.PlaySingle();

        var size = transform.localScale;
        Vector3 newSize = new Vector3(size.x + sizePerHit, size.y + sizePerHit, size.z + sizePerHit);
        transform.localScale = newSize;

        if (health <= 0)
        {
            return "Dead";
        }
        else return "Alive";
    }
}
