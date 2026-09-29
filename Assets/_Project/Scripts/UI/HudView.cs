using UnityEngine;
using UnityEngine.UI;

namespace SideScrollRPG
{
    /// <summary>HP · 골드 · 물약 · (보스전에서는) 보스 체력바.</summary>
    public class HudView : MonoBehaviour
    {
        GaugeView _hpGauge;
        Text _hpText, _goldText, _potionText, _gearText;

        RectTransform _bossGroup;
        GaugeView _bossGauge;

        PlayerHealth _player;
        BossController _boss;

        void Start()
        {
            Build();
            Refresh(true);
        }

        void Build()
        {
            var canvas = UIRoot.Canvas;
            if (canvas == null) return;

            var root = UIFactory.Group(canvas, "HUD", UIFactory.TopLeft, UIFactory.TopLeft,
                new Vector2(36f, -30f), new Vector2(560f, 140f));

            _hpGauge = UIFactory.Gauge(root, "HpGauge", UIFactory.HpRed, new Color(0.1f, 0.1f, 0.12f, 0.9f),
                UIFactory.TopLeft, UIFactory.TopLeft, Vector2.zero, new Vector2(420f, 34f));

            _hpText = UIFactory.Label(root, "HpText", "", UIFactory.TopLeft, UIFactory.TopLeft,
                new Vector2(12f, -4f), new Vector2(420f, 34f), 24, TextAnchor.MiddleLeft, UIFactory.Ink);

            _goldText = UIFactory.Label(root, "GoldText", "", UIFactory.TopLeft, UIFactory.TopLeft,
                new Vector2(0f, -46f), new Vector2(420f, 34f), 28, TextAnchor.MiddleLeft, UIFactory.Gold);

            _potionText = UIFactory.Label(root, "PotionText", "", UIFactory.TopLeft, UIFactory.TopLeft,
                new Vector2(0f, -82f), new Vector2(420f, 34f), 24, TextAnchor.MiddleLeft, new Color(0.6f, 1f, 0.7f));

            _gearText = UIFactory.Label(root, "GearText", "", UIFactory.TopLeft, UIFactory.TopLeft,
                new Vector2(0f, -112f), new Vector2(520f, 30f), 20, TextAnchor.MiddleLeft, new Color(0.7f, 0.78f, 0.9f));

            // 보스 체력바는 보스가 있는 씬에서만 표시한다.
            _boss = Object.FindAnyObjectByType<BossController>();
            if (_boss != null)
            {
                _bossGroup = UIFactory.Group(canvas, "BossBar", UIFactory.BottomCenter, UIFactory.BottomCenter,
                    new Vector2(0f, 60f), new Vector2(900f, 60f));

                UIFactory.Label(_bossGroup, "BossName", "무너진 문의 파수꾼",
                    UIFactory.BottomCenter, UIFactory.BottomCenter, new Vector2(0f, 30f),
                    new Vector2(900f, 32f), 26, TextAnchor.MiddleCenter, UIFactory.Ink);

                _bossGauge = UIFactory.Gauge(_bossGroup, "BossGauge",
                    new Color(0.7f, 0.2f, 0.35f), new Color(0.1f, 0.1f, 0.12f, 0.9f),
                    UIFactory.BottomCenter, UIFactory.BottomCenter, Vector2.zero, new Vector2(900f, 22f));
            }
        }

        void Update() => Refresh(false);

        void Refresh(bool instant)
        {
            if (_player == null) _player = Object.FindAnyObjectByType<PlayerHealth>();

            var gm = GameManager.I;

            if (_hpGauge != null && _player != null)
                _hpGauge.SetRatio(_player.Max <= 0 ? 0f : (float)_player.Current / _player.Max, instant);

            if (_hpText != null && _player != null)
                _hpText.text = $"HP {_player.Current} / {_player.Max}";

            if (_goldText != null && gm != null)
                _goldText.text = $"골드 {gm.Gold} G";

            if (_potionText != null && gm != null)
                _potionText.text = $"물약 {gm.Potions} / {Balance.PotionMax}   [R]";

            if (_gearText != null && gm != null)
            {
                string sword = gm.HasSword ? "날카로운 검" : "기본 검";
                string armor = gm.HasArmor ? "가죽 갑옷" : "장비 없음";
                _gearText.text = $"{sword} · {armor}";
            }

            if (_bossGauge != null)
            {
                if (_boss == null || _boss.HealthRef == null || _boss.HealthRef.IsDead)
                {
                    if (_bossGroup != null) _bossGroup.gameObject.SetActive(false);
                }
                else
                {
                    _bossGauge.SetRatio(_boss.HealthRef.Normalized, instant);
                }
            }
        }
    }
}
