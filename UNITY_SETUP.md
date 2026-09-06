# GemGrid — Unity Setup Guide (M1.5)

Hướng dẫn này giả định bạn có máy có GUI và có thể cài Unity Hub/Unity Editor — sandbox
phát triển dùng để viết code (không có Unity Editor) **không** dùng được cho các bước
dưới đây. Xem `README_M1.md` để biết chính xác phần nào đã/chưa được xác minh — **toàn
bộ 24 bước dưới đây chưa từng được chạy thử thật**, kể cả bởi tôi.

## Quy trình đầy đủ (24 bước)

### Cài đặt & mở project

**1. Cài Unity version nào**
Mốc tham chiếu ban đầu (M0): `2022.3.50f1` (xem `ProjectSettings/ProjectVersion.txt`).
Đây **không phải** yêu cầu bắt buộc đúng patch này — dùng **bản Unity LTS mới nhất
hiện có trong Unity Hub** tại thời điểm bạn đọc tài liệu (có thể là 2022.3.x mới hơn,
hoặc dòng LTS kế tiếp như 6000.x/"Unity 6 LTS" nếu 2022.3 đã hết hỗ trợ chính thức).
Tôi không đoán một patch cụ thể vì không có Unity Hub trong sandbox để kiểm chứng —
đoán sai một con số cụ thể còn tệ hơn nói rõ "dùng bản LTS mới nhất". Bắt buộc kèm
**Android Build Support** (Android SDK & NDK, OpenJDK) qua Unity Hub.

**2. Clone repository**
```bash
git clone https://github.com/dumemikhoataohoi/tht.git
cd tht
```

**3. Checkout branch**
```bash
git checkout feature/m1.5-unity-integration
```
(Toàn bộ M1.5 nằm trên branch này, chưa merge vào `main`.)

**4. Mở project**
Unity Hub ▸ Add ▸ trỏ tới thư mục repo vừa clone ▸ mở bằng version đã chọn ở bước 1.

**5. Chờ Unity import**
Lần mở đầu tiên, Unity tự sinh `Library/`, các file `.meta` còn thiếu cho toàn bộ
script/asmdef, và phần còn lại của `ProjectSettings/` — đây là hành vi bình thường
với một project git chưa từng mở qua Editor. Có thể mất vài phút tuỳ máy.

**6. Xử lý compile errors nếu có**
Theo dõi Console sau khi import xong. Toàn bộ code đã được syntax-check bằng shim
UnityEngine/UnityEditor tự viết (`tools/dotnet-tests/GemGrid.UnitySyntaxCheck/`, build
sạch 0 lỗi) nhưng **chưa từng biên dịch bằng Unity thật** — nếu Console báo lỗi, đó là
điều tôi cần biết để sửa. Chép nguyên văn lỗi (kèm file/dòng) lại. Không tự "làm cho
hết lỗi" bằng cách xoá code nếu không chắc nguyên nhân — báo lại trước.

### Tạo data asset & scene (KHÔNG hand-edit YAML)

**7. Chạy GemGrid Setup**
Menu `GemGrid ▸ Setup` xuất hiện sau khi biên dịch xong (từ
`Assets/_Project/Editor/GemGridAssetSetup.cs` + `GemGridSceneSetup.cs`). Thứ tự hiển
thị trong menu đã được đánh số 0→5 và Unity sắp xếp đúng thứ tự đó theo prefix số:

| Số | Menu item | Việc gì |
|---|---|---|
| 0 | Create All Required Data Assets | Gọi lại mục 1+2 |
| 1 | Create Starter Block Shape Set | Tạo `BlockShapeSet.asset` với 7 shape khởi điểm |
| 2 | Create Default Gameplay Config | Tạo `GameplayConfig.asset` với ScoreRules/ComboRules mặc định |
| 3 | Create Boot Scene | Tạo `Boot.unity` (cần asset ở mục 1,2 đã tồn tại) |
| 4 | Create Gameplay Scene | Tạo `Gameplay.unity` |
| 5 | Create Boot And Gameplay Scenes | Gọi lại mục 3+4 |

**8. Tạo required assets**
Chạy `GemGrid ▸ Setup ▸ 0. Create All Required Data Assets`. Kết quả mong đợi: 2 asset
mới tại `Assets/_Project/ScriptableObjects/`:
- `BlockShapeSet.asset` — 7 shape: dot, domino ngang/dọc, tromino L, square, line3
  ngang/dọc (bảng đầy đủ ở `README_M1.md` §5). Có thể chỉnh/thêm trong Inspector —
  đây là điểm khởi đầu, không phải cân bằng cuối cùng.
- `GameplayConfig.asset` — `ScorePerPlacedCell=1`, `BaseScorePerLine=10`,
  `MultiLineBonusFactor=0.5`, `MultiplierPerComboStep=0.1`, `MaxMultiplier=3.0`.

