using UnityEngine;
using UnityEngine.UI;

namespace SideScrollRPG
{
    /// <summary>여관. 골드를 내고 HP를 모두 회복한다. 그것만 한다(계획서 §5).</summary>
    public class InnPanel : MonoBehaviour
    {
        static InnPanel _instance;

        RectTransform _root;
        Text _infoText, _statusText;
        Button _restButton;

        bool _open;
        public static bool IsOpen => _instance != null && _instance._open;

        void Awake()
        {
            _instance = this;
            Build();
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public static void Open()
        {
            if (_instance == null || _instance._open) return;

            _instance._open = true;
            _instance._statusText.text = "";
            _instance._root.gameObject.SetActive(true);
            _instance.RefreshState();
            ModalState.Push();
            Sfx.PlayUI();
        }

        public static void Close()
        {
            if (_instance == null || !_instance._open) return;

            _instance._open = false;
            _instance._root.gameObject.SetActive(false);
            ModalState.Pop();
            Sfx.PlayUI();
        }

        void Update()
        {
            if (!_open) return;
            if (InputReader.CancelDown) Close();
        }

        void Build()
        {
            var canvas = UIRoot.Canvas;
            if (canvas == null) return;

            _root = UIFactory.Group(canvas, "InnPanel", UIFactory.Center, UIFactory.Center,
                Vector2.zero, new Vector2(700f, 360f));

            UIFactory.Panel(_root, "BG", UIFactory.PanelBg, UIFactory.Center, UIFactory.Center,
                Vector2.zero, new Vector2(700f, 360f));

            UIFactory.Label(_root, "Title", "여 관", UIFactory.TopCenter, UIFactory.TopCenter,
                new Vector2(0f, -28f), new Vector2(640f, 48f), 40, TextAnchor.MiddleCenter, UIFactory.Ink);

            _infoText = UIFactory.Label(_root, "Info", "", UIFactory.TopCenter, UIFactory.TopCenter,
                new Vector2(0f, -110f), new Vector2(640f, 80f), 26, TextAnchor.MiddleCenter, UIFactory.Ink);

            _restButton = UIFactory.TextButton(_root, "RestButton", $"묵는다  ({Balance.InnPrice} G)",
                UIFactory.TopCenter, UIFactory.TopCenter, new Vector2(0f, -210f),
                new Vector2(320f, 56f), 26, Rest);

            _statusText = UIFactory.Label(_root, "Status", "", UIFactory.TopCenter, UIFactory.TopCenter,
                new Vector2(0f, -268f), new Vector2(640f, 32f), 22, TextAnchor.MiddleCenter,
                new Color(1f, 0.7f, 0.6f));

            UIFactory.TextButton(_root, "CloseButton", "닫기  [ESC]",
                UIFactory.TopCenter, UIFactory.TopCenter, new Vector2(0f, -310f),
                new Vector2(240f, 44f), 22, Close);

            _root.gameObject.SetActive(false);
        }

        void Rest()
        {
            bool ok = GameManager.I != null && GameManager.I.UseInn();
            _statusText.text = ok
                ? "푹 잤다. 체력이 모두 회복되었다."
                : "골드가 부족하거나 이미 체력이 가득합니다.";
            Sfx.PlayUI();
            RefreshState();
        }

        void RefreshState()
        {
            var gm = GameManager.I;
            if (gm == null) return;

            _infoText.text = $"체력  {gm.CurrentHP} / {gm.MaxHP}\n보유 골드  {gm.Gold} G";
            UIFactory.SetButtonEnabled(_restButton, gm.CanUseInn);
        }
    }
}
