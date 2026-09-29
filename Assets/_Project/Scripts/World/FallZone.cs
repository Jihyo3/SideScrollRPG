using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 낙하 구간. 즉사 대신 피해를 주고 직전 지점으로 되돌린다.
    /// 즉사 처리는 부활 위치 버그로 시간을 잡아먹기 쉬워서 의도적으로 피했다(계획서 §6).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FallZone : MonoBehaviour
    {
        public Transform respawnPoint;
        public int damage = Balance.FallDamage;

        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            var health = player.GetComponent<PlayerHealth>();
            if (health != null) health.TakeEnvironmentDamage(damage);

            if (respawnPoint != null)
            {
                player.transform.position = respawnPoint.position;

                var body = player.GetComponent<Rigidbody2D>();
                if (body != null) body.linearVelocity = Vector2.zero;
            }

            CameraFollow.Shake(Balance.ShakeLight, 0.2f);
        }
    }
}
