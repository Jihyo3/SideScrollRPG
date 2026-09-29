namespace SideScrollRPG
{
    public class ShopZone : InteractZone
    {
        protected override string PromptText() => "상점  [E]";

        protected override void Interact() => ShopPanel.Open();
    }
}
