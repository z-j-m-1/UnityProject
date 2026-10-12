using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("模式")]
    [Tooltip("勾选 = 2D 模式（使用 Rigidbody2D），取消 = 3D 模式（使用 Rigidbody）")]
    [SerializeField] private bool is2DMode = false;

    [Header("地面检测")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;      // 3D 刚体
    private Rigidbody2D rb2d;  // 2D 刚体

    private Vector2 moveInput;
    [SerializeField] private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        if (is2DMode)
        {
            rb2d = GetComponent<Rigidbody2D>();
            if (rb2d == null)
                Debug.LogError("2D 模式需要 Rigidbody2D 组件！");
        }
        else
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                Debug.LogError("3D 模式需要 Rigidbody 组件！");
        }
    }

    private void Update()
    {
        // 地面检测：2D 用 CheckCircle / 3D 用 CheckSphere
        if (groundCheckPoint == null) return;

        isGrounded = is2DMode
            ? Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer)
            : Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    private void FixedUpdate()
    {
        if (is2DMode) FixedUpdate2D();
        else FixedUpdate3D();
    }

    // ==================== 3D 移动 ====================
    private void FixedUpdate3D()
    {
        if (rb == null) return;

        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.velocity = new Vector3(
            moveDirection.x * moveSpeed,
            rb.velocity.y,
            moveDirection.z * moveSpeed
        );

        if (jumpRequested && isGrounded)
        {
            rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    // ==================== 2D 移动 ====================
    private void FixedUpdate2D()
    {
        if (rb2d == null) return;

        rb2d.velocity = new Vector2(moveInput.x * moveSpeed, rb2d.velocity.y);

        if (jumpRequested && isGrounded)
        {
            rb2d.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            jumpRequested = false;
        }
    }

    // ========== 以下函数由 PlayerInput 通过 SendMessages/UnityEvents 调用 ==========

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpRequested = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}