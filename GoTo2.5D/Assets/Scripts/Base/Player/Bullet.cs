using UnityEngine;

// ==================== 极简弹幕脚本（带消失开关） ====================
// 挂载到子弹预制体上，负责碰撞触发玩家受伤和击退
public class SimpleBullet : MonoBehaviour
{
    [Header("弹幕属性")]
    public int damage = 20;              // 伤害值
    public float knockbackForce = 8f;    // 击退力度（会被传递到玩家）
    
    [Header("碰撞行为")]
    public bool destroyOnHit = false;     // 碰撞后是否销毁子弹

    // 使用Trigger碰撞检测（需在Collider2D上勾选Is Trigger）
    void OnTriggerEnter2D(Collider2D other)
    {
        // 只处理玩家
        if (!other.CompareTag("Player")) return;

        // 获取玩家生命值组件
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        // 造成伤害，传入子弹位置作为击退方向参考
        playerHealth.TakeDamage(damage, transform.position);

        // 根据开关决定是否销毁子弹
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }

    // 如果不使用Trigger，也可以用Collision版本（需非Trigger）
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                
                if (destroyOnHit)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}