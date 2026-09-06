# GemGrid — Unity Setup Guide (M1.5)

Hướng dẫn này giả định bạn có máy có GUI và có thể cài Unity Hub/Unity Editor — sandbox
phát triển dùng để viết code (không có Unity Editor) **không** dùng được cho các bước
dưới đây. Xem `README_M1.md` để biết chính xác phần nào đã/chưa được xác minh.

## 1. Unity version

Dự án được khởi tạo (M0) với mốc tham chiếu **Unity 2022.3 LTS** (xem
`ProjectSettings/ProjectVersion.txt`, ghi `2022.3.50f1`). Đây chỉ là **mốc tham
chiếu ban đầu**, không phải yêu cầu bắt buộc đúng patch version đó — tại thời điểm bạn
đọc tài liệu này, hãy dùng **bản Unity LTS mới nhất hiện có trong Unity Hub** (có thể
là một bản 2022.3.x mới hơn, hoặc dòng LTS kế tiếp như 6000.x/"Unity 6 LTS" nếu
2022.3 đã hết vòng đời hỗ trợ chính thức). Tôi cố tình không đoán một số patch chính
xác vì không có Unity Hub trong sandbox để kiểm chứng — chọn sai một con số cụ thể mà
không thể xác minh còn tệ hơn là nói rõ "dùng bản LTS mới nhất". Yêu cầu bắt buộc:
Android Build Support module (kèm Android SDK & NDK, OpenJDK) cài qua Unity Hub.

## 2. Mở project

1. Mở Unity Hub ▸ Add ▸ trỏ tới thư mục gốc repo này.
2. Mở project bằng version đã chọn ở mục 1. Lần mở đầu tiên, Unity sẽ tự sinh
   `Library/`, các file `.meta` còn thiếu cho toàn bộ script/asmdef, và phần còn lại
   của `ProjectSettings/` — đây là hành vi bình thường của Unity với một project mới
   từ git chưa từng mở qua Editor.
3. Đợi Unity import xong và biên dịch script (theo dõi thanh tiến trình + Console).
   Nếu Console có lỗi biên dịch, đó là điều **chưa từng được xác minh** trong môi
   trường viết code (không có Unity Editor ở đó) — xem README_M1.md mục "Chưa thể
   verify" trước khi báo lỗi.

## 3. Tạo data asset (BẮT BUỘC trước khi Play)

Không tự sửa YAML tay. Dùng menu Editor đã viết sẵn (`Assets/_Project/Editor/`):

1. `GemGrid ▸ Setup ▸ 0. Create All Required Data Assets`
   (hoặc chạy riêng `1. Create Starter Block Shape Set` rồi `2. Create Default Gameplay Config`)

Kết quả mong đợi: 2 asset mới tại `Assets/_Project/ScriptableObjects/`:
- `BlockShapeSet.asset` — đã điền sẵn 7 shape khởi điểm (dot, domino ngang/dọc,
  tromino L, square, line3 ngang/dọc) theo đúng bảng gợi ý ở `README_M1.md` §5. Có
  thể chỉnh/thêm shape trong Inspector — đây chỉ là điểm khởi đầu, không phải cân
  bằng cuối cùng (xem `ECONOMY_DESIGN.md`).
- `GameplayConfig.asset` — giá trị `ScoreRules`/`ComboRules` mặc định đã có sẵn trong
  code (`ScorePerPlacedCell=1`, `BaseScorePerLine=10`, `MultiLineBonusFactor=0.5`,
  `MultiplierPerComboStep=0.1`, `MaxMultiplier=3.0`).

Nếu asset đã tồn tại, menu sẽ cảnh báo trong Console và không ghi đè — xoá asset cũ
trước nếu muốn tạo lại từ đầu.

## 4. Tạo scene Boot & Gameplay

Sau khi có 2 asset ở mục 3:

`GemGrid ▸ Setup ▸ 5. Create Boot And Gameplay Scenes`
(hoặc riêng lẻ `3. Create Boot Scene` / `4. Create Gameplay Scene`)

Kết quả mong đợi: `Assets/_Project/Scenes/Boot.unity` và `Gameplay.unity`.

- **Boot.unity**: 1 Main Camera + 1 GameObject "Bootstrap" mang
  `GameManagerBehaviour` (đã gán sẵn 2 asset ở mục 3, grid 8x8, tray 3 slot) và
  `BootLoader` (sẽ load scene "Gameplay" ngay khi Play).
- **Gameplay.unity**: 1 Main Camera (orthographic, canh giữa lưới 8x8) + 1 GameObject
  "GameplayRoot" mang `GridController`, `BlockDragController`, `HapticHookListener`,
  `GameplayAnimationHooks`, `AudioHookListener`.

Sau khi tạo, thêm cả 2 scene vào **File ▸ Build Settings ▸ Scenes In Build**, **Boot
đặt trước Gameplay** (index 0 và 1) — bắt buộc để `SceneManager.LoadScene("Gameplay")`
trong `BootLoader` tìm thấy scene bằng tên.

## 5. Mở scene nào, bấm Play thế nào

Luôn mở và Play từ **`Assets/_Project/Scenes/Boot.unity`** (không Play trực tiếp từ
Gameplay.unity — `GridController`/`BlockDragController`/các hook listener đọc
`GameManagerBehaviour.Instance`, vốn chỉ tồn tại sau khi Boot chạy qua và dùng
`DontDestroyOnLoad`).

