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
**Việc này giờ tự động** — `GemGridAutoSetup.cs` chạy ngay sau khi Unity biên dịch
xong (không cần tìm/bấm menu). Nó tự tạo `BlockShapeSet.asset`, `GameplayConfig.asset`,
`Boot.unity`, `MainMenu.unity`, `Gameplay.unity`, và tự thêm cả 3 scene vào Build
Settings — bỏ qua mọi bước đã tồn tại (an toàn khi chạy lại nhiều lần, không ghi đè).
Kiểm tra Console sau khi import xong: sẽ thấy vài dòng log `[GemGrid] Created ...`
nếu đây là lần đầu.

> **Đã có sẵn `Boot.unity`/`MainMenu.unity`/`Gameplay.unity` cục bộ từ lần test
> trước?** Auto-setup **sẽ bỏ qua** 3 scene này vì chúng "đã tồn tại" — nghĩa là bất kỳ
> thay đổi nào ở `GemGridSceneSetup.cs` (visual/animation/audio/safe-area của M3, hoặc
> tutorial/placement-preview/Game Over reason text mới nhất) sẽ KHÔNG xuất hiện cho tới
> khi bạn xoá 3 file cũ. Trước khi mở Unity để test bản mới: xoá
> `Assets/_Project/Scenes/Boot.unity`, `MainMenu.unity`, `Gameplay.unity` (và các
> `.meta` đi kèm) rồi mở lại project — auto-setup sẽ tạo lại cả 3 với nội dung mới nhất.

Nếu vì lý do nào đó menu `GemGrid ▸ Setup` vẫn không xuất hiện trong thanh menu chính
— **không sao**, không cần menu để chạy game, vì bước tự động ở trên đã lo việc đó.
Menu (từ `Assets/_Project/Editor/GemGridAssetSetup.cs` + `GemGridSceneSetup.cs`) chỉ
còn dùng để **tạo lại thủ công** khi cần (VD: sau khi xoá asset/scene để làm lại từ
đầu). Thứ tự hiển thị trong menu (nếu có) được đánh số 0→6, Unity sắp xếp đúng thứ tự
đó theo prefix số:

| Số | Menu item | Việc gì |
|---|---|---|
| 0 | Create All Required Data Assets | Gọi lại mục 1+2 |
| 1 | Create Starter Block Shape Set | Tạo `BlockShapeSet.asset` với 7 shape khởi điểm |
| 2 | Create Default Gameplay Config | Tạo `GameplayConfig.asset` với ScoreRules/ComboRules mặc định |
| 3 | Create Boot Scene | Tạo `Boot.unity` (cần asset ở mục 1,2 đã tồn tại) |
| 4 | Create Main Menu Scene | Tạo `MainMenu.unity` |
| 5 | Create Gameplay Scene | Tạo `Gameplay.unity` |
| 6 | Create Boot, Main Menu And Gameplay Scenes | Gọi lại mục 3+4+5 |

**8. Tạo required assets**
Đã tự động chạy ở bước 7. Nếu vì lý do gì đó chưa có (kiểm tra
`Assets/_Project/ScriptableObjects/`), chạy tay `GemGrid ▸ Setup ▸ 0. Create All
Required Data Assets`. Kết quả mong đợi: 2 asset tại `Assets/_Project/ScriptableObjects/`:
- `BlockShapeSet.asset` — 7 shape: dot, domino ngang/dọc, tromino L, square, line3
  ngang/dọc (bảng đầy đủ ở `README_M1.md` §5). Có thể chỉnh/thêm trong Inspector —
  đây là điểm khởi đầu, không phải cân bằng cuối cùng.
- `GameplayConfig.asset` — `ScorePerPlacedCell=1`, `BaseScorePerLine=10`,
  `MultiLineBonusFactor=0.5`, `MultiplierPerComboStep=0.1`, `MaxMultiplier=3.0`.

Nếu asset đã tồn tại, menu cảnh báo trong Console và không ghi đè — xoá asset cũ nếu
muốn tạo lại.

**9. Tạo scenes**
Đã tự động chạy ở bước 7. Nếu chưa có (kiểm tra `Assets/_Project/Scenes/`), chạy tay
`GemGrid ▸ Setup ▸ 6. Create Boot, Main Menu And Gameplay Scenes` (yêu cầu bước 8 đã
xong).
- **Boot.unity**: Main Camera + GameObject "Bootstrap" (`GameManagerBehaviour` đã gán
  sẵn 2 asset ở bước 8, grid 8x8, tray 3 slot; `BootLoader` sẽ load scene "MainMenu").
