using UnityEngine;
using UnityEngine.SceneManagement;

namespace SideScrollRPG
{
    /// <summary>
    /// 씬을 넘나드는 유일한 상태 보관소. 저장 파일은 만들지 않는다(계획서 §7).
    /// 어떤 씬에서 Play를 눌러도 자동 생성되므로 프리팹을 배치할 필요가 없다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public int Gold { get; private set; }
        public int CurrentHP { get; private set; } = Balance.PlayerMaxHP;
        public int Potions { get; private set; }
        public bool HasSword { get; private set; }
        public bool HasArmor { get; private set; }
        public bool BossDefeated { get; private set; }

        public int MaxHP => Balance.PlayerMaxHP;
        public float AttackMultiplier => HasSword ? 1f + Balance.SwordAttackBonus : 1f;
        public float DamageTakenMultiplier => HasArmor ? 1f - Balance.ArmorDamageReduction : 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (I != null) return;
            var go = new GameObject("[GameManager]");
            go.AddComponent<GameManager>();
        }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── 진행 ─────────────────────────────────────────

        public void NewGame()
        {
            Gold = 0;
            CurrentHP = Balance.PlayerMaxHP;
            Potions = 0;
            HasSword = false;
            HasArmor = false;
            BossDefeated = false;
            GoTo(SceneNames.Town);
        }

        public void GoTo(string sceneName)
        {
            ModalState.ForceCloseAll();
            SceneManager.LoadScene(sceneName);
        }

        /// 사망 처리: 골드·장비는 유지하고 HP만 최소 복구한 뒤 마을로 돌려보낸다.
        public void ReviveInTown()
        {
            CurrentHP = Balance.ReviveHP;
            GoTo(SceneNames.Town);
        }

        public void ClearRoute(int clearBonus)
        {
            AddGold(clearBonus);
            GoTo(SceneNames.Town);
        }

        public void DefeatBoss()
        {
            BossDefeated = true;
            GoTo(SceneNames.Ending);
        }

        // ── 체력 ─────────────────────────────────────────

        public void SyncHP(int hp) => CurrentHP = Mathf.Clamp(hp, 0, MaxHP);

        public void HealFull() => CurrentHP = MaxHP;

        // ── 골드 ─────────────────────────────────────────

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            Gold += amount;
        }

        public bool TrySpend(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            return true;
        }

        // ── 상점 ─────────────────────────────────────────

        public bool CanBuySword => !HasSword && Gold >= Balance.SwordPrice;
        public bool CanBuyArmor => !HasArmor && Gold >= Balance.ArmorPrice;
        public bool CanBuyPotion => Potions < Balance.PotionMax && Gold >= Balance.PotionPrice;
        public bool CanUseInn => Gold >= Balance.InnPrice && CurrentHP < MaxHP;

        public bool BuySword()
        {
            if (!CanBuySword || !TrySpend(Balance.SwordPrice)) return false;
            HasSword = true;
            return true;
        }

        public bool BuyArmor()
        {
            if (!CanBuyArmor || !TrySpend(Balance.ArmorPrice)) return false;
            HasArmor = true;
            return true;
        }

        public bool BuyPotion()
        {
            if (!CanBuyPotion || !TrySpend(Balance.PotionPrice)) return false;
            Potions++;
            return true;
        }

        public bool UseInn()
        {
            if (!CanUseInn || !TrySpend(Balance.InnPrice)) return false;
            HealFull();
            var ph = Object.FindAnyObjectByType<PlayerHealth>();
            if (ph != null) ph.PullFromGameManager();
            return true;
        }

        public bool ConsumePotion()
        {
            if (Potions <= 0) return false;
            Potions--;
            return true;
        }
    }
}
