using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>적 공통 처리: 데이터 적용, 플레이어 탐색, 바라보는 방향, 좌우 이동.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public abstract class EnemyBase : MonoBehaviour
    {
        public EnemyData data;
        public Hitbox hitbox;

        protected Rigidbody2D Body;
        protected Health Hp;
        protected HitFlash Flash;
        protected Transform Player;
        protected PlayerHealth PlayerHp;
        protected int Facing = -1;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Body.freezeRotation = true;
            Body.gravityScale = Balance.EnemyGravityScale;

            Hp = GetComponent<Health>();
            Flash = GetComponent<HitFlash>();

            if (data != null)
            {
                Hp.Configure(data.maxHP, data.goldDrop);
                if (Flash != null) Flash.SetBaseColor(data.tint);
            }

            if (hitbox != null && hitbox.targetLayers.value == 0)
                hitbox.targetLayers = Layers.MaskPlayer;
        }

        protected virtual void Start() => AcquirePlayer();

        protected void AcquirePlayer()
        {
            PlayerHp = Object.FindAnyObjectByType<PlayerHealth>();
            Player = PlayerHp != null ? PlayerHp.transform : null;
        }

        protected bool HasLivingPlayer()
        {
            if (Player == null || PlayerHp == null || PlayerHp.IsDead)
            {
                AcquirePlayer();
                return Player != null && PlayerHp != null && !PlayerHp.IsDead;
            }
            return true;
        }

        protected float DistanceToPlayer()
            => Player == null ? float.MaxValue : Mathf.Abs(Player.position.x - transform.position.x);

        protected int DirectionToPlayer()
        {
            if (Player == null) return Facing;
            float dx = Player.position.x - transform.position.x;
            return Mathf.Abs(dx) < 0.05f ? Facing : (dx > 0f ? 1 : -1);
        }

        protected void FaceTowardPlayer() => Facing = DirectionToPlayer();

        protected void SetHorizontalSpeed(float speed)
            => Body.linearVelocity = new Vector2(speed, Body.linearVelocity.y);

        protected void Brake(float dt)
            => SetHorizontalSpeed(Mathf.MoveTowards(Body.linearVelocity.x, 0f, 40f * dt));

        protected DamageInfo BuildAttack(int amount, float shake)
        {
            var knockback = new Vector2(Balance.LightKnockback.x * Facing * 1.2f, Balance.LightKnockback.y);
            return new DamageInfo(amount, knockback, Balance.HitStop, shake);
        }

        protected int Attack => data != null ? data.attack : Balance.ChargerAttack;
    }
}
