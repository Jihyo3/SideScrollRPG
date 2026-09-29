using System.Collections;
using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 타격 시 때린 쪽·맞은 쪽을 함께 정지시키는 히트스톱(계획서 §3 타격감).
    /// 모달이 열려 있는 동안에는 timeScale을 건드리지 않는다.
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        static HitStop _instance;
        Coroutine _running;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("[HitStop]");
            _instance = go.AddComponent<HitStop>();
            DontDestroyOnLoad(go);
        }

        public static void Do(float duration)
        {
            if (_instance == null || duration <= 0f) return;
            _instance.Run(duration);
        }

        void Run(float duration)
        {
            if (ModalState.IsOpen) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Freeze(duration));
        }

        IEnumerator Freeze(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            if (!ModalState.IsOpen) Time.timeScale = 1f;
            _running = null;
        }
    }
}
