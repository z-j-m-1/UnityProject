using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("地面检测")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private Vector2 moveInput;
    [SerializeField] private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("需要 Rigidbody 组件！");
        }
    }

    private void Update()
    {
        // 检测玩家是否在地面上
        isGrounded = Physics.CheckSphere(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void FixedUpdate()
    {
        // 3D 移动：把 2D 输入映射到世界空间的 XZ 平面
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.velocity = new Vector3(
            moveDirection.x * moveSpeed,
            rb.velocity.y,
            moveDirection.z * moveSpeed
        );

        // 跳跃处理
        if (jumpRequested && isGrounded)
        {
            rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    // ========== 以下函数由 PlayerInput 通过 SendMessages/UnityEvents 调用 ==========

    /// <summary>
    /// 移动函数 - 由 PlayerInput 的 Move 动作调用
    /// </summary>
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    /// <summary>
    /// 跳跃函数 - 由 PlayerInput 的 Jump 动作调用
    /// </summary>
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpRequested = true;
        }
    }

    // 编辑器中绘制地面检测范围
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}