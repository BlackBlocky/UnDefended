using Photon.Pun;
using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int id = 0;
    public string resourcesName;

    public bool isBoss = false;
    public int bossDeadAction = 0;

    public float health = 100f;
    public bool isDead = false;

    public bool damageable = true;

    public SoundPlayer damageSound;

    private void OnTriggerEnter(Collider other)
    {
        // Check if bullet and bullet owner
        GameObject bullet = other.gameObject;
        if(bullet.tag == "Bullet" && damageable)
        {
            // Check if the Player that executes the Script is the Bullet owner
            if (bullet.GetComponent<BulletScript>().ownerId == PhotonNetwork.LocalPlayer.UserId)
            {
                // Add Damge to Zombie and Destroy bullet               
                if(!bullet.GetComponent<BulletScript>().byBuilding) CinemachineShake.Instance.ShakeCamera(5, 0.2f);
                if(!isBoss) ZombieManager.Instance.AddDamageToZombie(id ,bullet.GetComponent<BulletScript>().damage);
                else BossManager.Instance.AddDamageToBoss(id, bullet.GetComponent<BulletScript>().damage, bossDeadAction);
                BulletDestroyer.Instance.DestroyBullet(bullet.GetComponent<BulletScript>().id);
            }
        }

        
    }

    private void OnParticleCollision(GameObject other)
    {
        if (isDead) return;

        // Check if flamethrower hit
        GameObject flame = other;
        if (flame.tag == "Flamethrower" && damageable && health > 0)
        {
            // Check if marster client
            if (PhotonNetwork.IsMasterClient)
            {
                // Add Damage to Zombie
                if (!isBoss) ZombieManager.Instance.AddDamageToZombie(id, 20);
                else BossManager.Instance.AddDamageToBoss(id, 20, bossDeadAction);
            }
        }
    }

    public void AddDamage(float damage)
    {
        // Add Damage
        health -= damage;

        // Do Effects
        if (health <= 0) DeadEffect();
        else DamageEffect();
    }

    void DamageEffect()
    {
        damageSound.PlaySingle();
    }

    public void DeadEffect()
    {
        GameObject s = Instantiate(Resources.Load<GameObject>("Sounds/ZombieDeadSound"));
        s.GetComponent<SoundPlayer>().PlaySingleAndDestroy();
    }

}
