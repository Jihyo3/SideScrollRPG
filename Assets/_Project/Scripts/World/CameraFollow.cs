using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// Cinemachine 없이 쓰는 최소 추적 카메라. 데드존 + 룩어헤드 + 화면 흔들림만 담당한다.
    /// 패키지 의존을 없애려고 직접 만들었다(계획서 §7).
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        static CameraFollow _instance;

        public Transform target;
        public Vector2 deadZone = new Vector2(1.2f, 0.8f);
        public float smoothTime = 0.14f;
        public Vector2 offset = new Vector2(0f, 1.0f);
        public float lookAhead = 1.2f;

        public bool clampToBounds = false;
        public Vector2 minBounds;
        public Vector2 maxBounds;

        Vector3 _velocity;
        float _shakeAmplitude, _shakeTimer, _shakeDuration;

        void Awake() => _instance = this;

        void OnDestroy() { if (_instance == this) _instance = null; }

        void LateUpdate()
        {
            if (target == null)
            {
                var player = Object.FindAnyObjectByType<PlayerController>();
                if (player != null) target = player.transform;
                if (target == null) return;
            }

            Vector3 focus = target.position + (Vector3)offset;

            var body = target.GetComponent<Rigidbody2D>();
            if (body != null) focus.x += Mathf.Clamp(body.linearVelocity.x / Balance.MoveSpeed, -1f, 1f) * lookAhead;

            Vector3 current = transform.position;
            Vector3 desired = current;

            if (Mathf.Abs(focus.x - current.x) > deadZone.x)
                desired.x = focus.x - Mathf.Sign(focus.x - current.x) * deadZone.x;

            if (Mathf.Abs(focus.y - current.y) > deadZone.y)
                desired.y = focus.y - Mathf.Sign(focus.y - current.y) * deadZone.y;

            desired.z = current.z;

            Vector3 next = Vector3.SmoothDamp(current, desired, ref _velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

            if (clampToBounds)
            {
                next.x = Mathf.Clamp(next.x, minBounds.x, maxBounds.x);
                next.y = Mathf.Clamp(next.y, minBounds.y, maxBounds.y);
            }

            transform.position = next + ShakeOffset();
        }

        Vector3 ShakeOffset()
        {
            if (_shakeTimer <= 0f) return Vector3.zero;

            _shakeTimer -= Time.unscaledDeltaTime;
            float falloff = _shakeDuration <= 0f ? 0f : Mathf.Clamp01(_shakeTimer / _shakeDuration);
            float a = _shakeAmplitude * falloff;

            return new Vector3(Random.Range(-a, a), Random.Range(-a, a), 0f);
        }

        public static void Shake(float amplitude, float duration)
        {
            if (_instance == null || amplitude <= 0f) return;

            // 더 강한 흔들림이 약한 흔들림을 덮어쓰게 한다.
            if (amplitude < _instance._shakeAmplitude && _instance._shakeTimer > 0f) return;

            _instance._shakeAmplitude = amplitude * 0.25f;
            _instance._shakeDuration = duration;
            _instance._shakeTimer = duration;
        }
    }
}
