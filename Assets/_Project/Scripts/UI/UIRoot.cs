using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 씬마다 하나 존재하는 UI 컨테이너. 모든 UI 컴포넌트가 이 Canvas 아래에 붙는다.
    /// 컴포넌트 Awake 순서에 의존하지 않도록 필요할 때 지연 생성한다.
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        static UIRoot _instance;
        Canvas _canvas;

        public static Transform Canvas
        {
            get
            {
                var root = Resolve();
                return root == null ? null : root.CanvasTransform;
            }
        }

        Transform CanvasTransform
        {
            get
            {
                if (_canvas == null)
                    _canvas = UIFactory.CreateCanvas("UICanvas", 0, transform);
                return _canvas.transform;
            }
        }

        static UIRoot Resolve()
        {
            if (_instance != null) return _instance;

            _instance = Object.FindAnyObjectByType<UIRoot>();
            if (_instance != null) return _instance;

            var go = new GameObject("[UIRoot]");
            _instance = go.AddComponent<UIRoot>();
            return _instance;
        }

        void Awake()
        {
            if (_instance != null && _instance != this) return;
            _instance = this;
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
