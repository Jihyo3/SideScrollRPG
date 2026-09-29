using System;
using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>플레이어 체력. 실제 값은 GameManager가 들고 있고 여기서 동기화한다.</summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public int Current { get; private set; } = Balance.PlayerMaxHP;
        public int Max => Balance.PlayerMaxHP;
        public bool IsDead { get; private set; }

        public event Action OnChanged;
        public event Action OnDied;

        float _invulnTimer;
        PlayerController _controller;
        HitFlash _flash;
        Rigidbody2D _rb;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _flash = GetComponent<HitFlash>();
            _rb = GetComponent<Rigidbody2D>();
            PullFromGameManager();
        }

        /// GameManager의 값을 화면에 반영한다(씬 진입·여관 이용 직후).
        public void PullFromGameManager()
        {
            if (GameManager.I != null) Current = Mathf.Clamp(GameManager.I.CurrentHP, 0, Max);
            if (Current <= 0) Current = Balance.ReviveHP;
            IsDead = false;
            Push();
        }

        void Update()
        {
            if (_invulnTimer > 0f) _invulnTimer -= Time.deltaTime;
        }

        public void TakeDamage(DamageInfo info)
        {
            if (IsDead) return;
            if (_invulnTimer > 0f) return;
            if (_controller != null && _controller.DashInvincible) return;

            float mult = GameManager.I != null ? GameManager.I.DamageTakenMultiplier : 1f;
            int amount = Mathf.Max(1, Mathf.RoundToInt(info.Amount * mult));

            Current = Mathf.Max(0, Current - amount);
            _invulnTimer = Balance.HurtInvincible;

            if (_flash != null) _flash.Flash();
            if (_rb != null && info.Knockback != Vector2.zero) _rb.linearVelocity = info.Knockback;

            DamagePopup.Spawn(transform.position + Vector3.up * 1.1f, amount, new Color(1f, 0.4f, 0.4f));
            HitStop.Do(info.HitStop);
            CameraFollow.Shake(Mathf.Max(info.Shake, Balance.ShakeLight), Balance.ShakeDuration);
            Sfx.PlayHurt();

            Push();

            if (Current <= 0) Die();
        }

        /// 낙하 구간처럼 연출 없이 피해만 주는 경우.
        public void TakeEnvironmentDamage(int amount)
        {
            if (IsDead) return;

            Current = Mathf.Max(0, Current - amount);
            if (_flash != null) _flash.Flash();
            DamagePopup.Spawn(transform.position + Vector3.up * 1.1f, amount, new Color(1f, 0.4f, 0.4f));
            Sfx.PlayHurt();
            Push();

            if (Current <= 0) Die();
        }

        public bool TryUsePotion()
        {
            if (IsDead) return false;
            if (Current >= Max) return false;
            if (GameManager.I == null || !GameManager.I.ConsumePotion()) return false;

            Current = Mathf.Min(Max, Current + Balance.PotionHeal);
            DamagePopup.Spawn(transform.position + Vector3.up * 1.1f, Balance.PotionHeal, new Color(0.5f, 1f, 0.6f));
            Sfx.PlayUI();
            Push();
            return true;
        }

        void Die()
        {
            IsDead = true;
            Push();
            OnDied?.Invoke();
        }

        void Push()
        {
            if (GameManager.I != null) GameManager.I.SyncHP(Current);
            OnChanged?.Invoke();
        }
    }
}
