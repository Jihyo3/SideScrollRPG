using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>타이틀 화면. 조작법을 여기서 한 번만 보여 준다.</summary>
    public class TitleScreen : MonoBehaviour
    {
        const string Controls =
            "이동  A / D          점프  Space          대시  Shift\n" +
            "약공격  좌클릭 또는 J          강공격  우클릭 또는 K\n" +
            "물약  R          상호작용  E          닫기  ESC";

        void Start()
        {
            ModalState.ForceCloseAll();

            var canvas = UIRoot.Canvas;
            if (canvas == null) return;

            UIFactory.FullScreen(canvas, "Bg", new Color(0.05f, 0.06f, 0.09f, 1f));

            UIFactory.Label(canvas, "Title", "무너진 문", UIFactory.Center, UIFactory.Center,
                new Vector2(0f, 260f), new Vector2(1200f, 120f), 86, TextAnchor.MiddleCenter,
                UIFactory.Ink);

            UIFactory.Label(canvas, "Subtitle", "횡스크롤 액션 RPG · 프로토타입",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, 180f), new Vector2(1200f, 50f),
                28, TextAnchor.MiddleCenter, new Color(0.65f, 0.7f, 0.82f));

            UIFactory.TextButton(canvas, "StartButton", "게임 시작",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, 30f), new Vector2(420f, 70f), 32,
                () => { if (GameManager.I != null) GameManager.I.NewGame(); });

            UIFactory.TextButton(canvas, "QuitButton", "종료",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, -60f), new Vector2(420f, 56f), 26,
                Application.Quit);

            UIFactory.Label(canvas, "Controls", Controls,
                UIFactory.BottomCenter, UIFactory.BottomCenter, new Vector2(0f, 90f),
                new Vector2(1400f, 130f), 24, TextAnchor.MiddleCenter, new Color(0.6f, 0.66f, 0.78f));
        }
    }
}
