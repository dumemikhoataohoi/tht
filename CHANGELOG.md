# Changelog

Định dạng dựa trên [Keep a Changelog](https://keepachangelog.com/), versioning theo
milestone cho tới khi có bản release chính thức (sau đó chuyển sang SemVer thật).

## [Unreleased]

### M1.5 — Unity integration verification

#### Added
- `GameManagerBehaviour` chuyển thành singleton (`Instance`) + `DontDestroyOnLoad`,
  sống trên scene Boot và tồn tại xuyên suốt khi chuyển sang scene Gameplay.
- `Assets/_Project/Scripts/Bootstrap/BootLoader.cs` + `GemGrid.Bootstrap.asmdef`:
  chuyển từ Boot sang Gameplay scene sau khi khởi tạo xong.
- `Core/IPointerInputSource` + `Gameplay/View/UnityPointerInputSource`: tách
  `BlockDragController` khỏi việc gọi `UnityEngine.Input` trực tiếp; xác nhận Input
  Manager cổ điển của Unity đã hỗ trợ sẵn touch thật trên Android, không cần thêm
  package Input System mới.
- `BlockShapeSet.EditorSetShapes(...)` (guard `#if UNITY_EDITOR`): hook cho Editor
  tooling điền sẵn shape khởi điểm, tránh asset rỗng làm `BlockShapeLibrary` throw.
- `Assets/_Project/Editor/` (`GemGrid.EditorTools.asmdef`): `GemGridAssetSetup.cs`
  (menu tạo `BlockShapeSet.asset`/`GameplayConfig.asset`), `GemGridSceneSetup.cs`
  (menu tạo `Boot.unity`/`Gameplay.unity` bằng API Unity — không hand-write YAML).
- `Assets/Tests/PlayMode/` (`GemGrid.Tests.PlayMode.asmdef`, `BootFlowTests.cs`): 2
  PlayMode test tối thiểu cho luồng Boot→Gameplay và Restart.
- Mở rộng `tools/dotnet-tests/GemGrid.UnitySyntaxCheck/` để syntax-check cả Editor
  tooling và PlayMode test (thêm `UnityEditorAndTestShim.cs`, `DefineConstants=
  UNITY_EDITOR`).
- `UNITY_SETUP.md`: hướng dẫn đầy đủ mở project, tạo asset/scene, Play, chạy Test
  Runner, build Android debug.
- Bỏ `com.unity.2d.tilemap` khỏi `Packages/manifest.json` — package không được dùng
  bởi bất kỳ script nào (grid dựng bằng GameObject/SpriteRenderer thủ công, không
  dùng Tilemap).

#### Changed
- `GridController`, `BlockDragController`, `HapticHookListener`,
  `GameplayAnimationHooks`, `AudioHookListener`: bỏ
  `[RequireComponent(typeof(GameManagerBehaviour))]` + `GetComponent`, đọc
  `GameManagerBehaviour.Instance` thay thế (không còn cùng GameObject/scene).

#### Known issues / chưa verify
- Toàn bộ scene/asset do Editor tooling tạo ra, và cả 2 PlayMode test, **chưa từng
  chạy thật** trong Unity Editor (không có sẵn trong sandbox) — chỉ mới syntax-check
  bằng shim tự viết. Xem `README_M1.md` §4 và `UNITY_SETUP.md`.

### M1 — Core gameplay

#### Added
- Grid 8x8 (`IGridModel`/`GridModel`): đặt block, kiểm tra hợp lệ, clear hàng/cột
  (đơn lẻ, nhiều dòng, đồng thời hàng+cột), reset — 100% pure C#, không phụ thuộc
  UnityEngine.
- Hệ thống shape data-driven: `BlockShapeDefinition`, `BlockShapeLibrary` (random có
  trọng số), `BlockShapeSet` (ScriptableObject), `IBlockShapeProvider`.
- `BlockSpawner` (tray 3 slot, refill đồng loạt khi hết), `BlockData`, `BlockPlacement`.
- `ScoreManager` + `ScoreRules`, `ComboManager` + `ComboRules` (data-driven, không
  hard-code số liệu trong logic).
- `GameOverChecker` (brute-force mọi shape × mọi ô), `GameManager` (orchestrator:
  start/restart, TryPlaceBlock, chuyển state, phát event).
- Lớp adapter Unity (chưa verify runtime — xem README_M1.md): `GameManagerBehaviour`
  (composition root scene), `GridController` (world↔grid, render placeholder),
  `BlockDragController` (kéo/thả 1 ngón tay), `HapticHookListener`,
  `GameplayAnimationHooks` (UnityEvent hooks), `AudioHookListener` + `IAudioService`/
  `NullAudioService`, `IHapticService`/`NullHapticService`.
- Assembly definitions: `GemGrid.Core`, `GemGrid.Configuration`, `GemGrid.Gameplay`,
  `GemGrid.Audio`, `GemGrid.Tests.EditMode`.
- 44 unit test EditMode (`Assets/Tests/EditMode/`) bao phủ toàn bộ acceptance
  criteria M1 (grid init, empty grid, valid/invalid placement, ngoài biên, occupied
  cell, single/multi row/column clear, đồng thời row+column, score, combo,
  game-over, restart/reset) + spawner.
- `tools/dotnet-tests/GemGrid.Logic.Tests/`: project .NET độc lập link trực tiếp tới
  code + test thật để **chạy được unit test thật** trong sandbox không có Unity
  Editor. Kết quả: 44/44 pass, 0 warning.
- `README_M1.md`: hướng dẫn mở Unity Editor, chạy test, và ghi rõ phần đã/chưa verify.

#### Known issues / chưa verify
- Chưa build/compile được bằng Unity Editor thật (không có sẵn trong sandbox).
- Chưa có asset `BlockShapeSet.asset`/`GameplayConfig.asset` cụ thể (chỉ có class) —
  xem README_M1.md §5 để tạo khi mở bằng Unity.
- MonoBehaviour lifecycle, input thật, render — chưa chạy Play Mode thật.

### M0 — Project setup (đang thực hiện)

#### Added
- `PROJECT_PLAN.md`, `ARCHITECTURE.md`, `GAME_DESIGN.md`, `ECONOMY_DESIGN.md`,
  `MONETIZATION.md`, `QA_PLAN.md`, `RELEASE_PLAN.md`, `LICENSE_MANIFEST.md`,
  `CHANGELOG.md` — bộ tài liệu thiết kế/kế hoạch ban đầu.
- Cấu trúc thư mục Unity project skeleton (`Assets/_Project/...`, `Assets/Tests/...`,
  `Packages/manifest.json`, `.gitignore`, `README.md`).

#### Notes
- Chưa viết code gameplay — theo đúng yêu cầu chờ Product Owner xác nhận trước khi
  bắt đầu M1.
- Môi trường phát triển hiện tại không có Unity Editor cài sẵn — ghi nhận là blocker
  ảnh hưởng tới việc build/test thực tế (xem `QA_PLAN.md` §1 và `PROJECT_PLAN.md` §3).
