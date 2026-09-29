using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 적 수치를 코드 밖으로 빼 둔 데이터 에셋. 프로토타입에서는 이 한 종류만 쓴다.
    /// </summary>
    [CreateAssetMenu(menuName = "SideScrollRPG/Enemy Data", fileName = "EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("공통")]
        public string displayName = "적";
        public int maxHP = 30;
        public int attack = 12;
        public int goldDrop = 5;
        public Color tint = new Color(0.85f, 0.25f, 0.25f);

        [Header("감지 · 이동")]
        public float detectRange = 6f;
        public float moveSpeed = 3f;

        [Header("돌진형")]
        public float windup = 0.5f;
        public float dashSpeed = 12f;
        public float dashDuration = 0.45f;
        public float cooldown = 1.2f;

        [Header("사격형")]
        public float keepDistance = 8f;
        public float fireInterval = 1.6f;
        public float projectileSpeed = 9f;
    }
}
