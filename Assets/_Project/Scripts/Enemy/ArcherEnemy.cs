using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 사격형. 일정 거리를 유지하며 투사체를 쏴서 플레이어에게 접근을 강제한다.
    /// 돌진형과 같이 배치될 때 비로소 위협이 된다(계획서 §4).
    /// </summary>
    public class ArcherEnemy : EnemyBase
    {
        public Projectile projectilePrefab;
        public Transform muzzle;

        float _fireTimer;
        const float DistanceTolerance = 2f;
        const float MaxFireRange = 16f;

        protected override void Awake()
        {
            base.Awake();
            _fireTimer = Random.Range(0.2f, 0.8f);   // 여러 마리가 동시에 쏘지 않게 흩뿌린다
        }

        void Update()
        {
            if (Hp.IsDead) return;

            float dt = Time.deltaTime;

            if (!HasLivingPlayer())
            {
                Brake(dt);
                return;
            }

            FaceTowardPlayer();
            KeepDistance(dt);

            _fireTimer -= dt;
            if (_fireTimer > 0f) return;

            if (DistanceToPlayer() <= MaxFireRange) Fire();
            _fireTimer = data != null ? data.fireInterval : Balance.ArcherFireInterval;
        }

        void KeepDistance(float dt)
        {
            float want = data != null ? data.keepDistance : Balance.ArcherKeepDistance;
            float speed = data != null ? data.moveSpeed : Balance.ArcherMoveSpeed;
            float dist = DistanceToPlayer();

            if (dist < want - DistanceTolerance) SetHorizontalSpeed(-Facing * speed);
            else if (dist > want + DistanceTolerance) SetHorizontalSpeed(Facing * speed);
            else Brake(dt);
        }

        void Fire()
        {
            if (projectilePrefab == null || Player == null) return;

            Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 0.6f;
            var projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);

            Vector2 dir = ((Vector2)(Player.position + Vector3.up * 0.6f) - (Vector2)origin).normalized;
            float speed = data != null ? data.projectileSpeed : Balance.ProjectileSpeed;

            projectile.Launch(dir, Attack, speed);
        }
    }
}