- **MainMenu.unity**: Canvas (responsive, Scale With Screen Size 1080x1920) với tiêu
  đề "GemGrid", Best Score, nút **START GAME** (`MainMenuController` load scene
  "Gameplay" khi bấm).
- **Gameplay.unity**: Main Camera (orthographic, canh giữa lưới 8x8) + GameObject
  "GameplayRoot" (`GameplaySessionStarter` đảm bảo luôn vào ván mới, `GridController`,
  `BlockDragController`, `BlockTray` chứa `BlockTrayView` để hiển thị/kéo 3 block màu
  gem trong khay, `HapticHookListener`, `GameplayAnimationHooks`, `AudioHookListener`)
  + Canvas chứa `GameplayHud` (Score/Best/Combo) và `GameOverScreen` (panel ẩn, hiện
  khi Game Over với nút RESTART/MAIN MENU).

**10. Add scenes vào Build Settings**
Đã tự động chạy ở bước 7. Kiểm tra lại: **File ▸ Build Settings ▸ Scenes In Build** —
thứ tự phải là `Boot.unity` (0), `MainMenu.unity` (1), `Gameplay.unity` (2). Bắt buộc
— nếu không, `SceneManager.LoadScene(...)` trong `BootLoader`/`MainMenuController`/
`GameOverScreen` sẽ không tìm thấy scene bằng tên.

### Chạy thử

**11. Mở Boot scene**
Double-click `Assets/_Project/Scenes/Boot.unity` trong Project window. **Không** mở
trực tiếp `MainMenu.unity`/`Gameplay.unity` để Play — các script đọc
`GameManagerBehaviour.Instance`, chỉ tồn tại sau khi Boot chạy qua (`DontDestroyOnLoad`).

> **Lưu ý quan trọng**: mặc định, nút Play của Unity Editor chạy **scene đang mở
> trong tab hiện tại**, không tự chạy từ Boot — nếu đang mở `MainMenu.unity` hoặc
> `Gameplay.unity` khi bấm Play, `Boot` (và do đó `GameManagerBehaviour`) sẽ không
> bao giờ được tạo, gây lỗi `InvalidOperationException: ... Instance is null` ở mọi
> component đọc `GameManagerBehaviour.Instance`. `GemGridAutoSetup.cs` đã tự đặt
> `EditorSceneManager.playModeStartScene = Boot.unity` sau khi compile xong, để bấm
> Play **luôn luôn** chạy từ Boot trước bất kể tab nào đang mở — không cần tự tay mở
> đúng Boot.unity nữa, nhưng vẫn nên làm vậy cho rõ ràng khi debug.

**12. Play**
Bấm nút Play. Kỳ vọng: Console không có exception, màn hình Main Menu hiện ra (nền
tối, tiêu đề "GemGrid", Best Score, nút START GAME).

**13. Kiểm tra Boot → Main Menu → Gameplay**
Sau ~1 frame, scene đang active chuyển từ `Boot` sang `MainMenu`
(`GameManagerBehaviour.Instance` phải khác null và
`GameManagerBehaviour.Instance.Game.State == GameStateType.Playing` ngay từ lúc này).
Bấm nút **START GAME** — scene chuyển sang `Gameplay`, ván mới bắt đầu (grid trống,
Score = 0) nhờ `GameplaySessionStarter` gọi `Restart()` mỗi khi vào Gameplay.

**14. Kiểm tra grid**
`GridController` tạo 8x8 = 64 GameObject con tên `Cell_x_y` dưới "GameplayRoot" trong
Hierarchy khi đang Play — hiển thị ngay bằng ô vuông xanh navy (trống)/xanh ngọc
(đã chiếm) tự sinh (không cần gán sprite thủ công), kèm hiệu ứng pop khi đặt và
flash trắng khi clear. Có thể thay bằng sprite riêng qua field `Cell Sprite` trên
`GridController` nếu muốn, không bắt buộc.

**15. Kiểm tra drag & drop (mouse)**
Trong Game view khi đang Play: 3 ô vuông màu gem (tím/hổ phách/lam ngọc/hồng/jade —
mỗi màu ứng với 1 shape) nằm dưới lưới (GameObject `BlockTray/TraySlot_0..2`) là 3
block trong khay. Bấm giữ chuột lên một ô, kéo lên
lưới, thả — nếu vị trí hợp lệ, block được đặt và biến mất khỏi khay; nếu không hợp lệ,
nó bật lại đúng vị trí cũ trong khay. Cơ chế: `TrayBlockView.OnMouseDown()` gọi
`BlockDragController.BeginDrag`, sau đó `Update()` theo dõi con trỏ qua
`IPointerInputSource` (`UnityPointerInputSource`, đọc `Input.mousePosition`).

