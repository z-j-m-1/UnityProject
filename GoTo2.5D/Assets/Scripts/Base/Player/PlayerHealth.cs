using UnityEngine;
using UnityEngine.Events; // 引入事件系统，便于UI更新

// ==================== 玩家生命值脚本 ====================
// 挂载到玩家对象上，管理血量、受伤、死亡和击退响应
public class PlayerHealth : MonoBehaviour
{
    [Header("生命值设置")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("受伤响应")]
    public float invincibleDuration = 0.5f; // 无敌时间（秒）
    private float invincibleTimer;
    public bool isInvincible => invincibleTimer > 0;

    [Header("击退设置")]
    public float knockbackForce = 5f;        // 击退力度
    public float knockbackDuration = 0.2f;   // 击退持续时间

    // 事件：当生命值变化时触发（用于UI血条更新）
    public UnityEvent<int, int> onHealthChanged; // 参数：当前血量，最大血量
    public UnityEvent onDeath;                  // 死亡事件

    private Rigidbody2D rb;
    private MonoBehaviour[] scriptsToDisableOnDeath; // 死亡时禁用的脚本列表

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        // 自动收集需要禁用的脚本（例如移动、射击等）
        scriptsToDisableOnDeath = GetComponents<MonoBehaviour>();
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        // 无敌倒计时
        if (invincibleTimer > 0)
            invincibleTimer -= Time.deltaTime;
    }

    // ---------- 公共方法 ----------
    // 扣血方法：外部调用，传入伤害值和攻击者位置（用于击退方向）
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        // 无敌状态或已死亡则忽略伤害
        if (isInvincible || currentHealth <= 0) return;

        // 实际扣血
        currentHealth = Mathf.Max(0, currentHealth - damage);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        // 触发击退效果
        ApplyKnockback(attackerPosition);

        // 进入无敌状态
        invincibleTimer = invincibleDuration;

        // 检查死亡
        if (currentHealth <= 0)
            Die();
    }

    // 治疗或增加血量（可选）
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ---------- 私有方法 ----------
    private void ApplyKnockback(Vector2 attackerPos)
    {
        if (rb == null) return;

        // 计算击退方向（从攻击者指向玩家）
        Vector2 dir = ((Vector2)transform.position - attackerPos).normalized;
        // 添加力，并覆盖原有速度（避免累积）
        rb.velocity = Vector2.zero;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        // 可选：使用协程在击退结束后恢复（如需更精确控制）
        // 但这里简单处理，物理引擎会自然衰减
    }

    private void Die()
    {
        // 禁用玩家控制脚本（防止移动/射击等）
        foreach (var script in scriptsToDisableOnDeath)
        {
            // 不禁用自身（PlayerHealth）和Transform
            if (script != this && script.GetType() != typeof(Transform))
                script.enabled = false;
        }

        // 触发死亡事件
        onDeath?.Invoke();

        // 可选：播放死亡动画、销毁等
        Debug.Log("玩家死亡");
    }

    // 重置生命值（用于重生）
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        invincibleTimer = 0;
        // 重新启用脚本（如果之前禁用）
        foreach (var script in scriptsToDisableOnDeath)
        {
            if (script != this && script.GetType() != typeof(Transform))
                script.enabled = true;
        }
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

