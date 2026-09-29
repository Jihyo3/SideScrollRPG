using UnityEngine;

namespace SideScrollRPG
{
    public enum PlayerState { Idle, Move, Jump, Fall, Dash, Attack, Hurt, Dead }

    /// <summary>
    /// 이동·점프·대시. 중력을 직접 계산하고(gravityScale = 0) 속도를 매 물리 프레임에 덮어쓴다.
    /// 접지 판정은 OverlapCircle을 쓴다(레이 1발은 경사·모서리에서 실패한다).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public Transform groundCheck;
        public LayerMask groundMask;
        public float groundCheckRadius = 0.16f;

        public PlayerState State { get; private set; } = PlayerState.Idle;
        public int Facing { get; private set; } = 1;
        public bool IsGrounded { get; private set; }
        public bool DashInvincible => _dashInvulnTimer > 0f;

        Rigidbody2D _rb;
        PlayerCombat _combat;
        PlayerHealth _health;

        float _vx, _vy;
        float _coyoteTimer, _jumpBufferTimer;
        float _dashTimer, _dashCooldownTimer, _dashInvulnTimer;
        int _dashDir = 1;
        bool _dashQueued;
        bool _dead;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            _combat = GetComponent<PlayerCombat>();
            _health = GetComponent<PlayerHealth>();

            if (groundMask.value == 0) groundMask = Layers.MaskGround;
        }

        void OnEnable()
        {
            if (_health != null) _health.OnDied += HandleDeath;
        }

        void OnDisable()
        {
            if (_health != null) _health.OnDied -= HandleDeath;
        }

        void HandleDeath()
        {
            _dead = true;
            State = PlayerState.Dead;
            _vx = 0f;
        }

        void Update()
        {
            if (_dead) return;

            if (InputReader.JumpDown) _jumpBufferTimer = Balance.JumpBuffer;

            if (InputReader.DashDown) _dashQueued = true;

            if (InputReader.LightAttackDown && IsGrounded) _combat.TryLight(Facing);
            if (InputReader.HeavyAttackDown && IsGrounded) _combat.TryHeavy(Facing);
            if (InputReader.PotionDown) _health.TryUsePotion();

            // 공격·대시 중에는 방향을 고정한다.
            float moveX = InputReader.MoveX;
            if (Mathf.Abs(moveX) > 0.01f && !_combat.IsAttacking && State != PlayerState.Dash)
                Facing = moveX > 0f ? 1 : -1;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            Tick(ref _jumpBufferTimer, dt);
            Tick(ref _dashCooldownTimer, dt);
            Tick(ref _dashInvulnTimer, dt);

            IsGrounded = groundCheck != null &&
                         Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

            _coyoteTimer = IsGrounded ? Balance.CoyoteTime : Mathf.Max(0f, _coyoteTimer - dt);

            if (_dead)
            {
                _vx = Mathf.MoveTowards(_vx, 0f, Balance.Acceleration * dt);
                ApplyGravity(dt);
                Commit();
                return;
            }

            if (State == PlayerState.Dash)
            {
                TickDash(dt);
                Commit();
                return;
            }

            TryStartDash();
            if (State == PlayerState.Dash) { Commit(); return; }

            HorizontalMove(dt);
            TryJump();
            ApplyGravity(dt);
            UpdateState();
            Commit();
        }

        static void Tick(ref float timer, float dt)
        {
            if (timer > 0f) timer = Mathf.Max(0f, timer - dt);
        }

        void HorizontalMove(float dt)
        {
            // 공격 모션 중에는 이동할 수 없다(계획서 §3 커밋 규칙).
            float target = _combat.IsAttacking ? 0f : InputReader.MoveX * Balance.MoveSpeed;
            float accel = IsGrounded ? Balance.Acceleration : Balance.AirAcceleration;
            if (_combat.IsAttacking) accel = Balance.Acceleration * 2f;

            _vx = Mathf.MoveTowards(_vx, target, accel * dt);
        }

        void TryJump()
        {
            if (_combat.IsAttacking) return;
            if (_jumpBufferTimer <= 0f || _coyoteTimer <= 0f) return;

            _vy = Balance.JumpVelocity;
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            Sfx.PlayJump();
        }

        void ApplyGravity(float dt)
        {
            bool fastFall = _vy < 0f || !InputReader.JumpHeld;
            float g = Balance.Gravity * (fastFall ? Balance.FallGravityMult : 1f);

            _vy -= g * dt;
            _vy = Mathf.Max(_vy, -Balance.MaxFallSpeed);

            // 접지 중 하강값을 살짝 남겨 두면 경사·이음새에서 접지 판정이 덜 흔들린다.
            if (IsGrounded && _vy < 0f) _vy = -1f;
        }

        void TryStartDash()
        {
            if (!_dashQueued) return;
            _dashQueued = false;

            if (_dashCooldownTimer > 0f) return;
            if (_combat.IsAttacking && !_combat.CanCancel) return;

            if (_combat.IsAttacking) _combat.Cancel();

            float moveX = InputReader.MoveX;
            _dashDir = Mathf.Abs(moveX) > 0.01f ? (moveX > 0f ? 1 : -1) : Facing;
            Facing = _dashDir;

            State = PlayerState.Dash;
            _dashTimer = Balance.DashDuration;
            _dashInvulnTimer = Balance.DashInvincible;
            _dashCooldownTimer = Balance.DashCooldown + Balance.DashDuration;
        }

        void TickDash(float dt)
        {
            _dashTimer -= dt;
            _vx = _dashDir * Balance.DashSpeed;
            _vy = 0f;

            if (_dashTimer <= 0f)
                State = IsGrounded ? PlayerState.Idle : PlayerState.Fall;
        }

        void UpdateState()
        {
            if (_combat.IsAttacking) { State = PlayerState.Attack; return; }
            if (!IsGrounded) { State = _vy > 0f ? PlayerState.Jump : PlayerState.Fall; return; }
            State = Mathf.Abs(_vx) > 0.1f ? PlayerState.Move : PlayerState.Idle;
        }

        void Commit()
        {
            _rb.linearVelocity = new Vector2(_vx, _vy);
        }

        void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
