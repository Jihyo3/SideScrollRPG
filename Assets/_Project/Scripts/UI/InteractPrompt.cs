using UnityEngine;
using UnityEngine.UI;

namespace SideScrollRPG
{
    /// <summary>"상점 (E)" 같은 상호작용 안내 문구.</summary>
    public class InteractPrompt : MonoBehaviour
    {
        static InteractPrompt _instance;

        Text _text;
        string _current;

        public static void Show(string message)
        {
            var prompt = Resolve();
            if (prompt == null) return;

            prompt._current = message;
            prompt.Apply();
        }

        /// 해당 문구를 표시 중일 때만 숨긴다(존이 겹칠 때 서로 지우는 것을 막는다).
        public static void Hide(string message)
        {
            if (_instance == null) return;
            if (_instance._current != message) return;

            _instance._current = null;
            _instance.Apply();
        }

        static InteractPrompt Resolve()
        {
            if (_instance != null) return _instance;

            _instance = Object.FindAnyObjectByType<InteractPrompt>();
            if (_instance == null)
            {
                var go = new GameObject("[InteractPrompt]");
                _instance = go.AddComponent<InteractPrompt>();
            }

            _instance.EnsureBuilt();
            return _instance;
        }

        void Awake()
        {
            _instance = this;
            EnsureBuilt();
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        void EnsureBuilt()
        {
            if (_text != null) return;

            var canvas = UIRoot.Canvas;
            if (canvas == null) return;

            _text = UIFactory.Label(canvas, "InteractPrompt", "",
                UIFactory.BottomCenter, UIFactory.BottomCenter,
                new Vector2(0f, 170f), new Vector2(900f, 44f),
                28, TextAnchor.MiddleCenter, UIFactory.Ink);

            _text.gameObject.SetActive(false);
        }

        void Apply()
        {
            EnsureBuilt();
            if (_text == null) return;

            bool visible = !string.IsNullOrEmpty(_current);
            _text.gameObject.SetActive(visible);
            if (visible) _text.text = _current;
        }
    }
}
