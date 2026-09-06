# M1 — Core Gameplay: hướng dẫn mở & kiểm tra

M1 triển khai toàn bộ logic gameplay lõi của GemGrid (grid 8x8, block shape
data-driven, spawner, kéo/thả, đặt block, clear hàng/cột, score, combo, game-over,
restart) theo `GAME_DESIGN.md` và `ARCHITECTURE.md`. Tài liệu này giải thích **chính
xác** những gì đã được xác minh, những gì chưa, và cách tự kiểm tra khi có Unity Editor.

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

## 3. Chạy test trong Unity Editor (khi có máy có Unity)

1. Cài **Unity 2022.3 LTS** (bản mới nhất trong nhánh 2022.3) qua Unity Hub, kèm
   Android Build Support.
2. Mở project bằng Unity Hub, trỏ tới thư mục gốc repo này. Lần mở đầu tiên, Unity sẽ
   tự sinh `Library/`, các file `.meta` còn thiếu, và phần còn lại của
   `ProjectSettings/` — đây là hành vi bình thường.
3. **Trước khi Play**, tạo 2 data asset bắt buộc (chưa có sẵn — xem mục 5):
   - `Assets ▸ Create ▸ GemGrid ▸ Configuration ▸ Block Shape Set`
   - `Assets ▸ Create ▸ GemGrid ▸ Configuration ▸ Gameplay Config`
4. Mở **Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All**. Kỳ vọng: 44 test pass
   (cùng bộ test đã chạy bằng `dotnet test` ở mục 2 — **chưa có ai chạy bước này
   thật sự trong môi trường hiện tại**, vì không có Unity Editor).
5. Để thử gameplay thủ công: tạo một scene, thêm một GameObject với
   `GameManagerBehaviour` + `GridController` + `BlockDragController` (+ tuỳ chọn
   `HapticHookListener`, `GameplayAnimationHooks`), gán 2 asset ở bước 3 vào các
   trường serialize, gán `cellSprite` bất kỳ (ô vuông trắng) trên `GridController` để
   nhìn thấy lưới, rồi Play. Đây là phần **hoàn toàn chưa được xác minh** trong môi
   trường hiện tại — không có gì đảm bảo nó chạy đúng ngay lần đầu.

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
(kể cả `GameManagerBehaviour`, `GridController`, `BlockDragController`,
`HapticHookListener`, `GameplayAnimationHooks`, `AudioHookListener`, `BlockShapeSet`,
`GameplayConfig`) trước một shim `UnityEngine` tự viết tối thiểu (chỉ định nghĩa lại
các kiểu/API thật sự được dùng: `MonoBehaviour`, `ScriptableObject`, `SerializeField`,
`Vector3`, `Color`, `SpriteRenderer`, `Camera`, `Input`, `Mathf`, `UnityEvent`...).

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
  `HapticHookListener`, `GameplayAnimationHooks`, `AudioHookListener`: thứ tự
  MonoBehaviour lifecycle (Awake/Start), input thật (chuột/chạm), render sprite.
- Test Runner EditMode chạy **bên trong Unity** (rất nhiều khả năng pass vì dùng
  đúng logic/test đã pass ở `dotnet test`, nhưng chưa ai chạy thật).
- Hiệu năng, cảm giác chơi thật, UX kéo-thả trên thiết bị/emulator.
- 2 ScriptableObject asset (`BlockShapeSet`, `GameplayConfig`) — class đã có, nhưng
  chưa có asset `.asset` cụ thể nào được tạo (xem mục 5 — cố ý không tự tạo tay để
  tránh sinh file YAML/GUID sai định dạng mà không cách nào xác minh).

## 5. Cần làm khi mở bằng Unity Editor lần đầu (chưa làm ở M1)

Tạo `BlockShapeSet.asset` (đặt tại `Assets/_Project/ScriptableObjects/`) với vài
shape khởi điểm gợi ý (tự thiết kế, không sao chép bộ hình của game nào khác) —
ví dụ:

| Id | Cells (offset x,y) | SpawnWeight |
|---|---|---|
| dot | (0,0) | 2 |
| domino_h | (0,0),(1,0) | 4 |
| domino_v | (0,0),(0,1) | 4 |
| tromino_l | (0,0),(1,0),(0,1) | 3 |
| tetromino_square | (0,0),(1,0),(0,1),(1,1) | 2 |
| line3_h | (0,0),(1,0),(2,0) | 3 |
| line3_v | (0,0),(0,1),(0,2) | 3 |

Tạo `GameplayConfig.asset` (cùng thư mục) giữ giá trị mặc định trong `ScoreRules`/
`ComboRules` (đã có default hợp lý trong code) hoặc chỉnh theo `ECONOMY_DESIGN.md`
sau khi playtest.

## 6. Kết luận scope M1

M1 hoàn thành đúng 17 mục yêu cầu ở mức **logic + hook**, tách module rõ ràng, không
God Class, không hard-code số liệu/shape trong gameplay logic. Phần UI hoàn chỉnh,
art, animation thật, âm thanh thật, Ads, IAP đều **cố ý để ngoài phạm vi** — thuộc
M2 trở đi theo đúng yêu cầu.
