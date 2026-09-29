using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SideScrollRPG.EditorTools
{
    /// <summary>
    /// 폴더 · 레이어 · 스프라이트 · 데이터 · 프리팹 · 씬 · 빌드 설정을 한 번에 만든다.
    /// 메뉴( SideScrollRPG > 프로젝트 셋업 실행 )에서 돌리거나
    /// -executeMethod SideScrollRPG.EditorTools.ProjectSetup.RunAll 로 배치 실행한다.
    /// 여러 번 실행해도 같은 결과가 되도록 전부 덮어쓰기로 동작한다.
    /// </summary>
    public static class ProjectSetup
    {
        const string Root = "Assets/_Project";
        const string ArtDir = Root + "/Art";
        const string DataDir = Root + "/Data";
        const string PrefabDir = Root + "/Prefabs";
        const string SceneDir = Root + "/Scenes";
        const string SpritePath = ArtDir + "/white_square.png";

        // 지면 윗면을 y = 0 으로 고정한다. 모든 배치가 이 기준을 따른다.
        const float GroundTop = 0f;
        const float PlayerSpawnY = 0.95f;

        static readonly Color ColPlayer = new Color(0.36f, 0.66f, 1f);
        static readonly Color ColCharger = new Color(0.85f, 0.25f, 0.25f);
        static readonly Color ColArcher = new Color(0.92f, 0.82f, 0.28f);
        static readonly Color ColBoss = new Color(0.45f, 0.13f, 0.22f);
        static readonly Color ColGround = new Color(0.20f, 0.22f, 0.26f);
        static readonly Color ColPlatform = new Color(0.26f, 0.28f, 0.33f);
        static readonly Color ColShop = new Color(0.78f, 0.55f, 0.28f);
        static readonly Color ColInn = new Color(0.36f, 0.55f, 0.72f);
        static readonly Color ColGate = new Color(0.52f, 0.46f, 0.62f);
        static readonly Color ColBossGate = new Color(0.62f, 0.26f, 0.36f);
        static readonly Color ColChest = new Color(0.86f, 0.72f, 0.32f);
        static readonly Color ColProjectile = new Color(1f, 0.92f, 0.45f);

        // ── 진입점 ───────────────────────────────────────

        [MenuItem("SideScrollRPG/프로젝트 셋업 실행", false, 1)]
        public static void RunAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolders();
            EnsureLayers();

            var sprite = EnsureSprite();
            var data = CreateEnemyData();
            var prefabs = CreatePrefabs(sprite, data);

            BuildBootScene();
            BuildTownScene(sprite, prefabs);
            BuildRoute01(sprite, prefabs);
            BuildRoute02(sprite, prefabs);
            BuildBossArena(sprite, prefabs);
            BuildEndingScene();

            RegisterScenes();
            ConfigurePlayerSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SideScrollRPG] 셋업 완료 — Boot 씬을 열고 Play를 누르면 전체 루프가 돌아갑니다.");
        }

        // ── 폴더 ─────────────────────────────────────────

        static void EnsureFolders()
        {
            EnsureFolder("Assets", "_Project");
            foreach (var name in new[] { "Art", "Audio", "Data", "Prefabs", "Scenes", "Scripts", "Editor" })
                EnsureFolder(Root, name);
            EnsureFolder(Root, "Resources");
            EnsureFolder(Root + "/Resources", "Audio");
        }

        static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }

        // ── 레이어 ───────────────────────────────────────

        static void EnsureLayers()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("[SideScrollRPG] TagManager.asset을 찾지 못했습니다. 레이어를 수동으로 추가해야 합니다.");
                return;
            }

            var so = new SerializedObject(assets[0]);
            var layers = so.FindProperty("layers");

            foreach (var layerName in Layers.All)
            {
                if (HasLayer(layers, layerName)) continue;

                bool placed = false;
                for (int i = 8; i < layers.arraySize; i++)
                {
                    var element = layers.GetArrayElementAtIndex(i);
                    if (!string.IsNullOrEmpty(element.stringValue)) continue;

                    element.stringValue = layerName;
                    placed = true;
                    break;
                }

                if (!placed) Debug.LogWarning($"[SideScrollRPG] 빈 레이어 슬롯이 없어 '{layerName}'을 추가하지 못했습니다.");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        static bool HasLayer(SerializedProperty layers, string name)
        {
            for (int i = 0; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).stringValue == name) return true;
            return false;
        }

        static int L(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            return layer < 0 ? 0 : layer;
        }

        // ── 스프라이트 ───────────────────────────────────

        /// 1×1 유닛 흰색 사각형 하나로 모든 그레이박스 비주얼을 만든다.
        static Sprite EnsureSprite()
        {
            string full = Path.GetFullPath(SpritePath);

            if (!File.Exists(full))
            {
                var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                var pixels = new Color32[32 * 32];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                texture.SetPixels32(pixels);
                texture.Apply();

                File.WriteAllBytes(full, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);

                AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        // ── 데이터 에셋 ──────────────────────────────────

        class EnemyDataSet
        {
            public EnemyData Charger, Archer, Boss;
        }

        static EnemyDataSet CreateEnemyData()
        {
            var set = new EnemyDataSet();

            set.Charger = UpsertData("Enemy_Charger", d =>
            {
                d.displayName = "돌진형";
                d.maxHP = Balance.ChargerHP;
                d.attack = Balance.ChargerAttack;
                d.goldDrop = Balance.ChargerGold;
                d.tint = ColCharger;
                d.detectRange = Balance.ChargerDetectRange;
                d.moveSpeed = 3f;
                d.windup = Balance.ChargerWindup;
                d.dashSpeed = Balance.ChargerDashSpeed;
                d.dashDuration = Balance.ChargerDashDuration;
                d.cooldown = Balance.ChargerCooldown;
            });

            set.Archer = UpsertData("Enemy_Archer", d =>
            {
                d.displayName = "사격형";
                d.maxHP = Balance.ArcherHP;
                d.attack = Balance.ArcherAttack;
                d.goldDrop = Balance.ArcherGold;
                d.tint = ColArcher;
                d.detectRange = 14f;
                d.moveSpeed = Balance.ArcherMoveSpeed;
                d.keepDistance = Balance.ArcherKeepDistance;
                d.fireInterval = Balance.ArcherFireInterval;
                d.projectileSpeed = Balance.ProjectileSpeed;
            });

            set.Boss = UpsertData("Enemy_Boss", d =>
            {
                d.displayName = "무너진 문의 파수꾼";
                d.maxHP = Balance.BossHP;
                d.attack = Balance.BossAttack;
                d.goldDrop = 0;
                d.tint = ColBoss;
                d.detectRange = 30f;
                d.moveSpeed = 3.2f;
            });

            return set;
        }

        static EnemyData UpsertData(string name, System.Action<EnemyData> configure)
        {
            string path = $"{DataDir}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<EnemyData>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            configure(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        // ── 프리팹 ───────────────────────────────────────

        class PrefabSet
        {
            public GameObject Player, Charger, Archer, Boss, Projectile, Chest;
        }

        static PrefabSet CreatePrefabs(Sprite sprite, EnemyDataSet data)
        {
            var set = new PrefabSet();

            set.Projectile = BuildProjectilePrefab(sprite);
            var projectile = set.Projectile.GetComponent<Projectile>();

            set.Player = BuildPlayerPrefab(sprite);
            set.Charger = BuildChargerPrefab(sprite, data.Charger);
            set.Archer = BuildArcherPrefab(sprite, data.Archer, projectile);
            set.Boss = BuildBossPrefab(sprite, data.Boss);
            set.Chest = BuildChestPrefab(sprite);

            return set;
        }

        static GameObject BuildPlayerPrefab(Sprite sprite)
        {
            var root = NewActor("Player", new Vector2(0.9f, 1.8f), ColPlayer, sprite, L(Layers.Player));

            var capsule = root.AddComponent<CapsuleCollider2D>();
            capsule.direction = CapsuleDirection2D.Vertical;
            capsule.size = new Vector2(0.9f, 1.8f);

            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(root.transform, false);
            groundCheck.transform.localPosition = new Vector3(0f, -0.9f, 0f);

            var hitboxGo = new GameObject("Hitbox");
            hitboxGo.transform.SetParent(root.transform, false);
            hitboxGo.layer = L(Layers.PlayerHitbox);
            var hitbox = hitboxGo.AddComponent<Hitbox>();
            hitbox.localOffset = new Vector2(0.95f, 0.1f);
            hitbox.size = new Vector2(1.7f, 1.5f);
            hitbox.targetLayers = 1 << L(Layers.Enemy);

            root.AddComponent<HitFlash>();

            var controller = root.AddComponent<PlayerController>();
            controller.groundCheck = groundCheck.transform;
            controller.groundMask = 1 << L(Layers.Ground);
            controller.groundCheckRadius = 0.16f;

            var combat = root.AddComponent<PlayerCombat>();
            combat.hitbox = hitbox;

            root.AddComponent<PlayerHealth>();

            return SaveAsPrefab(root, "Player");
        }

        static GameObject BuildChargerPrefab(Sprite sprite, EnemyData data)
        {
            var root = NewActor("Enemy_Charger", new Vector2(1.0f, 1.0f), data.tint, sprite, L(Layers.Enemy));

            var box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.0f, 1.0f);

            AddEnemyBody(root);
            var health = root.AddComponent<Health>();
            health.maxHP = data.maxHP;
            health.goldOnDeath = data.goldDrop;
            root.AddComponent<HitFlash>();

            var hitbox = AddEnemyHitbox(root, new Vector2(0.75f, 0f), new Vector2(1.5f, 1.1f));

            var enemy = root.AddComponent<ChargerEnemy>();
            enemy.data = data;
            enemy.hitbox = hitbox;

            return SaveAsPrefab(root, "Enemy_Charger");
        }

        static GameObject BuildArcherPrefab(Sprite sprite, EnemyData data, Projectile projectile)
        {
            var root = NewActor("Enemy_Archer", new Vector2(0.9f, 1.3f), data.tint, sprite, L(Layers.Enemy));

            var box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 1.3f);

            AddEnemyBody(root);
            var health = root.AddComponent<Health>();
            health.maxHP = data.maxHP;
            health.goldOnDeath = data.goldDrop;
            root.AddComponent<HitFlash>();

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(root.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0.35f, 0f);

            var enemy = root.AddComponent<ArcherEnemy>();
            enemy.data = data;
            enemy.projectilePrefab = projectile;
            enemy.muzzle = muzzle.transform;

            return SaveAsPrefab(root, "Enemy_Archer");
        }

        static GameObject BuildBossPrefab(Sprite sprite, EnemyData data)
        {
            var root = NewActor("Boss_Gatekeeper", new Vector2(2.4f, 3.0f), data.tint, sprite, L(Layers.Enemy));

            var box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(2.4f, 3.0f);

            AddEnemyBody(root);

            var health = root.AddComponent<Health>();
            health.maxHP = data.maxHP;
            health.goldOnDeath = 0;
            health.destroyOnDeath = true;
            health.destroyDelay = 1.0f;

            root.AddComponent<HitFlash>();

            var melee = AddEnemyHitbox(root, new Vector2(1.9f, 0f), new Vector2(3.2f, 3.0f));

            // 지면 충격파: 좌우 전체를 덮되 높이가 낮아 점프로만 피할 수 있다.
            var shockwave = AddEnemyHitbox(root, new Vector2(0f, -1.15f), new Vector2(20f, 1.2f));
            shockwave.gameObject.name = "ShockwaveHitbox";

            var boss = root.AddComponent<BossController>();
            boss.data = data;
            boss.hitbox = melee;
            boss.shockwaveHitbox = shockwave;
            boss.approachSpeed = 3.2f;
            boss.chargeSpeed = 10f;

            return SaveAsPrefab(root, "Boss_Gatekeeper");
        }

        static GameObject BuildProjectilePrefab(Sprite sprite)
        {
            var root = NewActor("Projectile", new Vector2(0.32f, 0.32f), ColProjectile, sprite, L(Layers.Projectile));

            var circle = root.AddComponent<CircleCollider2D>();
            circle.radius = 0.16f;
            circle.isTrigger = true;

            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;

            var projectile = root.AddComponent<Projectile>();
            projectile.hitLayers = 1 << L(Layers.Player);
            projectile.blockLayers = 1 << L(Layers.Ground);
            projectile.lifetime = 4f;

            return SaveAsPrefab(root, "Projectile");
        }

        static GameObject BuildChestPrefab(Sprite sprite)
        {
            var root = NewActor("GoldChest", new Vector2(0.8f, 0.6f), ColChest, sprite, L(Layers.Interactable));

            var box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(2.4f, 2.0f);
            box.isTrigger = true;

            var chest = root.AddComponent<ChestZone>();
            chest.gold = Balance.ChestGold;

            return SaveAsPrefab(root, "GoldChest");
        }

        static void AddEnemyBody(GameObject root)
        {
            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = Balance.EnemyGravityScale;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        static Hitbox AddEnemyHitbox(GameObject root, Vector2 offset, Vector2 size)
        {
            var go = new GameObject("Hitbox");
            go.transform.SetParent(root.transform, false);
            go.layer = L(Layers.EnemyHitbox);

            var hitbox = go.AddComponent<Hitbox>();
            hitbox.localOffset = offset;
            hitbox.size = size;
            hitbox.targetLayers = 1 << L(Layers.Player);
            return hitbox;
        }

        static GameObject NewActor(string name, Vector2 size, Color color, Sprite sprite, int layer)
        {
            var root = new GameObject(name);
            root.layer = layer;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = 1;

            return root;
        }

        static GameObject SaveAsPrefab(GameObject instance, string name)
        {
            string path = $"{PrefabDir}/{name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        // ── 씬 조립 공통 ─────────────────────────────────

        static void NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        static void SaveScene(string name)
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), $"{SceneDir}/{name}.unity");
        }

        static Camera MakeCamera(bool follow, float orthoSize = 6.5f)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 3f, -10f);

            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = orthoSize;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.062f, 0.09f);
            camera.nearClipPlane = -20f;

            if (follow) go.AddComponent<CameraFollow>();
            return camera;
        }

        static GameObject MakeGround(string name, float left, float right, float top, Sprite sprite, Color color)
        {
            float width = right - left;
            const float thickness = 2f;

            var go = NewActor(name, new Vector2(width, thickness), color, sprite, L(Layers.Ground));
            go.transform.position = new Vector3(left + width * 0.5f, top - thickness * 0.5f, 0f);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(width, thickness);

            var visual = go.transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (visual != null) visual.sortingOrder = 0;

            return go;
        }

        static GameObject MakeWall(string name, float x, float bottom, float height, Sprite sprite)
        {
            var go = NewActor(name, new Vector2(1f, height), ColGround, sprite, L(Layers.Ground));
            go.transform.position = new Vector3(x, bottom + height * 0.5f, 0f);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, height);
            return go;
        }

        static T MakeBuilding<T>(string name, float x, Vector2 size, Color color, Sprite sprite,
            float triggerPadX = 2.0f) where T : Component
        {
            var go = NewActor(name, size, color, sprite, L(Layers.Interactable));
            go.transform.position = new Vector3(x, GroundTop + size.y * 0.5f, 0f);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(size.x + triggerPadX, size.y);
            box.isTrigger = true;

            return go.AddComponent<T>();
        }

        static void MakeWorldLabel(string text, Vector3 position, float size = 0.14f)
        {
            var go = new GameObject("Label_" + text);
            go.transform.position = position;

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = size;
            mesh.fontSize = 72;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(0.92f, 0.94f, 0.98f);
            mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null && mesh.font != null) renderer.sharedMaterial = mesh.font.material;
        }

        static FallZone MakeFallZone(string name, float centerX, float width, float centerY, Vector3 respawn)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(centerX, centerY, 0f);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(width, 2f);
            box.isTrigger = true;

            var marker = new GameObject("RespawnPoint");
            marker.transform.SetParent(go.transform, false);
            marker.transform.position = respawn;

            var zone = go.AddComponent<FallZone>();
            zone.respawnPoint = marker.transform;
            zone.damage = Balance.FallDamage;
            return zone;
        }

        static GameObject MakeUI(bool withTownPanels, bool withFlow)
        {
            var go = new GameObject("[UI]");
            go.AddComponent<UIRoot>();
            go.AddComponent<InteractPrompt>();

            if (withTownPanels)
            {
                go.AddComponent<ShopPanel>();
                go.AddComponent<InnPanel>();
            }

            go.AddComponent<HudView>();
            if (withFlow) go.AddComponent<GameFlowUI>();

            return go;
        }

        static GameObject SpawnPrefab(GameObject prefab, Vector3 position)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
            return instance;
        }

        static void SpawnPlayer(PrefabSet prefabs, float x, Camera camera)
        {
            var player = SpawnPrefab(prefabs.Player, new Vector3(x, PlayerSpawnY, 0f));

            var follow = camera != null ? camera.GetComponent<CameraFollow>() : null;
            if (follow != null) follow.target = player.transform;

            camera.transform.position = new Vector3(x, PlayerSpawnY + 1.5f, -10f);
        }

        // ── 씬: Boot ─────────────────────────────────────

        static void BuildBootScene()
        {
            NewScene();
            MakeCamera(false);

            var ui = new GameObject("[UI]");
            ui.AddComponent<UIRoot>();
            ui.AddComponent<TitleScreen>();

            SaveScene(SceneNames.Boot);
        }

        // ── 씬: Ending ───────────────────────────────────

        static void BuildEndingScene()
        {
            NewScene();
            MakeCamera(false);

            var ui = new GameObject("[UI]");
            ui.AddComponent<UIRoot>();
            ui.AddComponent<EndingScreen>();

            SaveScene(SceneNames.Ending);
        }

        // ── 씬: Town ─────────────────────────────────────

        static void BuildTownScene(Sprite sprite, PrefabSet prefabs)
        {
            NewScene();
            var camera = MakeCamera(true);

            MakeGround("Ground", -22f, 34f, GroundTop, sprite, ColGround);
            MakeWall("Wall_L", -22.5f, GroundTop, 10f, sprite);
            MakeWall("Wall_R", 34.5f, GroundTop, 10f, sprite);

            MakeBuilding<ShopZone>("Shop", -8f, new Vector2(4f, 4f), ColShop, sprite);
            MakeWorldLabel("상점", new Vector3(-8f, 4.8f, 0f));

            MakeBuilding<InnZone>("Inn", 0f, new Vector2(4f, 3.6f), ColInn, sprite);
            MakeWorldLabel("여관", new Vector3(0f, 4.4f, 0f));

            var routeA = MakeBuilding<TravelZone>("Gate_RouteA", 10f, new Vector2(2.6f, 4f), ColGate, sprite);
            routeA.targetScene = SceneNames.Route01;
            routeA.displayName = "버려진 갱도";
            routeA.subtitle = "짧음 · 돌진형";
            MakeWorldLabel("루트 A\n버려진 갱도", new Vector3(10f, 5.4f, 0f), 0.11f);

            var routeB = MakeBuilding<TravelZone>("Gate_RouteB", 18f, new Vector2(2.6f, 4f), ColGate, sprite);
            routeB.targetScene = SceneNames.Route02;
            routeB.displayName = "안개 다리";
            routeB.subtitle = "길다 · 사격형 · 낙하 구간";
            MakeWorldLabel("루트 B\n안개 다리", new Vector3(18f, 5.4f, 0f), 0.11f);

            var boss = MakeBuilding<TravelZone>("Gate_Boss", 28f, new Vector2(3.2f, 5.4f), ColBossGate, sprite);
            boss.targetScene = SceneNames.BossArena;
            boss.displayName = "무너진 문";
            boss.subtitle = "보스";
            MakeWorldLabel("무너진 문\n보스", new Vector3(28f, 6.6f, 0f), 0.12f);

            MakeWorldLabel("A / D 이동 · Space 점프 · Shift 대시 · E 상호작용",
                new Vector3(-16f, 3.2f, 0f), 0.1f);

            MakeUI(true, true);
            SpawnPlayer(prefabs, -16f, camera);

            SaveScene(SceneNames.Town);
        }

        // ── 씬: Route01 (버려진 갱도) ────────────────────

        static void BuildRoute01(Sprite sprite, PrefabSet prefabs)
        {
            NewScene();
            var camera = MakeCamera(true);

            // 지면 3단 + 점프 구간 2개
            MakeGround("Ground_1", -16f, 6f, 0f, sprite, ColGround);
            MakeGround("Platform_2", 8.5f, 16.5f, 1f, sprite, ColPlatform);
            MakeGround("Platform_3", 19f, 27f, 2f, sprite, ColPlatform);
            MakeGround("Ground_4", 29f, 42f, 0f, sprite, ColGround);

            MakeWall("Wall_L", -16.5f, 0f, 10f, sprite);
            MakeWall("Wall_R", 42.5f, 0f, 10f, sprite);

            // 낙하 구간: 각 틈 바로 아래에서 직전 발판으로 되돌린다.
            MakeFallZone("Fall_Gap1", 7.25f, 3.0f, -3f, new Vector3(4.5f, 1.0f, 0f));
            MakeFallZone("Fall_Gap2", 17.75f, 3.0f, -3f, new Vector3(14.5f, 2.0f, 0f));
            MakeFallZone("Fall_Gap3", 28f, 3.0f, -3f, new Vector3(25f, 3.0f, 0f));
            MakeFallZone("Fall_Catch", 12f, 120f, -12f, new Vector3(-14f, PlayerSpawnY, 0f));

            SpawnPrefab(prefabs.Charger, new Vector3(-2f, 0.6f, 0f));
            SpawnPrefab(prefabs.Charger, new Vector3(3f, 0.6f, 0f));
            SpawnPrefab(prefabs.Charger, new Vector3(12f, 1.6f, 0f));
            SpawnPrefab(prefabs.Charger, new Vector3(23f, 2.6f, 0f));

            SpawnPrefab(prefabs.Chest, new Vector3(25.5f, 2.4f, 0f));
            MakeWorldLabel("버려진 갱도", new Vector3(-13f, 4f, 0f), 0.13f);

            var exit = MakeBuilding<LevelExit>("Exit", 39f, new Vector2(2.6f, 4f), ColGate, sprite);
            exit.clearBonus = Balance.RouteAClearBonus;
            MakeWorldLabel("귀환", new Vector3(39f, 4.8f, 0f));

            MakeUI(false, true);
            SpawnPlayer(prefabs, -14f, camera);

            SaveScene(SceneNames.Route01);
        }

        // ── 씬: Route02 (안개 다리) ──────────────────────

        static void BuildRoute02(Sprite sprite, PrefabSet prefabs)
        {
            NewScene();
            var camera = MakeCamera(true);

            MakeGround("Ground_1", -18f, 2f, 0f, sprite, ColGround);
            MakeGround("Bridge_Pillar", 3.5f, 5.5f, 1f, sprite, ColPlatform);
            MakeGround("Ground_2", 7f, 23f, 0f, sprite, ColGround);
            MakeGround("Ground_3", 25f, 44f, 0f, sprite, ColGround);

            MakeWall("Wall_L", -18.5f, 0f, 10f, sprite);
            MakeWall("Wall_R", 44.5f, 0f, 10f, sprite);

            MakeFallZone("Fall_Bridge_A", 2.75f, 2.0f, -3f, new Vector3(0f, PlayerSpawnY, 0f));
            MakeFallZone("Fall_Bridge_B", 6.25f, 2.0f, -3f, new Vector3(4.5f, 2.0f, 0f));
            MakeFallZone("Fall_Gap2", 24f, 2.0f, -3f, new Vector3(21.5f, PlayerSpawnY, 0f));
            MakeFallZone("Fall_Catch", 12f, 140f, -12f, new Vector3(-16f, PlayerSpawnY, 0f));

            SpawnPrefab(prefabs.Charger, new Vector3(-8f, 0.6f, 0f));
            SpawnPrefab(prefabs.Charger, new Vector3(11f, 0.6f, 0f));
            SpawnPrefab(prefabs.Charger, new Vector3(30f, 0.6f, 0f));

            SpawnPrefab(prefabs.Archer, new Vector3(17f, 0.8f, 0f));
            SpawnPrefab(prefabs.Archer, new Vector3(22f, 0.8f, 0f));
            SpawnPrefab(prefabs.Archer, new Vector3(36f, 0.8f, 0f));

            SpawnPrefab(prefabs.Chest, new Vector3(39f, 0.4f, 0f));
            MakeWorldLabel("안개 다리", new Vector3(-15f, 4f, 0f), 0.13f);
            MakeWorldLabel("사격형은 거리를 벌린다 — 붙어라", new Vector3(14f, 4f, 0f), 0.1f);

            var exit = MakeBuilding<LevelExit>("Exit", 42f, new Vector2(2.6f, 4f), ColGate, sprite);
            exit.clearBonus = Balance.RouteBClearBonus;
            MakeWorldLabel("귀환", new Vector3(42f, 4.8f, 0f));

            MakeUI(false, true);
            SpawnPlayer(prefabs, -16f, camera);

            SaveScene(SceneNames.Route02);
        }

        // ── 씬: BossArena ────────────────────────────────

        static void BuildBossArena(Sprite sprite, PrefabSet prefabs)
        {
            NewScene();
            var camera = MakeCamera(true, 7.5f);

            MakeGround("Arena_Ground", -20f, 20f, 0f, sprite, ColGround);
            MakeWall("Wall_L", -20.5f, 0f, 14f, sprite);
            MakeWall("Wall_R", 20.5f, 0f, 14f, sprite);

            MakeFallZone("Fall_Catch", 0f, 120f, -12f, new Vector3(-14f, PlayerSpawnY, 0f));

            SpawnPrefab(prefabs.Boss, new Vector3(8f, 1.6f, 0f));
            MakeWorldLabel("무너진 문의 파수꾼", new Vector3(0f, 6.5f, 0f), 0.14f);
            MakeWorldLabel("지면 충격파는 점프로만 피할 수 있다", new Vector3(0f, 5.4f, 0f), 0.1f);

            var back = MakeBuilding<TravelZone>("Gate_Town", -17f, new Vector2(2.6f, 4f), ColGate, sprite);
            back.targetScene = SceneNames.Town;
            back.displayName = "마을로 돌아가기";
            back.subtitle = "";

            MakeUI(false, true);
            SpawnPlayer(prefabs, -13f, camera);

            SaveScene(SceneNames.BossArena);
        }

        // ── 빌드 설정 · 플레이어 설정 ────────────────────

        static void RegisterScenes()
        {
            var order = new[]
            {
                SceneNames.Boot, SceneNames.Town, SceneNames.Route01,
                SceneNames.Route02, SceneNames.BossArena, SceneNames.Ending
            };

            var list = new List<EditorBuildSettingsScene>();
            foreach (var name in order)
            {
                string path = $"{SceneDir}/{name}.unity";
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
                list.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = list.ToArray();
        }

        static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "SideScrollRPG";
            PlayerSettings.productName = "무너진 문";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
        }
    }
}
