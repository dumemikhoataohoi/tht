# M1 — Core Gameplay: hướng dẫn mở & kiểm tra

M1 triển khai toàn bộ logic gameplay lõi của GemGrid (grid 8x8, block shape
data-driven, spawner, kéo/thả, đặt block, clear hàng/cột, score, combo, game-over,
restart) theo `GAME_DESIGN.md` và `ARCHITECTURE.md`. Tài liệu này giải thích **chính
xác** những gì đã được xác minh, những gì chưa, và cách tự kiểm tra khi có Unity Editor.

> **Cập nhật M1.5**: đã thêm Boot/Gameplay scene (tạo qua Editor tooling, không hand
> YAML), Editor tooling tạo data asset, input abstraction (`IPointerInputSource`), và
> PlayMode test. Hướng dẫn mở/Play/build chi tiết từng bước nằm ở
> **`UNITY_SETUP.md`** — tài liệu này chỉ còn tập trung vào "cái gì đã verify thật,
> cái gì chưa".

## 1. Vì sao có hai cách "chạy test"

Sandbox phát triển hiện tại **không có Unity Editor**. Toàn bộ logic gameplay thuần
(`GridModel`, `ScoreManager`, `ComboManager`, `BlockSpawner`, `GameOverChecker`,
`GameManager`, `BlockPlacement`, các kiểu dữ liệu trong `Configuration`) được viết
**không phụ thuộc UnityEngine**, nên có thể biên dịch và chạy bằng .NET SDK thuần —
đây là cách tôi đã **thực sự build và chạy** 44 unit test trong milestone này, không
phải suy đoán.

Project phụ `tools/dotnet-tests/GemGrid.Logic.Tests/` **không phải một phần của
Unity project** — nó chỉ là một .csproj độc lập **link trực tiếp** (không copy) tới
đúng các file `.cs` sản xuất nằm trong `Assets/_Project/Scripts/{Core,Configuration,
Gameplay}` và bộ test trong `Assets/Tests/EditMode/`. Nhờ vậy:
- Không có bản sao code nào — một nguồn sự thật duy nhất.
- Cùng những file test này (`Assets/Tests/EditMode/*.cs`) chỉ dùng `NUnit.Framework`
  thuần (không dùng `UnityEngine.TestTools`), nên **chạy y hệt** khi mở bằng Unity
  Test Runner (EditMode) lẫn khi chạy `dotnet test` ngoài Unity.

Các file dùng `UnityEngine` (MonoBehaviour: `GameManagerBehaviour`, `GridController`,
`BlockDragController`, `HapticHookListener`, `GameplayAnimationHooks`,
`AudioHookListener`; ScriptableObject: `BlockShapeSet`, `GameplayConfig`) **không**
được link vào project .NET này vì không thể biên dịch nếu thiếu UnityEngine — những
file này **chưa được build/chạy thực tế**, xem mục 4.

## 2. Chạy test đã verify được (không cần Unity)

```bash
cd tools/dotnet-tests/GemGrid.Logic.Tests
dotnet test
```

Kết quả thực tế tại thời điểm hoàn thành M1: **44/44 test pass**, build 0 warning
(`dotnet build -warnaserror`). Đây là bằng chứng thật, không phải giả định.

## 3. Mở project, tạo asset/scene, chạy test trong Unity Editor

Xem **`UNITY_SETUP.md`** cho quy trình đầy đủ từng bước (Unity version, mở project,
tạo data asset + scene qua menu `GemGrid ▸ Setup ▸ ...`, chạy Play, chạy Test Runner,
build Android debug). Tóm tắt nhanh:

1. Mở project bằng Unity Hub (bản LTS mới nhất — xem UNITY_SETUP.md §1).
2. `GemGrid ▸ Setup ▸ 0. Create All Required Data Assets` (tạo `BlockShapeSet.asset` +
   `GameplayConfig.asset` — không hand-edit YAML, xem mục 5 bên dưới).
3. `GemGrid ▸ Setup ▸ 5. Create Boot And Gameplay Scenes`, rồi thêm cả hai vào
   Build Settings ▸ Scenes In Build (Boot trước).
