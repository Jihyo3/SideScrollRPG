using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 돌진형. 시야 안에 들어오면 예비동작 후 직선 돌진한다.
    /// 돌진 방향으로 대시하면 무적 프레임으로 통과할 수 있다(계획서 §4).
    /// </summary>
    public class ChargerEnemy : EnemyBase
    {
        enum State { Idle, Windup, Charge, Cooldown }

        State _state = State.Idle;
        float _timer;

        static readonly Color WindupColor = new Color(1f, 0.75f, 0.3f);

        void Update()
        {
            if (Hp.IsDead) return;

            float dt = Time.deltaTime;

            switch (_state)
            {
                case State.Idle: TickIdle(dt); break;
                case State.Windup: TickWindup(dt); break;
                case State.Charge: TickCharge(dt); break;
                case State.Cooldown: TickCooldown(dt); break;
            }
        }

        void TickIdle(float dt)
        {
            Brake(dt);

            if (!HasLivingPlayer()) return;

            FaceTowardPlayer();

            float range = data != null ? data.detectRange : Balance.ChargerDetectRange;
            if (DistanceToPlayer() > range) return;

            _state = State.Windup;
            _timer = data != null ? data.windup : Balance.ChargerWindup;
            if (Flash != null) Flash.SetBaseColor(WindupColor);
        }

        void TickWindup(float dt)
        {
            Brake(dt);
            FaceTowardPlayer();

            _timer -= dt;
            if (_timer > 0f) return;

            _state = State.Charge;
            _timer = data != null ? data.dashDuration : Balance.ChargerDashDuration;

            if (Flash != null && data != null) Flash.SetBaseColor(data.tint);
            if (hitbox != null) hitbox.Begin();   // 한 번의 돌진에 플레이어는 최대 1회만 맞는다
        }

        void TickCharge(float dt)
        {
            float speed = data != null ? data.dashSpeed : Balance.ChargerDashSpeed;
            SetHorizontalSpeed(Facing * speed);

            if (hitbox != null)
                hitbox.Strike(BuildAttack(Attack, Balance.ShakeLight), Facing);

            _timer -= dt;
            if (_timer > 0f) return;

            _state = State.Cooldown;
            _timer = data != null ? data.cooldown : Balance.ChargerCooldown;
        }

        void TickCooldown(float dt)
        {
            Brake(dt);

            _timer -= dt;
            if (_timer <= 0f) _state = State.Idle;
        }
    }
}
