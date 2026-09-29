using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>마을의 루트 입구 · 보스 입구. 씬을 바꾼다.</summary>
    public class TravelZone : InteractZone
    {
        public string targetScene = SceneNames.Route01;
        public string displayName = "루트";
        public string subtitle = "";

        protected override string PromptText()
            => string.IsNullOrEmpty(subtitle)
                ? $"{displayName}  [E]"
                : $"{displayName}  [E]   ({subtitle})";

        protected override void Interact()
        {
            if (GameManager.I == null || string.IsNullOrEmpty(targetScene)) return;

            HidePrompt();
            GameManager.I.GoTo(targetScene);
        }
    }
}
