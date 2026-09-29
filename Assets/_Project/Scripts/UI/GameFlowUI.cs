using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 게임플레이 씬의 사망 / 보스 격파 화면. 둘 다 모달로 띄워 게임을 멈춘다.
    /// </summary>
    public class GameFlowUI : MonoBehaviour
    {
        RectTransform _deathRoot, _victoryRoot;
        PlayerHealth _player;
        BossController _boss;
        bool _resolved;

        void Start()
        {
            BuildDeath();

            _player = Object.FindAnyObjectByType<PlayerHealth>();
            if (_player != null) _player.OnDied += ShowDeath;

            _boss = Object.FindAnyObjectByType<BossController>();
            if (_boss != null && _boss.HealthRef != null)
            {
                BuildVictory();
                _boss.HealthRef.OnDied += HandleBossDied;
            }
        }

        void OnDestroy()
        {
            if (_player != null) _player.OnDied -= ShowDeath;
            if (_boss != null && _boss.HealthRef != null) _boss.HealthRef.OnDied -= HandleBossDied;
        }

        void HandleBossDied(Health _) => ShowVictory();

        void BuildDeath()
        {
            var canvas = UIRoot.Canvas;
            if (canvas == null) return;

            _deathRoot = UIFactory.Group(canvas, "DeathScreen", UIFactory.Center, UIFactory.Center,
                Vector2.zero, new Vector2(1920f, 1080f));

            UIFactory.FullScreen(_deathRoot, "Dim", new Color(0.03f, 0.02f, 0.04f, 0.88f));

            UIFactory.Label(_deathRoot, "Title", "쓰러졌다", UIFactory.Center, UIFactory.Center,
                new Vector2(0f, 120f), new Vector2(900f, 90f), 72, TextAnchor.MiddleCenter,
                new Color(0.9f, 0.3f, 0.32f));

            UIFactory.Label(_deathRoot, "Desc", "골드와 장비는 그대로 유지된다.\n마을에서 다시 준비하자.",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, 20f), new Vector2(900f, 80f),
                28, TextAnchor.MiddleCenter, UIFactory.Ink);

            UIFactory.TextButton(_deathRoot, "TownButton", "마을로 돌아가기",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, -80f), new Vector2(420f, 64f), 28,
                () => { if (GameManager.I != null) GameManager.I.ReviveInTown(); });

            UIFactory.TextButton(_deathRoot, "TitleButton", "타이틀로",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, -160f), new Vector2(420f, 52f), 24,
                () => { if (GameManager.I != null) GameManager.I.GoTo(SceneNames.Boot); });

            _deathRoot.gameObject.SetActive(false);
        }

        void BuildVictory()
        {
            var canvas = UIRoot.Canvas;
            if (canvas == null) return;

            _victoryRoot = UIFactory.Group(canvas, "VictoryScreen", UIFactory.Center, UIFactory.Center,
                Vector2.zero, new Vector2(1920f, 1080f));

            UIFactory.FullScreen(_victoryRoot, "Dim", new Color(0.02f, 0.03f, 0.05f, 0.88f));

            UIFactory.Label(_victoryRoot, "Title", "파수꾼 격파", UIFactory.Center, UIFactory.Center,
                new Vector2(0f, 110f), new Vector2(900f, 90f), 72, TextAnchor.MiddleCenter,
                new Color(1f, 0.85f, 0.4f));

            UIFactory.Label(_victoryRoot, "Desc", "무너진 문이 열렸다.",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, 20f), new Vector2(900f, 60f),
                30, TextAnchor.MiddleCenter, UIFactory.Ink);

            UIFactory.TextButton(_victoryRoot, "EndingButton", "엔딩 보기",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, -80f), new Vector2(420f, 64f), 28,
                () => { if (GameManager.I != null) GameManager.I.DefeatBoss(); });

            _victoryRoot.gameObject.SetActive(false);
        }

        void ShowDeath()
        {
            if (_resolved || _deathRoot == null) return;

            _resolved = true;
            _deathRoot.gameObject.SetActive(true);
            ModalState.Push();
        }

        void ShowVictory()
        {
            if (_resolved || _victoryRoot == null) return;

            _resolved = true;
            _victoryRoot.gameObject.SetActive(true);
            ModalState.Push();
        }
    }
}
