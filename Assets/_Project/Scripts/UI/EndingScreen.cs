using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>엔딩. 한 바퀴가 끝났다는 것만 알려 주는 최소 화면.</summary>
    public class EndingScreen : MonoBehaviour
    {
        void Start()
        {
            ModalState.ForceCloseAll();

            var canvas = UIRoot.Canvas;
            if (canvas == null) return;

            UIFactory.FullScreen(canvas, "Bg", new Color(0.04f, 0.05f, 0.08f, 1f));

            UIFactory.Label(canvas, "Title", "문을 지나며", UIFactory.Center, UIFactory.Center,
                new Vector2(0f, 220f), new Vector2(1200f, 110f), 76, TextAnchor.MiddleCenter,
                new Color(1f, 0.88f, 0.55f));

            string gold = GameManager.I != null ? GameManager.I.Gold.ToString() : "0";
            string gear = "";
            if (GameManager.I != null)
            {
                gear = (GameManager.I.HasSword ? "날카로운 검" : "기본 검") + " · " +
                       (GameManager.I.HasArmor ? "가죽 갑옷" : "갑옷 없음");
            }

            UIFactory.Label(canvas, "Body",
                "파수꾼은 무너졌고, 문 너머는 아직 비어 있다.\n" +
                "다음 지역은 프로토타입 범위 밖이다.",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, 90f), new Vector2(1200f, 100f),
                30, TextAnchor.MiddleCenter, UIFactory.Ink);

            UIFactory.Label(canvas, "Stats", $"남은 골드  {gold} G\n{gear}",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, -30f), new Vector2(1200f, 90f),
                28, TextAnchor.MiddleCenter, UIFactory.Gold);

            UIFactory.TextButton(canvas, "TitleButton", "타이틀로",
                UIFactory.Center, UIFactory.Center, new Vector2(0f, -170f), new Vector2(420f, 64f), 28,
                () => { if (GameManager.I != null) GameManager.I.GoTo(SceneNames.Boot); });
        }
    }
}
