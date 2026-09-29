using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 좌→우로 채워지는 게이지. 스프라이트 없는 Image는 fillAmount가 동작하지 않으므로
    /// 채움 막대의 너비를 직접 조절한다.
    /// </summary>
    public class GaugeView : MonoBehaviour
    {
        public RectTransform fill;
        public float fullWidth = 100f;

        float _shown = 1f;

        public void SetRatio(float ratio, bool instant = false)
        {
            ratio = Mathf.Clamp01(ratio);
            _shown = instant ? ratio : Mathf.MoveTowards(_shown, ratio, Time.unscaledDeltaTime * 2.5f);
            if (instant) _shown = ratio;

            if (fill != null)
                fill.sizeDelta = new Vector2(fullWidth * _shown, fill.sizeDelta.y);
        }
    }
}
