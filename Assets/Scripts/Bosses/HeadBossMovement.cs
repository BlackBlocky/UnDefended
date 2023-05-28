using Photon.Pun;
using UnityEditor;
using UnityEngine;

public class HeadBossMovement : MonoBehaviour
{
    [Header("Values")]
    public float speed = 6f;
    public float jumpCooldown = 0.6f;
    public float jumpHeight = 7f;

    public float shrinkSize = 0.7f;
    public float shrinkSpeed = 2f;
    
    public enum State { Idle = 0, Big = 1, Split = 2}
    [Header("States")]
    public State state = State.Big;

    public enum AnimState { Idle = 0, Shrink = 1, Grow = 2}
    public AnimState animState = AnimState.Idle;

    public enum MoveState { Stay = 0, Fly = 1}
    public MoveState moveState = MoveState.Fly;

    [Header("References")]
    public Transform body;
    public SoundPlayer groundHitSound;

    // Locals
    Transform activeTarget;
    Rigidbody rb;
    PowerUpManager powerUpManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        powerUpManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<PowerUpManager>();
    }

    private void FixedUpdate()
    {
        if(activeTarget == null) FindNewTarget();

        if(state != State.Idle && !powerUpManager.isFreeze) FixedMovementUpdate();
    }

    private void Update()
    {
        if(animState != AnimState.Idle) AnimationUpdate();
    }

    float activeJumpCooldown = 1.5f;
    void FixedMovementUpdate()
    {
        if(moveState == MoveState.Stay) // Boss Staying on the Ground
        {
            animState = AnimState.Shrink;

            activeJumpCooldown -= 1f * Time.fixedDeltaTime;
            if(activeJumpCooldown < 0f)
            {
                activeJumpCooldown = jumpCooldown;

                // States
                moveState = MoveState.Fly;
                animState = AnimState.Grow;

                // Start Jumping
                rb.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);

                // Look at Target
                Vector3 diff = activeTarget.position - transform.position;
                diff.y = 0;
                transform.rotation = Quaternion.LookRotation(diff);

                // Refreh Target
                FindNewTarget();
            }
        }
        else // Boss is Flying
        {
            transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime);
        }
    }

    void AnimationUpdate()
    {
        if(animState == AnimState.Grow) // Boss Growing Bigger
        {
            body.localScale = new Vector3(1f, body.localScale.y + shrinkSpeed * Time.deltaTime, 1f);

            // Check if target Size is reached
            if (body.localScale.y >= 1f)
            {
                body.localScale = Vector3.one;
                animState = AnimState.Idle;
            }
        }
        else // Boss Shrinking
        {
            body.localScale = new Vector3(1f, body.localScale.y - shrinkSpeed * Time.deltaTime, 1f);

            // Check if target Size is reached
            if (body.localScale.y <= shrinkSize)
            {
                body.localScale = new Vector3(1f, shrinkSize, 1f);
                animState = AnimState.Idle;
            }
        }
    }

    void FindNewTarget()
    {
        if(state == State.Big)
        {
            // Go to the nearest Building
            GameObject[] allBuildings = GameObject.FindGameObjectsWithTag("Building");
            float closest = Mathf.Infinity;
            GameObject closestGo = null;

            foreach (GameObject item in allBuildings)
            {
                float distance = Vector3.Distance(transform.position, item.transform.position);
                if(distance < closest)
                {
                    closest = distance;
                    closestGo = item;
                }
            }
            if(closestGo != null)
            {
                activeTarget = closestGo.transform;
                return;
            }

            // Only run this, if no building was found
            // Then attack the Base
            activeTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
        else if (state == State.Split)
        {
            // Check if the other Splited is alive
            GameObject[] allSplits = GameObject.FindGameObjectsWithTag("Boss");
            foreach (GameObject item in allSplits)
            {
                if(item != this.gameObject)
                {
                    // Other Split found
                    activeTarget = item.transform;
                    return;
                }
            }

            // Only Run this, if no other Split was found
            activeTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If Boss Collided to the Ground
        // Set the moveState to Stay
        if(collision.collider.gameObject.layer == 6)
        {
            moveState = MoveState.Stay;

            // Effects
            CinemachineShake.Instance.ShakeCamera(12, 0.3f);
            groundHitSound.PlaySingle();
        }

        // Check if Boss collided with the Base
        // If this happend, add up force to for unstucking
        else if(collision.collider.gameObject.CompareTag("Base"))
        {
            rb.AddForce(Vector3.up * 1f, ForceMode.VelocityChange);
        }

        // Check if Splitted Boss Collided with his Clone
        // If this happens, combine these 2 together
        else if (collision.collider.gameObject.CompareTag("Boss"))
        {
            if(state == State.Split && PhotonNetwork.IsMasterClient)
            {
                // Check if other boss isnt Idle and already done this
                if (collision.gameObject.GetComponent<HeadBossMovement>().state == State.Idle) return;

                // Set Vars
                state = State.Idle;

                // Spawn the Big Zombie Again
                BossManager.Instance.SpawnBoss("HeadBoss", transform.position, Random.Range(-2147483647, 2147483647));

                // Destroy the 2 Zombies
                BossManager.Instance.DestroyBoss(GetComponent<ZombieHealth>().id, GetComponent<ZombieHealth>().bossDeadAction);
                BossManager.Instance.DestroyBoss(collision.gameObject.GetComponent<ZombieHealth>().id,
                    collision.gameObject.GetComponent<ZombieHealth>().bossDeadAction);
            }
        }

        // Check if Boss Collide with a Building
        // If that Happens, give the Boss nockback
        else if (collision.collider.gameObject.CompareTag("Building"))
        {
            rb.AddForce(transform.forward * -3 + transform.up * 2, ForceMode.VelocityChange);
            print("lol");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Add Constant Up Force, if Boss is Stuck of the Base
        if(collision.collider.gameObject.CompareTag("Base"))
        {
            transform.Translate(transform.up * 2f * Time.fixedDeltaTime);
        }
    }
}
