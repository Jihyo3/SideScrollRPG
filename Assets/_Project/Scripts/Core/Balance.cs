using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 계획서(GAME_PLAN.md)의 모든 수치를 한 곳에 모아 둔다.
    /// 밸런싱은 원칙적으로 이 파일만 수정한다.
    /// </summary>
    public static class Balance
    {
        // ── 플레이어 이동 ────────────────────────────────
        public const float MoveSpeed = 7.0f;
        public const float Acceleration = 90f;
        public const float AirAcceleration = 35f;
        public const float Gravity = 45f;
        public const float JumpHeight = 3.2f;
        public const float FallGravityMult = 1.8f;
        public const float MaxFallSpeed = 25f;
        public const float CoyoteTime = 0.10f;
        public const float JumpBuffer = 0.12f;
        public const float DashDistance = 3.5f;
        public const float DashDuration = 0.18f;
        public const float DashInvincible = 0.14f;
        public const float DashCooldown = 0.45f;

        public static float JumpVelocity => Mathf.Sqrt(2f * Gravity * JumpHeight);
        public static float DashSpeed => DashDistance / DashDuration;

        // ── 플레이어 전투 ────────────────────────────────
        public const int PlayerMaxHP = 100;
        public const float HurtInvincible = 0.6f;
        public const int ReviveHP = 30;              // 사망 후 마을 복귀 시 HP (여관 이용 유도)

        public static readonly int[] LightDamage = { 10, 12, 16 };
        public const float ComboInputBuffer = 0.15f;
        public const float ComboResetTime = 0.4f;

        // 애니메이션이 없는 단계이므로 공격 타이밍을 코드로 구동한다.
        // 스프라이트 애니메이션을 넣으면 Strike 호출을 Animation Event로 옮긴다.
        public static readonly float[] LightWindup = { 0.07f, 0.07f, 0.10f };
        public static readonly float[] LightRecovery = { 0.13f, 0.13f, 0.22f };

        public const int HeavyDamage = 28;
        public const float HeavyWindup = 0.35f;
        public const float HeavyRecovery = 0.35f;

        public const int PotionHeal = 40;
        public const int PotionMax = 2;

        // ── 타격감 ───────────────────────────────────────
        public const float HitStop = 0.08f;
        public const float FlashDuration = 0.08f;
        public static readonly Vector2 LightKnockback = new Vector2(3f, 1f);
        public static readonly Vector2 HeavyKnockback = new Vector2(7f, 2f);
        public const float ShakeLight = 0.5f;
        public const float ShakeHeavy = 1.5f;
        public const float ShakeDuration = 0.12f;

        // ── 적 ───────────────────────────────────────────
        public const float EnemyGravityScale = 4.5f;   // 플레이어 중력 45와 체감을 맞춘 값

        public const int ChargerHP = 30;
        public const int ChargerAttack = 12;
        public const int ChargerGold = 5;
        public const float ChargerDetectRange = 6f;
        public const float ChargerWindup = 0.5f;
        public const float ChargerDashSpeed = 12f;
        public const float ChargerDashDuration = 0.45f;
        public const float ChargerCooldown = 1.2f;

        public const int ArcherHP = 25;
        public const int ArcherAttack = 10;
        public const int ArcherGold = 7;
        public const float ArcherKeepDistance = 8f;
        public const float ArcherFireInterval = 1.6f;
        public const float ArcherMoveSpeed = 3f;
        public const float ProjectileSpeed = 9f;

        // ── 보스 ─────────────────────────────────────────
        public const int BossHP = 300;
        public const int BossAttack = 22;
        public const float BossPhase2Threshold = 0.5f;  // HP 50% 이하에서 2페이즈
        public const float BossIdleBetweenPatterns = 0.8f;
        public const float BossPhase2SpeedMult = 1.2f;

        // ── 경제 ─────────────────────────────────────────
        public const int SwordPrice = 60;
        public const int ArmorPrice = 50;
        public const int PotionPrice = 20;
        public const int InnPrice = 15;

        public const float SwordAttackBonus = 0.30f;    // 공격력 +30%
        public const float ArmorDamageReduction = 0.20f; // 받는 피해 -20%

        public const int ChestGold = 10;
        public const int RouteAClearBonus = 15;
        public const int RouteBClearBonus = 25;

        // ── 루트 위험 요소 ───────────────────────────────
        public const int FallDamage = 20;
    }
}
