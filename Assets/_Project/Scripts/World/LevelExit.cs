namespace SideScrollRPG
{
    /// <summary>루트 끝. 클리어 보너스를 주고 마을로 돌려보낸다.</summary>
    public class LevelExit : InteractZone
    {
        public int clearBonus = Balance.RouteAClearBonus;

        protected override string PromptText() => $"마을로 귀환  [E]   (+{clearBonus} G)";

        protected override void Interact()
        {
            if (GameManager.I == null) return;

            HidePrompt();
            GameManager.I.ClearRoute(clearBonus);
        }
    }
}
