using System.Collections;
using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 약공격 3타 콤보 + 강공격.
    /// 애니메이션이 아직 없으므로 선딜·판정·후딜을 코루틴으로 구동한다.
    /// 스프라이트 애니메이션을 붙이면 Strike 호출만 Animation Event로 옮기면 된다.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        public Hitbox hitbox;

        public bool IsAttacking { get; private set; }
        /// 3타째와 강공격은 대시로 캔슬할 수 없다(계획서 §3 커밋 규칙).
        public bool CanCancel { get; private set; }

        int _comboIndex;
        float _comboResetTimer;
        bool _queuedLight;
        int _facing = 1;
        Coroutine _routine;

        void Awake()
        {
            if (hitbox != null && hitbox.targetLayers.value == 0)
                hitbox.targetLayers = Layers.MaskEnemy;
        }

        void Update()
        {
            if (IsAttacking) return;

            if (_comboResetTimer > 0f)
            {
                _comboResetTimer -= Time.deltaTime;
                if (_comboResetTimer <= 0f) _comboIndex = 0;
            }
        }

        public void TryLight(int facing)
        {
            _facing = facing;

            if (IsAttacking)
            {
                _queuedLight = true;
                return;
            }

            StartLight(_comboIndex);
        }

        public void TryHeavy(int facing)
        {
            if (IsAttacking) return;
            _facing = facing;
            _comboIndex = 0;
            _routine = StartCoroutine(HeavyRoutine());
        }

        public void Cancel()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            IsAttacking = false;
            CanCancel = false;
            _queuedLight = false;
            _comboIndex = 0;
            _comboResetTimer = 0f;
        }

        void StartLight(int index)
        {
            index = Mathf.Clamp(index, 0, Balance.LightDamage.Length - 1);
            _routine = StartCoroutine(LightRoutine(index));
        }

        IEnumerator LightRoutine(int index)
        {
            IsAttacking = true;
            CanCancel = index < Balance.LightDamage.Length - 1;
            _queuedLight = false;

            yield return new WaitForSeconds(Balance.LightWindup[index]);

            int damage = ScaleDamage(Balance.LightDamage[index]);
            var knockback = new Vector2(Balance.LightKnockback.x * _facing, Balance.LightKnockback.y);
            Strike(new DamageInfo(damage, knockback, Balance.HitStop, Balance.ShakeLight));

            yield return new WaitForSeconds(Balance.LightRecovery[index]);

            IsAttacking = false;
            CanCancel = false;
            _routine = null;

            bool isLast = index >= Balance.LightDamage.Length - 1;
            if (_queuedLight && !isLast)
            {
                _comboIndex = index + 1;
                StartLight(_comboIndex);
            }
            else
            {
                _comboIndex = isLast ? 0 : index + 1;
                _comboResetTimer = Balance.ComboResetTime;
                _queuedLight = false;
            }
        }

        IEnumerator HeavyRoutine()
        {
            IsAttacking = true;
            CanCancel = false;

            yield return new WaitForSeconds(Balance.HeavyWindup);

            int damage = ScaleDamage(Balance.HeavyDamage);
            var knockback = new Vector2(Balance.HeavyKnockback.x * _facing, Balance.HeavyKnockback.y);
            Strike(new DamageInfo(damage, knockback, Balance.HitStop * 1.4f, Balance.ShakeHeavy));

            yield return new WaitForSeconds(Balance.HeavyRecovery);

            IsAttacking = false;
            _routine = null;
            _comboIndex = 0;
        }

        void Strike(DamageInfo info)
        {
            if (hitbox == null) return;
            hitbox.Begin();
            hitbox.Strike(info, _facing);
        }

        static int ScaleDamage(int baseDamage)
        {
            float mult = GameManager.I != null ? GameManager.I.AttackMultiplier : 1f;
            return Mathf.RoundToInt(baseDamage * mult);
        }
    }
}