Nếu asset đã tồn tại, menu cảnh báo trong Console và không ghi đè — xoá asset cũ nếu
muốn tạo lại.

**9. Tạo scenes**
Chạy `GemGrid ▸ Setup ▸ 5. Create Boot And Gameplay Scenes` (yêu cầu bước 8 đã xong).
Kết quả mong đợi: `Assets/_Project/Scenes/Boot.unity` + `Gameplay.unity`.
- **Boot.unity**: Main Camera + GameObject "Bootstrap" (`GameManagerBehaviour` đã gán
  sẵn 2 asset ở bước 8, grid 8x8, tray 3 slot; `BootLoader` sẽ load scene "Gameplay").
- **Gameplay.unity**: Main Camera (orthographic, canh giữa lưới 8x8) + GameObject
  "GameplayRoot" (`GridController`, `BlockDragController`, `HapticHookListener`,
  `GameplayAnimationHooks`, `AudioHookListener`).

**10. Add scenes vào Build Settings**
**File ▸ Build Settings ▸ Scenes In Build** ▸ kéo `Boot.unity` vào trước (index 0),
`Gameplay.unity` sau (index 1). Bắt buộc — nếu không, `SceneManager.LoadScene
("Gameplay")` trong `BootLoader` sẽ không tìm thấy scene bằng tên.

### Chạy thử

**11. Mở Boot scene**
Double-click `Assets/_Project/Scenes/Boot.unity` trong Project window. **Không** mở
trực tiếp `Gameplay.unity` để Play — `GridController`/`BlockDragController`/hook
listener đọc `GameManagerBehaviour.Instance`, chỉ tồn tại sau khi Boot chạy qua
(`DontDestroyOnLoad`).

**12. Play**
Bấm nút Play. Kỳ vọng: Console không có exception.

**13. Kiểm tra Boot → Gameplay**
Sau ~1 frame, scene đang active phải chuyển từ `Boot` sang `Gameplay` (xem tiêu đề
Hierarchy window hoặc gọi `SceneManager.GetActiveScene().name` từ breakpoint/log tạm).
`GameManagerBehaviour.Instance` phải khác null và
`GameManagerBehaviour.Instance.Game.State == GameStateType.Playing`.

**14. Kiểm tra grid**
`GridController` tạo 8x8 = 64 GameObject con tên `Cell_x_y` dưới "GameplayRoot" trong
Hierarchy khi đang Play. **Lưu ý**: `cellSprite` chưa được gán (M1 cố tình để
placeholder, art thật thuộc M2) nên ô lưới **có thể không hiển thị màu/hình** dù logic
đã đúng — gán một sprite hình vuông bất kỳ vào field `Cell Sprite` trên
`GridController` (trong Hierarchy, chọn "GameplayRoot") nếu muốn xác nhận bằng mắt.

**15. Kiểm tra drag & drop (mouse)**
Chưa có UI tray thật (M2) để tự bấm-kéo trên màn hình. Cách kiểm tra kiến trúc input:
gọi `BlockDragController.BeginDrag(slotIndex, someTransform)` từ một script tạm hoặc
breakpoint, sau đó di chuột trong Game view và quan sát `someTransform.position` cập
nhật theo `Camera.main.ScreenToWorldPoint`. `IPointerInputSource`
(`UnityPointerInputSource`) là nơi đọc `Input.mousePosition`/`GetMouseButtonUp` — xem
`ARCHITECTURE.md` phần "Input abstraction".

**16. Kiểm tra touch input architecture (Android)**
Không cần thiết bị Android để xác nhận kiến trúc: `UnityPointerInputSource` ưu tiên
`Input.touchCount > 0` (chuẩn Input Manager cổ điển, hỗ trợ sẵn Android, không cần
package Input System mới) trước khi fallback về mouse — đọc code để xác nhận logic,
hoặc build APK debug (bước 24 trong `RELEASE_PLAN.md`/mục Build Android bên dưới) và
test kéo-thả bằng ngón tay thật trên thiết bị/emulator.

**17. Kiểm tra valid placement**
Gọi `GameManagerBehaviour.Instance.Game.TryPlaceBlock(0, new Int2(0, 0))` (slot 0
đang có shape hợp lệ) từ Console/script tạm khi đang Play. Kỳ vọng: trả về `true`,
`Game.Grid.IsCellOccupied(new Int2(0,0)) == true`, `Game.Score.TotalScore` tăng.

**18. Kiểm tra invalid placement**
Gọi lại `TryPlaceBlock` với slot khác nhắm vào đúng ô `(0,0)` vừa chiếm. Kỳ vọng: trả
về `false`, `Game.Score.TotalScore` không đổi.

