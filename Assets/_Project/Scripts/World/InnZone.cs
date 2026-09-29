namespace SideScrollRPG
{
    public class InnZone : InteractZone
    {
        protected override string PromptText() => $"여관  [E]   ({Balance.InnPrice} G · 체력 전체 회복)";

        protected override void Interact() => InnPanel.Open();
    }
}