4. Mở `Boot.unity`, bấm Play.
5. **Window ▸ General ▸ Test Runner**: tab EditMode ▸ Run All (kỳ vọng 44/44 pass,
   cùng bộ test đã pass thật bằng `dotnet test` ở mục 2); tab PlayMode ▸ Run All (2
   test `BootFlowTests`).

Toàn bộ quy trình trên **chưa từng được chạy thật** trong môi trường viết code này
(không có Unity Editor) — xem mục 4.

## 4. Việc gì đã verify, việc gì chưa

### Đã verify thật (build + chạy thành công bằng `dotnet test`)
- `GridModel`/`IGridModel`: khởi tạo, ô trống, đặt hợp lệ/không hợp lệ, đặt ngoài
  biên, phát hiện ô đã chiếm, clear 1/nhiều hàng, clear 1/nhiều cột, clear đồng thời
  hàng+cột, reset.
- `BlockShapeDefinition`, `BlockShapeLibrary` (random có trọng số).
- `BlockSpawner`: refill tray, tray rỗng/không rỗng, consume slot, trọng số spawn.
- `ScoreManager`: điểm đặt block, điểm clear (đơn/nhiều dòng), combo multiplier, reset.
- `ComboManager`: tăng/giảm combo, best combo, multiplier tăng dần và bị clamp, reset.
- `GameOverChecker`: còn nước đi / hết nước đi, bỏ qua slot rỗng.
- `GameManager` (orchestrator): start/restart, đặt block hợp lệ/không hợp lệ, tự
  refill tray khi hết, chuyển trạng thái GameOver đúng lúc, từ chối thao tác sau
  GameOver.
- Build sạch, 0 warning với `dotnet build -warnaserror`.

### Syntax-check bổ sung cho phần dùng UnityEngine (không thay thế Unity Editor thật)

`tools/dotnet-tests/GemGrid.UnitySyntaxCheck/` biên dịch **toàn bộ** file production
dùng UnityEngine/UnityEditor — MonoBehaviour (`GameManagerBehaviour`, `GridController`,
`BlockDragController`, `HapticHookListener`, `GameplayAnimationHooks`,
`AudioHookListener`, `BootLoader`), ScriptableObject (`BlockShapeSet`,
`GameplayConfig`), **Editor tooling** (`GemGridAssetSetup`, `GemGridSceneSetup` —
thêm ở M1.5), và **PlayMode test** (`BootFlowTests` — thêm ở M1.5) — trước một shim
`UnityEngine`/`UnityEditor`/`UnityEngine.TestTools` tự viết tối thiểu
(`UnityEngineShim.cs` + `UnityEditorAndTestShim.cs`; chỉ định nghĩa lại đúng các
kiểu/API thật sự được dùng: `MonoBehaviour`, `ScriptableObject`, `SerializeField`,
`Vector3`, `Color`, `SpriteRenderer`, `Camera`, `Input`, `Mathf`, `UnityEvent`,
`MenuItem`, `AssetDatabase`, `SerializedObject`, `EditorSceneManager`, `SceneManager`,
`UnityTest`, `UnityTearDown`...).

```bash
cd tools/dotnet-tests/GemGrid.UnitySyntaxCheck
dotnet build
```

Kết quả thật: **build thành công, 0 lỗi** (sau khi sửa một sai sót trong chính shim —
xem bên dưới). Việc này **có giá trị thật**: nó từng bắt được một lỗi biên dịch thử
nghiệm khi tôi thiếu toán tử ép kiểu ngầm định `Vector2 → Vector3` trong shim (Unity
thật có toán tử này, `BlockDragController.cs` dùng đúng theo API thật, nên sau khi bổ
sung shim cho khớp thì build sạch — không phải sửa code sản xuất).

**Giới hạn quan trọng — đây KHÔNG phải Unity Editor thật**: shim chỉ mô phỏng đúng
chữ ký các API mà tôi *nhớ* là đúng với UnityEngine thật; nó không kiểm tra được
hành vi runtime, thứ tự vòng đời MonoBehaviour, GameObject/Component thật, render,
input thật, hay bất kỳ API nào tôi dùng sai mà tưởng là đúng. Nó chỉ tăng độ tin cậy
về mặt cú pháp/tên thành viên, không thay thế được việc mở bằng Unity Editor thật.

