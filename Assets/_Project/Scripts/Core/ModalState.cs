using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 상점·여관·사망창 같은 모달이 열렸을 때 게임을 멈추고 입력을 차단한다.
    /// 중첩 열림을 카운터로 관리해서 하나만 닫아도 시간이 풀려 버리는 사고를 막는다.
    /// </summary>
    public static class ModalState
    {
        static int _openCount;

        public static bool IsOpen => _openCount > 0;

        public static void Push()
        {
            _openCount++;
            Apply();
        }

        public static void Pop()
        {
            _openCount = Mathf.Max(0, _openCount - 1);
            Apply();
        }

        /// 씬 전환 시 timeScale이 0으로 남는 사고를 막기 위한 강제 초기화.
        public static void ForceCloseAll()
        {
            _openCount = 0;
            Apply();
        }

        static void Apply()
        {
            InputReader.Blocked = IsOpen;
            Time.timeScale = IsOpen ? 0f : 1f;
        }
    }
}
