using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 입력을 한 군데로 모아 두는 얇은 레이어.
    /// 지금은 레거시 Input Manager를 쓴다. Input System으로 갈아탈 때 이 파일만 고치면 된다.
    /// </summary>
    public static class InputReader
    {
        /// 모달 UI(상점·여관·사망창)가 열려 있으면 게임플레이 입력을 모두 막는다.
        public static bool Blocked { get; set; }

        public static float MoveX => Blocked ? 0f : Input.GetAxisRaw("Horizontal");

        public static bool JumpDown => !Blocked && (Input.GetKeyDown(KeyCode.Space) || GetButtonDownSafe("Jump"));
        public static bool JumpHeld => !Blocked && (Input.GetKey(KeyCode.Space) || GetButtonSafe("Jump"));

        public static bool DashDown => !Blocked && (Input.GetKeyDown(KeyCode.LeftShift) || GetButtonDownSafe("Fire3"));

        public static bool LightAttackDown => !Blocked &&
            (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.J));

        public static bool HeavyAttackDown => !Blocked &&
            (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.K));

        public static bool PotionDown => !Blocked && Input.GetKeyDown(KeyCode.R);

        /// 상호작용은 모달이 닫혀 있을 때만 유효하다.
        public static bool InteractDown => !Blocked && Input.GetKeyDown(KeyCode.E);

        /// 모달 닫기는 Blocked 상태에서도 받아야 한다.
        public static bool CancelDown => Input.GetKeyDown(KeyCode.Escape);

        // 프로젝트 설정에 축이 없을 때 예외로 죽지 않도록 감싼다.
        static bool GetButtonDownSafe(string name)
        {
            try { return Input.GetButtonDown(name); }
            catch { return false; }
        }

        static bool GetButtonSafe(string name)
        {
            try { return Input.GetButton(name); }
            catch { return false; }
        }
    }
}
