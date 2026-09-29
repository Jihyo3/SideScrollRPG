using System.Collections.Generic;
using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 상시 활성 트리거 콜라이더를 쓰지 않는다. 공격 판정 프레임에 OverlapBox를 1회 호출해서
    /// 중복 타격과 유령 판정을 원천적으로 없앤다(계획서 §7).
    /// </summary>
    public class Hitbox : MonoBehaviour
    {
        [Tooltip("캐릭터 기준 오프셋. x는 바라보는 방향으로 자동 반전된다.")]
        public Vector2 localOffset = new Vector2(0.9f, 0f);
        public Vector2 size = new Vector2(1.6f, 1.4f);
        public LayerMask targetLayers;

        readonly List<IDamageable> _alreadyHit = new List<IDamageable>();
        readonly Collider2D[] _buffer = new Collider2D[16];

        Vector2 _lastCenter;
        float _debugUntil;

        /// 한 번의 공격(또는 콤보 1타)이 시작될 때 호출해 중복 타격 목록을 비운다.
        public void Begin() => _alreadyHit.Clear();

        /// 실제 판정. 애니메이션을 붙이면 이 메서드를 Animation Event에서 호출하면 된다.
        public int Strike(DamageInfo info, int facing)
        {
            Vector2 center = (Vector2)transform.position
                             + new Vector2(localOffset.x * facing, localOffset.y);
            _lastCenter = center;
            _debugUntil = Time.time + 0.1f;

            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(targetLayers);

            int count = Physics2D.OverlapBox(center, size, 0f, filter, _buffer);
            int hitCount = 0;

            for (int i = 0; i < count; i++)
            {
                var target = _buffer[i].GetComponentInParent<IDamageable>();
                if (target == null || _alreadyHit.Contains(target)) continue;

                _alreadyHit.Add(target);
                target.TakeDamage(info);
                hitCount++;
            }

            return hitCount;
        }

        void OnDrawGizmos()
        {
            if (Time.time > _debugUntil) return;
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(_lastCenter, size);
        }
    }
}
