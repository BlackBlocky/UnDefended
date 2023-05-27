using UnityEngine;

public class BossDestructibleDestroyer : MonoBehaviour
{
    public float destructibleDestroyRange = 3f;
    public Vector3 destructibleDestroyRangeOffset;
    public LayerMask destructibleLayer;

    private void FixedUpdate()
    {
        // Check if a Destructible is in destructibleDestroyRange Range
        // If this is the case, destroy the Destructible
        Collider[] coll = Physics.OverlapSphere(transform.position + destructibleDestroyRangeOffset, destructibleDestroyRange, destructibleLayer);
        if(coll.Length > 0)
        {
            // Destroy all Destrucibles that in the Range
            foreach (Collider item in coll)
            {
                if (!item.gameObject.CompareTag("Destructible")) continue;
                DestructibleDestroyer.Instance.DestroyDestructible(item.gameObject.GetComponent<DestructibleScript>().Id);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + destructibleDestroyRangeOffset, destructibleDestroyRange);
    }
}
