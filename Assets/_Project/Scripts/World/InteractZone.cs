using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// E 키 상호작용의 공통 처리. 트리거 안에 플레이어가 있을 때만 반응한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractZone : MonoBehaviour
    {
        public string prompt = "상호작용  [E]";

        bool _inside;
        string _shown;

        protected virtual string PromptText() => prompt;

        protected abstract void Interact();

        protected static bool IsPlayer(Collider2D other)
            => other.GetComponentInParent<PlayerController>() != null;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsPlayer(other)) return;

            _inside = true;
            ShowPrompt();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!IsPlayer(other)) return;

            _inside = false;
            HidePrompt();
        }

        void OnDisable() => HidePrompt();

        void Update()
        {
            if (!_inside) return;

            // 안내 문구가 바뀌는 경우(상자 개방 등)를 반영한다.
            if (_shown != PromptText()) ShowPrompt();

            if (InputReader.InteractDown) Interact();
        }

        protected void ShowPrompt()
        {
            HidePrompt();
            _shown = PromptText();
            if (!string.IsNullOrEmpty(_shown)) InteractPrompt.Show(_shown);
        }

        protected void HidePrompt()
        {
            if (string.IsNullOrEmpty(_shown)) return;

            InteractPrompt.Hide(_shown);
            _shown = null;
        }
    }
}
