using System;
using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>적·보스 공용 체력. 플레이어는 PlayerHealth를 따로 쓴다.</summary>
    public class Health : MonoBehaviour, IDamageable
    {
        public int maxHP = 30;
        public int goldOnDeath = 0;
        public bool destroyOnDeath = true;
        public float destroyDelay = 0f;

        public int Current { get; private set; }
        public float Normalized => maxHP <= 0 ? 0f : (float)Current / maxHP;
        public bool IsDead { get; private set; }

        public event Action<Health> OnDamaged;
        public event Action<Health> OnDied;

        HitFlash _flash;
        Rigidbody2D _rb;

        void Awake()
        {
            Current = maxHP;
            _flash = GetComponent<HitFlash>();
            _rb = GetComponent<Rigidbody2D>();
        }

        /// Awake 순서에 의존하지 않도록 최대치와 현재치를 함께 설정한다.
        public void Configure(int newMax, int gold)
        {
            maxHP = Mathf.Max(1, newMax);
            goldOnDeath = gold;
            Current = maxHP;
        }

        public void TakeDamage(DamageInfo info)
        {
            if (IsDead) return;

            Current = Mathf.Max(0, Current - info.Amount);

            if (_flash != null) _flash.Flash();
            if (_rb != null && info.Knockback != Vector2.zero) _rb.linearVelocity = info.Knockback;

            DamagePopup.Spawn(transform.position + Vector3.up * 0.8f, info.Amount, new Color(1f, 0.95f, 0.5f));
            HitStop.Do(info.HitStop);
            CameraFollow.Shake(info.Shake, Balance.ShakeDuration);
            Sfx.PlayHit();

            OnDamaged?.Invoke(this);

            if (Current <= 0) Die();
        }

        void Die()
        {
            IsDead = true;

            if (goldOnDeath > 0 && GameManager.I != null) GameManager.I.AddGold(goldOnDeath);
            Sfx.PlayCoin();

            OnDied?.Invoke(this);

            if (destroyOnDeath) Destroy(gameObject, destroyDelay);
        }
    }
}