1. Mở `Boot.unity` trong cửa sổ Hierarchy/Project.
2. Bấm Play. Kỳ vọng: Console không có exception, scene tự chuyển sang `Gameplay`
   sau 1 frame, `GameManagerBehaviour.Instance.Game.State == Playing`.

**Về mặt hình ảnh**: `GridController` chưa được gán `cellSprite` (M1 cố tình để
placeholder — art thật thuộc M2), nên các ô lưới **có thể không hiển thị rõ** dù logic
vẫn chạy đúng. Muốn thấy lưới: chọn GameObject "GameplayRoot" trong Hierarchy khi
đang Play (hoặc trước khi Play), kéo một sprite hình vuông trắng bất kỳ (VD: sprite
built-in "Square" hoặc "UISprite" của Unity) vào field `Cell Sprite` trên component
`GridController`.

## 6. Cách test gameplay thủ công

Vì chưa có UI tray thật (M2), muốn tự tay kiểm tra logic khi đang Play:

- Mở cửa sổ **Debug** hoặc dùng breakpoint/Debug.Log tạm thời để gọi
  `GameManagerBehaviour.Instance.Game.TryPlaceBlock(slotIndex, new Int2(x, y))` từ
  Console/script tạm — hoặc viết một script test tạm thời gọi hàm này theo lịch trong
  `Update()` để quan sát Console/Inspector.
- Theo dõi `GameManagerBehaviour.Instance.Game.Score.TotalScore`,
  `.Combo.CurrentCombo`, `.State` trong Inspector (Debug mode) hoặc qua Debug.Log gắn
  tạm vào các event (`BlockPlaced`, `LinesClearedEvent`, `GameOver`, `GameRestarted`).
- Gọi `GameManagerBehaviour.Instance.Restart()` để kiểm tra restart.
- Full drag-and-drop bằng chuột/chạm thật (`BlockDragController.BeginDrag`) cần một
  tray UI gọi vào nó — chưa có ở M1/M1.5 (thuộc M2). Cho tới lúc đó, cách đáng tin cậy
  nhất để kiểm tra logic là Unity Test Runner (mục 7) hoặc gọi `TryPlaceBlock` thủ
  công như trên.

## 7. Chạy Unity Test Runner

**Window ▸ General ▸ Test Runner**

- Tab **EditMode**: chọn `GemGrid.Tests.EditMode`, bấm **Run All**. Kỳ vọng: 44/44
  pass — đây là *đúng những test đã thật sự chạy thành công* bằng `dotnet test` ở máy
  viết code (xem README_M1.md §2), nhưng **chưa có ai chạy qua chính Unity Test
  Runner** trong môi trường hiện tại.
- Tab **PlayMode**: chọn `GemGrid.Tests.PlayMode`, bấm **Run All**. Có 2 test
  (`BootFlowTests`) kiểm tra luồng Boot→Gameplay và Restart — **yêu cầu Boot.unity +
  Gameplay.unity đã được thêm vào Build Settings** (mục 4). Đây là test **hoàn toàn
  chưa được chạy/xác minh lần nào** — có rủi ro timing (số frame `yield return null`
  cần để chờ scene load) chưa được tinh chỉnh qua thực nghiệm.
- Chạy headless qua CLI (CI):
  ```bash
  Unity -batchmode -projectPath . -runTests -testPlatform EditMode \
        -testResults ./EditModeResults.xml -logFile ./editmode.log -quit
  Unity -batchmode -projectPath . -runTests -testPlatform PlayMode \
        -testResults ./PlayModeResults.xml -logFile ./playmode.log -quit
  ```

## 8. Build Android debug

1. **File ▸ Build Settings ▸ Android** ▸ Switch Platform (lần đầu sẽ mất vài phút để
   Unity re-import asset cho platform Android).
2. Đảm bảo `Boot` là scene đầu tiên trong danh sách Scenes In Build (mục 4).
3. **Player Settings** (nút trong cùng cửa sổ):
   - Package Name: đặt tạm `com.gemgrid.game` (placeholder — xem `RELEASE_PLAN.md`
     §1, cần Product Owner chốt tên thật trước khi publish).
   - Minimum API Level / Target API Level: theo khuyến nghị hiện hành của Google Play
     tại thời điểm build (xem `RELEASE_PLAN.md` §1).
   - Scripting Backend: IL2CPP, Target Architectures: ARM64 (+ ARMv7 nếu cần).
   - Orientation: Portrait (khoá xoay) — đúng thiết kế chơi một tay.
4. **Build** (không phải Build And Run nếu không có thiết bị/emulator kết nối) ▸ chọn
   thư mục output ▸ đợi Unity build xong ▸ kiểm tra file `.apk` sinh ra.
5. Đây là bản **debug build**, không ký bằng keystore release, chỉ để cài thử/kiểm
   tra chạy được trên thiết bị/emulator thật — không dùng để nộp Google Play (xem
   `RELEASE_PLAN.md` cho quy trình release thật ở M10).

## 9. Nếu có lỗi khi làm theo hướng dẫn này

Toàn bộ hướng dẫn trên **chưa được chạy thử bằng Unity Editor thật** trong môi trường
viết code hiện tại (không có Unity Editor ở đó — xem README_M1.md). Nếu bạn gặp lỗi ở
bước nào, đó là thông tin quý giá để tôi sửa — hãy chép nguyên văn lỗi trong Console
lại cho tôi.
