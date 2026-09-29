using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>골드 상자. 탐색 보상의 최소 형태(계획서 §6).</summary>
    public class ChestZone : InteractZone
    {
        public int gold = Balance.ChestGold;

        bool _opened;

        protected override string PromptText()
            => _opened ? "" : $"상자 열기  [E]   (+{gold} G)";

        protected override void Interact()
        {
            if (_opened) return;

            _opened = true;

            if (GameManager.I != null) GameManager.I.AddGold(gold);
            Sfx.PlayCoin();
            DamagePopup.Spawn(transform.position + Vector3.up * 0.9f, gold, UIFactory.Gold);

            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = new Color(0.35f, 0.32f, 0.26f);

            HidePrompt();
        }
    }
}
