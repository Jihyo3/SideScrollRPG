using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SideScrollRPG.EditorTools
{
    /// <summary>
    /// 셋업 결과가 실제로 맞게 조립됐는지 씬을 열어 검사한다.
    /// 배치 실행: -executeMethod SideScrollRPG.EditorTools.ProjectAudit.Run
    /// </summary>
    public static class ProjectAudit
    {
        static readonly List<string> Problems = new List<string>();
        static readonly StringBuilder Report = new StringBuilder();

        [MenuItem("SideScrollRPG/셋업 검증", false, 2)]
        public static void Run()
        {
            Problems.Clear();
            Report.Clear();

            AuditScene(SceneNames.Boot, AuditBoot);
            AuditScene(SceneNames.Town, AuditTown);
            AuditScene(SceneNames.Route01, AuditRoute01);
            AuditScene(SceneNames.Route02, AuditRoute02);
            AuditScene(SceneNames.BossArena, AuditBoss);
            AuditScene(SceneNames.Ending, AuditEnding);

            Report.Insert(0, Problems.Count == 0
                ? "[AUDIT] 통과 — 문제 없음\n"
                : $"[AUDIT] 문제 {Problems.Count}건\n" + string.Join("\n", Problems.Select(p => "  ! " + p)) + "\n");

            Debug.Log(Report.ToString());
        }

        static void AuditScene(string sceneName, System.Action body)
        {
            string path = $"Assets/_Project/Scenes/{sceneName}.unity";
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            Report.AppendLine($"[{sceneName}]");
            body();
            Report.AppendLine();
        }

        static void Fail(string message) => Problems.Add(message);

        static void Line(string message) => Report.AppendLine("  " + message);

        static void Expect(bool condition, string message)
        {
            if (!condition) Fail(message);
        }

        static void ExpectCount<T>(int expected, string label) where T : Component
        {
            int found = Object.FindObjectsByType<T>(FindObjectsSortMode.None).Length;
            Line($"{label}: {found}개");
            if (found != expected) Fail($"{label} 개수가 {found}개 (기대값 {expected})");
        }

        // ── 씬별 검사 ────────────────────────────────────

        static void AuditBoot()
        {
            ExpectCount<TitleScreen>(1, "TitleScreen");
            ExpectCount<UIRoot>(1, "UIRoot");
            Expect(Camera.main != null, "Boot: MainCamera 없음");
        }

        static void AuditEnding()
        {
            ExpectCount<EndingScreen>(1, "EndingScreen");
            ExpectCount<UIRoot>(1, "UIRoot");
        }

        static void AuditTown()
        {
            AuditGameplayCommon();
            ExpectCount<ShopZone>(1, "ShopZone");
            ExpectCount<InnZone>(1, "InnZone");
            ExpectCount<ShopPanel>(1, "ShopPanel");
            ExpectCount<InnPanel>(1, "InnPanel");
            ExpectCount<TravelZone>(3, "TravelZone(루트A/루트B/보스)");

            foreach (var zone in Object.FindObjectsByType<TravelZone>(FindObjectsSortMode.None))
            {
                Expect(!string.IsNullOrEmpty(zone.targetScene),
                    $"Town: {zone.name}의 targetScene이 비어 있음");
                Line($"게이트 {zone.name} → {zone.targetScene}");
            }
        }

        static void AuditRoute01()
        {
            AuditGameplayCommon();
            ExpectCount<ChargerEnemy>(4, "돌진형");
            ExpectCount<ArcherEnemy>(0, "사격형");
            ExpectCount<ChestZone>(1, "골드 상자");
            ExpectCount<LevelExit>(1, "출구");
            AuditFallZones();
            AuditEnemyGrounding();
            AuditRouteReward(Balance.RouteAClearBonus, 40);
        }

        static void AuditRoute02()
        {
            AuditGameplayCommon();
            ExpectCount<ChargerEnemy>(3, "돌진형");
            ExpectCount<ArcherEnemy>(3, "사격형");
            ExpectCount<ChestZone>(1, "골드 상자");
            ExpectCount<LevelExit>(1, "출구");
            AuditFallZones();
            AuditEnemyGrounding();
            AuditRouteReward(Balance.RouteBClearBonus, 70);

            foreach (var archer in Object.FindObjectsByType<ArcherEnemy>(FindObjectsSortMode.None))
            {
                Expect(archer.projectilePrefab != null, $"Route02: {archer.name}에 투사체 프리팹이 없음");
                Expect(archer.muzzle != null, $"Route02: {archer.name}에 muzzle이 없음");
            }
        }

        static void AuditBoss()
        {
            AuditGameplayCommon();
            ExpectCount<BossController>(1, "보스");

            var boss = Object.FindAnyObjectByType<BossController>();
            if (boss == null) { Fail("BossArena: 보스가 없음"); return; }

            Expect(boss.hitbox != null, "BossArena: 근접 히트박스 미할당");
            Expect(boss.shockwaveHitbox != null, "BossArena: 충격파 히트박스 미할당");
            Expect(boss.data != null && boss.data.maxHP == Balance.BossHP,
                $"BossArena: 보스 HP가 {Balance.BossHP}이 아님");

            var health = boss.GetComponent<Health>();
            Expect(health != null && health.maxHP == Balance.BossHP, "BossArena: Health.maxHP 불일치");

            Line($"보스 HP {Balance.BossHP} / 공격력 {Balance.BossAttack}");
        }

        // ── 공통 검사 ────────────────────────────────────

        static void AuditGameplayCommon()
        {
            var player = Object.FindAnyObjectByType<PlayerController>();
            if (player == null) { Fail("플레이어가 배치되지 않음"); return; }

            Expect(player.groundCheck != null, "플레이어: groundCheck 미할당");
            Expect(player.groundMask.value != 0, "플레이어: groundMask가 비어 있음");

            var combat = player.GetComponent<PlayerCombat>();
            Expect(combat != null && combat.hitbox != null, "플레이어: 공격 히트박스 미할당");
            Expect(combat == null || combat.hitbox == null || combat.hitbox.targetLayers.value != 0,
                "플레이어: 히트박스 targetLayers가 비어 있음");
            Expect(player.GetComponent<PlayerHealth>() != null, "플레이어: PlayerHealth 없음");

            var camera = Camera.main;
            Expect(camera != null, "MainCamera 없음");

            var follow = camera != null ? camera.GetComponent<CameraFollow>() : null;
            Expect(follow != null, "카메라: CameraFollow 없음");
            Expect(follow == null || follow.target != null, "카메라: 추적 대상 미할당");

            ExpectCount<UIRoot>(1, "UIRoot");
            ExpectCount<HudView>(1, "HudView");
            ExpectCount<InteractPrompt>(1, "InteractPrompt");
            ExpectCount<GameFlowUI>(1, "GameFlowUI");

            // 플레이어가 지면 위에 떠 있거나 박혀 있지 않은지 확인
            float gap = GapToGroundBelow(player.transform.position, 0.9f);
            Line($"플레이어 발밑 간격: {gap:0.00}u");
            Expect(gap >= -0.05f && gap <= 0.6f, $"플레이어 스폰 높이가 부적절함 (간격 {gap:0.00}u)");
        }

        static void AuditFallZones()
        {
            var zones = Object.FindObjectsByType<FallZone>(FindObjectsSortMode.None);
            Line($"낙하 존: {zones.Length}개");
            Expect(zones.Length > 0, "낙하 존이 없음 — 틈에 빠지면 무한 낙하");

            foreach (var zone in zones)
                Expect(zone.respawnPoint != null, $"{zone.name}: respawnPoint 미할당");
        }

        static void AuditEnemyGrounding()
        {
            foreach (var enemy in Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
            {
                var collider = enemy.GetComponent<Collider2D>();
                float half = collider != null ? collider.bounds.extents.y : 0.5f;

                float gap = GapToGroundBelow(enemy.transform.position, half);
                Line($"{enemy.name} 발밑 간격: {gap:0.00}u");

                // 살짝 띄워서 배치하는 것은 정상(중력으로 내려앉는다). 크게 뜨거나 박히면 문제.
                Expect(gap >= -0.15f && gap <= 1.2f, $"{enemy.name} 배치 높이가 부적절함 (간격 {gap:0.00}u)");
            }
        }

        static void AuditRouteReward(int clearBonus, int target)
        {
            int fromEnemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None)
                .Select(e => e.data != null ? e.data.goldDrop : 0).Sum();

            int fromChests = Object.FindObjectsByType<ChestZone>(FindObjectsSortMode.None)
                .Select(c => c.gold).Sum();

            int total = fromEnemies + fromChests + clearBonus;
            Line($"완주 보상: 적 {fromEnemies} + 상자 {fromChests} + 클리어 {clearBonus} = {total} G (계획 {target} G)");

            if (Mathf.Abs(total - target) > 10)
                Fail($"루트 보상 {total} G가 계획서의 {target} G에서 10G 이상 벗어남");
        }

        /// 지면 레이어 콜라이더 중 대상 아래에 있는 가장 가까운 윗면과의 간격.
        static float GapToGroundBelow(Vector3 position, float halfHeight)
        {
            int groundLayer = LayerMask.NameToLayer(Layers.Ground);
            float bottom = position.y - halfHeight;
            float best = float.MaxValue;

            foreach (var collider in Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None))
            {
                if (collider.gameObject.layer != groundLayer) continue;

                var bounds = collider.bounds;
                if (position.x < bounds.min.x || position.x > bounds.max.x) continue;
                if (bounds.max.y > bottom + 0.2f) continue;   // 대상보다 위에 있는 지면은 무시

                float gap = bottom - bounds.max.y;
                if (gap < best) best = gap;
            }

            return best == float.MaxValue ? 999f : best;
        }
    }
}