**19. Kiểm tra row clear**
Đặt đủ 8 block liên tiếp lấp đầy 1 hàng (dùng nhiều lệnh `TryPlaceBlock` với toạ độ
khác nhau trong cùng `y`). Kỳ vọng: hàng đó tự động clear, `Game.Grid.IsCellOccupied`
trả về `false` cho toàn bộ ô trong hàng sau khi đầy.

**20. Kiểm tra column clear**
Tương tự bước 19 nhưng lấp theo cột (`x` cố định, `y` chạy 0→7).

**21. Kiểm tra score & combo**
Theo dõi `Game.Score.TotalScore` và `Game.Combo.CurrentCombo` (Debug.Log tạm gắn vào
event `BlockPlaced`/`LinesClearedEvent`/`Combo.ComboChanged`) trong lúc thực hiện bước
17–20. Kỳ vọng: điểm tăng đúng theo `ScoreRules`, combo tăng khi clear liên tiếp, reset
về 0 khi có lượt không clear (xem công thức ở `ECONOMY_DESIGN.md`/`GAME_DESIGN.md`).

**22. Kiểm tra game over & restart**
Tiếp tục đặt block cho tới khi không còn shape nào trong tray đặt được vào chỗ trống
nào của grid. Kỳ vọng: `Game.State == GameStateType.GameOver`, `TryPlaceBlock` sau đó
luôn trả về `false`. Gọi `GameManagerBehaviour.Instance.Restart()` — kỳ vọng: grid
trống lại, `Score.TotalScore == 0`, `State == Playing`.

### Chạy test

**23. Chạy EditMode tests**
**Window ▸ General ▸ Test Runner ▸ tab EditMode** ▸ chọn `GemGrid.Tests.EditMode` ▸
**Run All**. Kỳ vọng: 44/44 pass — đây là *đúng* bộ test đã chạy thật thành công bằng
`dotnet test` ngoài Unity (xem `README_M1.md` §2), nhưng **chưa ai chạy qua chính
Unity Test Runner**.

**24. Chạy PlayMode tests**
**Window ▸ General ▸ Test Runner ▸ tab PlayMode** ▸ chọn `GemGrid.Tests.PlayMode` ▸
**Run All**. 2 test trong `BootFlowTests.cs` — **yêu cầu bước 10 (scenes trong Build
Settings) đã làm xong**. Đây là test **hoàn toàn chưa chạy lần nào** — rủi ro lớn nhất
là số frame `yield return null` giả định để chờ Boot→Gameplay chuyển scene có thể
không khớp thực tế; nếu fail, chép log lại, đừng tự tăng số frame mà không báo.

Chạy headless qua CLI (CI), sau khi đã có `Library/` từ lần mở Editor đầu tiên:
```bash
Unity -batchmode -projectPath . -runTests -testPlatform EditMode \
      -testResults ./EditModeResults.xml -logFile ./editmode.log -quit
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode \
      -testResults ./PlayModeResults.xml -logFile ./playmode.log -quit
```

## Build Android debug (không nằm trong 24 bước bắt buộc, thực hiện khi cần)

1. **File ▸ Build Settings ▸ Android** ▸ Switch Platform (lần đầu mất vài phút để
   Unity re-import asset cho platform Android).
2. Đảm bảo `Boot` là scene đầu tiên trong Scenes In Build (bước 10 ở trên).
3. **Player Settings**:
   - Package Name: đặt tạm `com.gemgrid.game` (placeholder — xem `RELEASE_PLAN.md`
     §1, cần Product Owner chốt tên thật trước khi publish).
   - Minimum/Target API Level: theo khuyến nghị hiện hành của Google Play tại thời
     điểm build (xem `RELEASE_PLAN.md` §1).
   - Scripting Backend: IL2CPP, Target Architectures: ARM64 (+ ARMv7 nếu cần).
   - Orientation: Portrait (khoá xoay).
4. **Build** (không phải Build And Run nếu không có thiết bị/emulator kết nối) ▸ chọn
   thư mục output ▸ đợi Unity build xong ▸ kiểm tra file `.apk` sinh ra.
5. Đây là bản **debug build**, không ký bằng keystore release — chỉ để cài thử/kiểm
   tra chạy được trên thiết bị/emulator thật, không dùng để nộp Google Play (xem
   `RELEASE_PLAN.md` cho quy trình release thật ở M10).

## Nếu có lỗi ở bất kỳ bước nào

Toàn bộ 24 bước trên **chưa được chạy thử bằng Unity Editor thật** trong môi trường
viết code hiện tại (xem `README_M1.md`). Nếu bạn gặp lỗi, hãy điền vào
`M1_5_MANUAL_TEST_CHECKLIST.md` (cột PASS/FAIL + NOTES) và chép nguyên văn lỗi
Console lại — đó là thông tin cần thiết để sửa, không phải lỗi của bạn.