**16. Kiểm tra touch input architecture (Android)**
Không cần thiết bị Android để xác nhận kiến trúc: `UnityPointerInputSource` ưu tiên
`Input.touchCount > 0` (chuẩn Input Manager cổ điển, hỗ trợ sẵn Android, không cần
package Input System mới) trước khi fallback về mouse — đọc code để xác nhận logic,
hoặc build APK debug (bước 24 trong `RELEASE_PLAN.md`/mục Build Android bên dưới) và
test kéo-thả bằng ngón tay thật trên thiết bị/emulator.

**17. Kiểm tra valid placement**
Kéo một block từ khay thả vào ô trống trên lưới (bước 15). Kỳ vọng: ô đó chuyển màu
(occupied), số **Score** trên góc trái màn hình (từ `GameplayHud`) tăng. Muốn
kiểm tra chính xác qua code: `GameManagerBehaviour.Instance.Game.TryPlaceBlock(0, new
Int2(0, 0))` từ Console/script tạm — trả về `true`,
`Game.Grid.IsCellOccupied(new Int2(0,0)) == true`.

**18. Kiểm tra invalid placement**
Kéo một block thả đè lên ô đã occupied. Kỳ vọng: block bật lại vị trí cũ trong khay
(bước 15 đã mô tả), Score không đổi. Qua code: gọi lại `TryPlaceBlock` nhắm cùng ô —
trả về `false`.

**19. Kiểm tra row clear**
Kéo đủ block lấp đầy 1 hàng (8 ô cùng `y`). Kỳ vọng: hàng đó tự động biến mất
(clear). Qua code: nhiều lệnh `TryPlaceBlock` với toạ độ khác nhau cùng `y` —
`Game.Grid.IsCellOccupied` trả về `false` cho toàn bộ ô trong hàng sau khi đầy.

**20. Kiểm tra column clear**
Tương tự bước 19 nhưng lấp theo cột (`x` cố định, `y` chạy 0→7).

**21. Kiểm tra score & combo**
Theo dõi **Score**/**Combo** trên HUD góc trên bên trái (`GameplayHud`) trong
lúc thực hiện bước 17–20. Kỳ vọng: điểm tăng đúng theo `ScoreRules`, combo tăng khi
clear liên tiếp, reset về 0 khi có lượt không clear (công thức ở
`ECONOMY_DESIGN.md`/`GAME_DESIGN.md`).

**22. Kiểm tra game over & restart**
Tiếp tục đặt block cho tới khi không còn shape nào trong khay đặt được vào chỗ trống
nào của grid. Kỳ vọng: panel Game Over (`GameOverScreen`) hiện lên — nền tối phủ toàn
màn hình, chữ "GAME OVER", điểm cuối + best score, nút **RESTART** và **MAIN MENU**.
Bấm **RESTART** — kỳ vọng: panel biến mất, grid trống lại, Score về 0, State về
`Playing`. Bấm **MAIN MENU** (ở lần Game Over khác) — kỳ vọng: quay lại scene
MainMenu, Best Score hiển thị đúng giá trị cao nhất đã đạt. (Qua code:
`GameManagerBehaviour.Instance.Restart()`.)

### Chạy test

**23. Chạy EditMode tests**
**Window ▸ General ▸ Test Runner ▸ tab EditMode** ▸ chọn `GemGrid.Tests.EditMode` ▸
**Run All**. Kỳ vọng: 44/44 pass — đây là *đúng* bộ test đã chạy thật thành công bằng
`dotnet test` ngoài Unity (xem `README_M1.md` §2), nhưng **chưa ai chạy qua chính
Unity Test Runner**.

**24. Chạy PlayMode tests**
**Window ▸ General ▸ Test Runner ▸ tab PlayMode** ▸ chọn `GemGrid.Tests.PlayMode` ▸
**Run All**. 3 test trong `BootFlowTests.cs` (Boot→MainMenu, Start Game→Gameplay ván
mới, Restart) — **yêu cầu bước 10 (3 scene trong Build Settings đúng thứ tự) đã làm
xong**. Đây là test **hoàn toàn chưa chạy lần nào** — rủi ro lớn nhất là số frame
`yield return null` giả định để chờ chuyển scene có thể không khớp thực tế; nếu fail,
chép log lại, đừng tự tăng số frame mà không báo.

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