### Chưa thể verify (cần Unity Editor — không có trong sandbox này)
- Project có **compile được trong Unity thật** hay không (asmdef, GUID reference,
  Editor-specific behaviour...) — chỉ mới syntax-check bằng shim tự viết ở trên, chưa
  có trình biên dịch Unity thật để xác nhận.
- `GameManagerBehaviour`, `GridController`, `BlockDragController`,
  `HapticHookListener`, `GameplayAnimationHooks`, `AudioHookListener`, `BootLoader`:
  thứ tự MonoBehaviour lifecycle thật (Awake/Start/`DontDestroyOnLoad` khi chuyển
  scene), input thật (chuột/chạm), render sprite.
- `GemGridAssetSetup`/`GemGridSceneSetup` (Editor tooling M1.5): chỉ syntax-check
  được, **chưa từng thực sự chạy** — không có gì đảm bảo `AssetDatabase.CreateAsset`,
  `EditorSceneManager.SaveScene`, `SerializedObject.FindProperty(...)` (đúng tên field
  private `blockShapeSet`/`gameplayConfig`/`gridController`) hoạt động đúng như kỳ
  vọng cho tới khi ai đó chạy menu `GemGrid ▸ Setup` thật trong Unity.
- Test Runner EditMode chạy **bên trong Unity** (rất nhiều khả năng pass vì dùng
  đúng logic/test đã pass ở `dotnet test`, nhưng chưa ai chạy thật).
- 2 PlayMode test (`BootFlowTests`) — **hoàn toàn chưa chạy lần nào**, kể cả không
  qua `dotnet test` (không thể, vì phụ thuộc scene loading thật). Rủi ro lớn nhất là
  số frame `yield return null` giả định để chờ Boot→Gameplay chuyển scene có thể
  không đúng thực tế.
- Hiệu năng, cảm giác chơi thật, UX kéo-thả trên thiết bị/emulator.
- Nội dung thật của `BlockShapeSet.asset`/`GameplayConfig.asset` sau khi Editor
  tooling tạo ra — class + tool đã có, nhưng chưa ai chạy tool đó trong Unity thật để
  xác nhận asset sinh ra đúng như code mong đợi (xem mục 5).

## 5. Vì sao không hand-write `.unity`/`.asset` YAML

M1.5 cố tình **không** tự viết tay file scene (`.unity`) hay asset ScriptableObject
(`.asset`) — định dạng YAML của Unity phụ thuộc GUID (script `.meta`, cross-reference
giữa asset) mà không có Unity Editor để sinh/kiểm chứng thì rủi ro tạo ra file hỏng
(project không mở được scene, hoặc worse) là có thật và tôi không thể xác minh trước.
Thay vào đó, `Assets/_Project/Editor/GemGridAssetSetup.cs` và `GemGridSceneSetup.cs`
tạo asset/scene **bằng chính API của Unity** (`ScriptableObject.CreateInstance`,
`AssetDatabase.CreateAsset`, `EditorSceneManager.NewScene/SaveScene`) khi một người
chạy menu `GemGrid ▸ Setup ▸ ...` trong Unity Editor thật — Unity tự lo phần
serialization đúng định dạng, tôi chỉ cần viết đúng C#. Xem `UNITY_SETUP.md` §3–4 cho
quy trình chạy các menu này, và bảng 7 shape khởi điểm mà `GemGridAssetSetup` điền sẵn
vào `BlockShapeSet.asset`.

## 6. Kết luận scope M1 + M1.5

M1 hoàn thành đúng 17 mục yêu cầu ở mức **logic + hook**, tách module rõ ràng, không
God Class, không hard-code số liệu/shape trong gameplay logic. M1.5 đưa project từ
"logic đã test" sang "có scene + tooling để mở bằng Unity và chạy thử thật" — vẫn ở
mức functionality-only (primitive/placeholder visual), không phải UI hoàn chỉnh. Phần
UI hoàn chỉnh, art, animation thật, âm thanh thật, Ads, IAP đều **cố ý để ngoài phạm
vi** — thuộc M2 trở đi theo đúng yêu cầu.
