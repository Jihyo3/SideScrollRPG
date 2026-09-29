using System.Collections;
using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>피격 시 흰색 플래시(계획서 §3 타격감).</summary>
    public class HitFlash : MonoBehaviour
    {
        SpriteRenderer _sr;
        Color _base;
        Coroutine _running;

        void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();
            if (_sr != null) _base = _sr.color;
        }

        public void Flash()
        {
            if (_sr == null) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Routine());
        }

        IEnumerator Routine()
        {
            _sr.color = Color.white;
            yield return new WaitForSecondsRealtime(Balance.FlashDuration);
            if (_sr != null) _sr.color = _base;
            _running = null;
        }

        /// 예비동작 표시처럼 색을 직접 바꿔야 할 때 기준색을 갱신한다.
        public void SetBaseColor(Color c)
        {
            _base = c;
            if (_sr != null && _running == null) _sr.color = c;
        }
    }
}
