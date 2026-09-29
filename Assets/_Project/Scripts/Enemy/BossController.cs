using System.Collections;
using UnityEngine;

namespace SideScrollRPG
{
    /// <summary>
    /// 보스 "무너진 문의 파수꾼". 2페이즈(계획서 §4).
    ///  1페이즈: 3연타 휘두르기 / 전방 돌진
    ///  2페이즈: + 지면 충격파(점프로만 회피) / 전체 속도 1.2배
    /// 패턴 사이 경직이 플레이어의 딜 윈도우이며, 난이도 조절 노브다.
    /// </summary>
    public class BossController : EnemyBase
    {
        public Hitbox shockwaveHitbox;
        public float approachSpeed = 3.2f;
        public float chargeSpeed = 10f;

        public Health HealthRef => Hp;
        public bool IsPhaseTwo => Hp != null && Hp.Normalized <= Balance.BossPhase2Threshold;

        static readonly Color WindupColor = new Color(1f, 0.7f, 0.25f);
        static readonly Color PhaseTwoColor = new Color(0.75f, 0.2f, 0.45f);

        bool _phaseTwoAnnounced;

        protected override void Start()
        {
            base.Start();
            StartCoroutine(Brain());
        }

        float SpeedMult => IsPhaseTwo ? Balance.BossPhase2SpeedMult : 1f;

        IEnumerator Brain()
        {
            // 플레이어가 아직 생성되지 않았을 수 있으므로 한 프레임 양보한다.
            yield return null;

            int patternIndex = 0;

            while (Hp != null && !Hp.IsDead)
            {
                yield return Interlude(Balance.BossIdleBetweenPatterns);

                if (Hp.IsDead) break;

                CheckPhaseTransition();

                if (IsPhaseTwo && patternIndex % 3 == 2)
                    yield return Shockwave();
                else if (patternIndex % 2 == 0)
                    yield return TripleSwing();
                else
                    yield return ChargeAttack();

                patternIndex++;
            }

            SetHorizontalSpeed(0f);
        }

        void CheckPhaseTransition()
        {
            if (_phaseTwoAnnounced || !IsPhaseTwo) return;

            _phaseTwoAnnounced = true;
            if (Flash != null) Flash.SetBaseColor(PhaseTwoColor);
            CameraFollow.Shake(Balance.ShakeHeavy, 0.3f);
        }

        /// 패턴 사이의 경직. 멀면 걸어서 접근한다.
        IEnumerator Interlude(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                if (Hp.IsDead) yield break;

                if (HasLivingPlayer())
                {
                    FaceTowardPlayer();
                    float dist = DistanceToPlayer();
                    if (dist > 3f) SetHorizontalSpeed(Facing * approachSpeed * SpeedMult);
                    else Brake(Time.deltaTime);
                }
                else
                {
                    Brake(Time.deltaTime);
                }

                t += Time.deltaTime;
                yield return null;
            }

            SetHorizontalSpeed(0f);
        }

        IEnumerator TripleSwing()
        {
            FaceTowardPlayer();
            yield return Telegraph(0.6f);

            for (int i = 0; i < 3; i++)
            {
                if (Hp.IsDead) yield break;

                if (hitbox != null)
                {
                    hitbox.Begin();
                    hitbox.Strike(BuildAttack(Attack, Balance.ShakeLight), Facing);
                }

                yield return WaitScaled(0.25f);
            }

            yield return WaitScaled(0.8f);
        }

        IEnumerator ChargeAttack()
        {
            FaceTowardPlayer();
            yield return Telegraph(0.5f);

            if (hitbox != null) hitbox.Begin();

            float duration = 0.6f;
            float t = 0f;
            while (t < duration)
            {
                if (Hp.IsDead) { SetHorizontalSpeed(0f); yield break; }

                SetHorizontalSpeed(Facing * chargeSpeed * SpeedMult);
                if (hitbox != null)
                    hitbox.Strike(BuildAttack(Attack, Balance.ShakeHeavy), Facing);

                t += Time.deltaTime;
                yield return null;
            }

            SetHorizontalSpeed(0f);
            yield return WaitScaled(0.7f);
        }

        IEnumerator Shockwave()
        {
            Brake(1f);
            yield return Telegraph(0.7f);

            if (shockwaveHitbox != null)
            {
                shockwaveHitbox.Begin();
                shockwaveHitbox.Strike(BuildAttack(Attack, Balance.ShakeHeavy), Facing);
            }

            CameraFollow.Shake(Balance.ShakeHeavy, 0.25f);
            yield return WaitScaled(0.9f);
        }

        /// 예비동작: 색으로 알려 주고, 2페이즈에서는 더 짧아진다.
        IEnumerator Telegraph(float seconds)
        {
            if (Flash != null) Flash.SetBaseColor(WindupColor);

            float t = 0f;
            float scaled = seconds / SpeedMult;
            while (t < scaled)
            {
                if (Hp.IsDead) yield break;
                Brake(Time.deltaTime);
                t += Time.deltaTime;
                yield return null;
            }

            if (Flash != null)
                Flash.SetBaseColor(IsPhaseTwo ? PhaseTwoColor : (data != null ? data.tint : Color.red));
        }

        IEnumerator WaitScaled(float seconds)
        {
            yield return new WaitForSeconds(seconds / SpeedMult);
        }
    }
}
