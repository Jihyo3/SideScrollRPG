using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 데미지 숫자 팝업. Canvas 없이 3D TextMesh를 쓰므로 프리팹도 필요 없다.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        const float Lifetime = 0.6f;
        const float RiseSpeed = 1.6f;

        float _age;
        TextMesh _text;

        public static void Spawn(Vector3 position, int amount, Color color)
        {
            var go = new GameObject("DamagePopup");
            go.transform.position = position + new Vector3(Random.Range(-0.2f, 0.2f), 0f, 0f);

            var text = go.AddComponent<TextMesh>();
            text.text = amount.ToString();
            text.characterSize = 0.18f;
            text.fontSize = 64;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null && text.font != null) mr.sharedMaterial = text.font.material;

            var popup = go.AddComponent<DamagePopup>();
            popup._text = text;
        }

        void Update()
        {
            // 히트스톱(timeScale 0) 중에도 팝업은 떠야 하므로 unscaled를 쓴다.
            float dt = Time.unscaledDeltaTime;
            _age += dt;
            transform.position += Vector3.up * (RiseSpeed * dt);

            if (_text != null)
            {
                var c = _text.color;
                c.a = Mathf.Clamp01(1f - _age / Lifetime);
                _text.color = c;
            }

            if (_age >= Lifetime) Destroy(gameObject);
        }
    }
}
