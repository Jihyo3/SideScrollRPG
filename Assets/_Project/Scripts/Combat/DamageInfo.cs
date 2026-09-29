using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 한 번의 타격이 전달하는 정보. 넉백은 이미 방향이 적용된 속도값으로 들어온다.
    /// </summary>
    public readonly struct DamageInfo
    {
        public readonly int Amount;
        public readonly Vector2 Knockback;
        public readonly float HitStop;
        public readonly float Shake;

        public DamageInfo(int amount, Vector2 knockback, float hitStop, float shake)
        {
            Amount = amount;
            Knockback = knockback;
            HitStop = hitStop;
            Shake = shake;
        }

        /// 연출 없이 피해만 주는 경우(낙하 피해 등).
        public static DamageInfo Plain(int amount) => new DamageInfo(amount, Vector2.zero, 0f, 0f);
    }

    public interface IDamageable
    {
        void TakeDamage(DamageInfo info);
    }
}
