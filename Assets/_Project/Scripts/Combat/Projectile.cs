using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>사격형 적의 투사체. 대상/차단 레이어를 마스크로만 판정한다.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        public LayerMask hitLayers;
        public LayerMask blockLayers;
        public float lifetime = 4f;

        int _damage;
        Rigidbody2D _rb;

        void Awake() => _rb = GetComponent<Rigidbody2D>();

        public void Launch(Vector2 direction, int damage, float speed)
        {
            _damage = damage;
            _rb.linearVelocity = direction.normalized * speed;
            Destroy(gameObject, lifetime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            int otherBit = 1 << other.gameObject.layer;

            if ((blockLayers.value & otherBit) != 0)
            {
                Destroy(gameObject);
                return;
            }

            if ((hitLayers.value & otherBit) == 0) return;

            var target = other.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                float dir = Mathf.Sign(_rb.linearVelocity.x == 0f ? 1f : _rb.linearVelocity.x);
                var info = new DamageInfo(
                    _damage,
                    new Vector2(Balance.LightKnockback.x * dir * 0.6f, Balance.LightKnockback.y),
                    Balance.HitStop * 0.5f,
                    Balance.ShakeLight);
                target.TakeDamage(info);
            }

            Destroy(gameObject);
        }
    }
}
