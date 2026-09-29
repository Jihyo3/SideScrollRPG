# 다음 세션 인계 문서

> 마지막 갱신: 2026-09-15
> 이 문서는 "지금 어디까지 됐고, 다음에 무엇을 하면 되는지"만 담는다.
> 설계 의도와 수치의 근거는 `GAME_PLAN.md`에 있다.

---

## 30초 요약

Unity 2D 횡스크롤 액션 RPG 프로토타입. **계획서 1~6일차 코드가 전부 들어가 있고, 7일차(밸런싱·아트·사운드)가 남았다.**

- 프로젝트: `C:\Users\COMSW\SideScrollRPG`
- Unity **6000.6.0f1** (Built-in + 2D, 추가 패키지 없음)
- 스크립트 43개 / 씬 6개 / 프리팹 6개 / `EnemyData` 3개
- 컴파일 0에러 · 씬 배선 검증 통과 · Windows 빌드 성공(92MB)
- **아직 실제 플레이 테스트를 하지 않았다** ← 다음 세션의 1순위

## 바로 실행하기

```
Unity Hub → Projects → Add → Add project from disk
→ C:\Users\COMSW\SideScrollRPG
→ Assets/_Project/Scenes/Boot.unity 열고 Play
```

빌드 결과물: `Build/Windows/무너진문.exe`

조작: `A`/`D` 이동 · `Space` 점프 · `Shift` 대시 · 좌클릭(`J`) 약공격 3타 · 우클릭(`K`) 강공격 · `R` 물약 · `E` 상호작용 · `ESC` 창 닫기

## 한 바퀴 동선

```
Boot(타이틀) → Town(상점·여관·게이트 3개) → Route01 또는 Route02
   → 클리어 후 Town 복귀 → 장비 구매 → BossArena → Ending
```

루트는 반복 입장 가능. 장비 2종 합 110 G, 루트 1회 보상 45~71 G → 2~3회 돌아야 다 산다.

---

## 다음에 할 일 (우선순위 순)

### 1. 플레이 테스트 — 타격감 검증 (가장 중요)

`Boot` 씬에서 Play를 누르고 아래를 손으로 확인한다. 어긋나면 **`Assets/_Project/Scripts/Core/Balance.cs`만 고친다.**

- [ ] 이동·점프가 답답하지 않은가 → `MoveSpeed`, `Acceleration`, `JumpHeight`, `FallGravityMult`
- [ ] 대시로 돌진형을 통과할 수 있는가 → `DashInvincible`(0.14s), `DashDuration`
- [ ] 3타 콤보 입력이 씹히지 않는가 → `ComboInputBuffer`, `LightWindup`, `LightRecovery`
- [ ] 때리는 느낌이 있는가 → `HitStop`(0.08s), `ShakeLight`/`ShakeHeavy`, `LightKnockback`
- [ ] 공격 중 이동이 막히는 게 답답한 수준인가 (의도된 제약이지만 과하면 후딜을 줄인다)

### 2. 보스 밸런싱

목표: **TTK 40~60초, 첫 클리어까지 사망 2~5회**

- 너무 쉬우면 `Balance.BossHP`(300) / `BossAttack`(22)를 올린다
- 패턴이 안 읽히면 `BossController.cs`의 `Telegraph(0.6f)` 선딜을 늘린다
- 딜 넣을 틈이 없으면 `Balance.BossIdleBetweenPatterns`(0.8s)를 늘린다

### 3. 사운드 붙이기 (코드 수정 불필요)

`Assets/_Project/Resources/Audio/` 에 아래 이름으로 넣으면 `Sfx.cs`가 자동으로 잡는다.

```
hit_1.wav  hit_2.wav  hit_3.wav     (타격, 3종 랜덤 + 피치 ±5%)
hurt_1.wav hurt_2.wav              (피격)
jump.wav   coin.wav   ui.wav
```

무료 출처: Kenney, Freesound. 라이선스는 `ThirdParty/LICENSES.md`에 기록한다.

### 4. 아트 적용

현재는 전부 색 사각형 그레이박스다. **하나의 무료 팩 안에서만 골라 쓴다** (여러 팩을 섞으면 픽셀 밀도가 어긋나 더 조잡해진다).

교체 지점: 각 프리팹의 `Visual` 자식에 붙은 `SpriteRenderer`. 루트 오브젝트는 스케일 1을 유지하고 `Visual`만 크기를 조절하는 구조이므로, 스프라이트를 갈아 끼울 때 콜라이더를 다시 맞출 필요가 없다.

### 5. 버전 관리

`git`이 아직 설치되어 있지 않다. `.gitignore`는 준비되어 있다.

```powershell
winget install --id Git.Git -e
# 설치 후
git init; git add .; git commit -m "프로토타입 초기 구현"
```

---

## 주의사항

**`SideScrollRPG > 프로젝트 셋업 실행` 메뉴는 씬 6개를 전부 덮어쓴다.**
에디터에서 적 위치나 지형을 손으로 고친 뒤에 누르면 그 작업이 사라진다.

- 씬을 손으로 편집하기 시작했다면 이 메뉴를 쓰지 않는다
- 수치만 조정할 거면 `Balance.cs`를 고친다 (씬과 무관하게 적용된다)
- 배치를 바꾸고 그걸 유지하고 싶으면, 씬을 직접 고치는 대신 `Editor/ProjectSetup.cs`의 배치 좌표를 고치고 셋업을 다시 돌린다 ← 이 방식을 권장

안전한 메뉴: `셋업 검증`, `Windows 빌드` (아무것도 덮어쓰지 않는다)

## 코드 지도

| 위치 | 내용 |
|---|---|
| `Scripts/Core/Balance.cs` | **모든 수치.** 밸런싱은 원칙적으로 여기만 고친다 |
| `Scripts/Core/` | `GameManager`(골드·HP·장비), `InputReader`, `ModalState`, `HitStop`, `Sfx` |
| `Scripts/Player/` | `PlayerController`(이동·점프·대시), `PlayerCombat`(콤보), `PlayerHealth` |
| `Scripts/Combat/` | `Hitbox`(OverlapBox 1회 판정), `Health`, `HitFlash`, `Projectile`, `DamagePopup` |
| `Scripts/Enemy/` | `EnemyBase`, `ChargerEnemy`, `ArcherEnemy`, `BossController`(2페이즈) |
| `Scripts/World/` | `CameraFollow`, `InteractZone` 계열(상점·여관·게이트·상자·출구), `FallZone` |
| `Scripts/UI/` | `UIFactory`(코드 UI 조립), `HudView`, `ShopPanel`, `InnPanel`, `GameFlowUI` |
| `Editor/` | `ProjectSetup`(생성), `ProjectAudit`(검증), `BuildScript`(빌드) |

## 알려진 미검증 항목

플레이 모드를 배치로 돌릴 수 없어 아래는 코드만 들어가 있고 **실제 동작은 확인되지 않았다.**

- 타격감 6개 항목 전체의 체감
- 보스 2페이즈 전환과 지면 충격파 회피 난이도
- 사격형 적의 거리 유지 AI가 지형에서 끼지 않는지
- 낙하 존 복귀 위치가 어색하지 않은지
- 상점·여관 UI가 `Time.timeScale = 0` 상태에서 의도대로 동작하는지
