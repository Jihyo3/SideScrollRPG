using UnityEngine;
using UnityEngine.UI;

namespace SideScrollRPG
{
    /// <summary>
    /// 상점. 정적 버튼 3개로 고정한다 — 스크롤·아이콘 그리드·툴팁은 만들지 않는다(계획서 §5).
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        static ShopPanel _instance;

        RectTransform _root;
        Text _goldText, _statusText;
        Button _swordButton, _armorButton, _potionButton;

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
            _instance.RefreshButtons();
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

            _root = UIFactory.Group(canvas, "ShopPanel", UIFactory.Center, UIFactory.Center,
                Vector2.zero, new Vector2(860f, 560f));

            UIFactory.Panel(_root, "BG", UIFactory.PanelBg, UIFactory.Center, UIFactory.Center,
                Vector2.zero, new Vector2(860f, 560f));

            UIFactory.Label(_root, "Title", "상 점", UIFactory.TopCenter, UIFactory.TopCenter,
                new Vector2(0f, -28f), new Vector2(800f, 48f), 40, TextAnchor.MiddleCenter, UIFactory.Ink);

            _goldText = UIFactory.Label(_root, "Gold", "", UIFactory.TopCenter, UIFactory.TopCenter,
                new Vector2(0f, -80f), new Vector2(800f, 34f), 26, TextAnchor.MiddleCenter, UIFactory.Gold);

            _swordButton = BuildRow(-150f,
                $"날카로운 검   공격력 +{Mathf.RoundToInt(Balance.SwordAttackBonus * 100f)}%",
                Balance.SwordPrice, BuySword);

            _armorButton = BuildRow(-250f,
                $"가죽 갑옷   받는 피해 -{Mathf.RoundToInt(Balance.ArmorDamageReduction * 100f)}%",
                Balance.ArmorPrice, BuyArmor);

            _potionButton = BuildRow(-350f,
                $"회복 물약   HP +{Balance.PotionHeal} (최대 {Balance.PotionMax}개 소지)",
                Balance.PotionPrice, BuyPotion);

            _statusText = UIFactory.Label(_root, "Status", "", UIFactory.TopCenter, UIFactory.TopCenter,
                new Vector2(0f, -452f), new Vector2(800f, 34f), 22, TextAnchor.MiddleCenter,
                new Color(1f, 0.7f, 0.6f));

            UIFactory.TextButton(_root, "CloseButton", "닫기  [ESC]",
                UIFactory.TopCenter, UIFactory.TopCenter, new Vector2(0f, -496f),
                new Vector2(260f, 48f), 24, Close);

            _root.gameObject.SetActive(false);
        }

        Button BuildRow(float y, string description, int price, UnityEngine.Events.UnityAction action)
        {
            UIFactory.Label(_root, "Desc", description, UIFactory.TopCenter, UIFactory.TopLeft,
                new Vector2(-390f, y), new Vector2(520f, 40f), 24, TextAnchor.MiddleLeft, UIFactory.Ink);

            UIFactory.Label(_root, "Price", $"{price} G", UIFactory.TopCenter, UIFactory.TopLeft,
                new Vector2(140f, y), new Vector2(120f, 40f), 24, TextAnchor.MiddleLeft, UIFactory.Gold);

            return UIFactory.TextButton(_root, "Buy", "구매",
                UIFactory.TopCenter, UIFactory.TopLeft, new Vector2(255f, y),
                new Vector2(140f, 48f), 24, action);
        }

        void BuySword() => Apply(GameManager.I != null && GameManager.I.BuySword(), "검");
        void BuyArmor() => Apply(GameManager.I != null && GameManager.I.BuyArmor(), "갑옷");
        void BuyPotion() => Apply(GameManager.I != null && GameManager.I.BuyPotion(), "물약");

        void Apply(bool success, string label)
        {
            _statusText.text = success ? $"{label}을 구매했습니다." : "골드가 부족하거나 이미 가지고 있습니다.";
            Sfx.PlayUI();
            RefreshButtons();
        }

        void RefreshButtons()
        {
            var gm = GameManager.I;
            if (gm == null) return;

            _goldText.text = $"보유 골드  {gm.Gold} G";

            UIFactory.SetButtonEnabled(_swordButton, gm.CanBuySword);
            UIFactory.SetButtonEnabled(_armorButton, gm.CanBuyArmor);
            UIFactory.SetButtonEnabled(_potionButton, gm.CanBuyPotion);

            SetLabel(_swordButton, gm.HasSword ? "보유 중" : "구매");
            SetLabel(_armorButton, gm.HasArmor ? "보유 중" : "구매");
            SetLabel(_potionButton, gm.Potions >= Balance.PotionMax ? "가득" : "구매");
        }

        static void SetLabel(Button button, string label)
        {
            if (button == null) return;
            var text = button.GetComponentInChildren<Text>();
            if (text != null) text.text = label;
        }
    }
}
