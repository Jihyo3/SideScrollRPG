using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 레이어 이름 상수. 충돌 매트릭스는 기본값(전부 허용)으로 두고,
    /// 피해 판정은 전부 명시적인 LayerMask로만 처리한다(계획서 §7).
    /// </summary>
    public static class Layers
    {
        public const string Player = "Player";
        public const string PlayerHitbox = "PlayerHitbox";
        public const string Enemy = "Enemy";
        public const string EnemyHitbox = "EnemyHitbox";
        public const string Ground = "Ground";
        public const string Projectile = "Projectile";
        public const string Interactable = "Interactable";

        public static readonly string[] All =
        {
            Player, PlayerHitbox, Enemy, EnemyHitbox, Ground, Projectile, Interactable
        };

        public static int MaskPlayer => LayerMask.GetMask(Player);
        public static int MaskEnemy => LayerMask.GetMask(Enemy);
        public static int MaskGround => LayerMask.GetMask(Ground);
    }
}
