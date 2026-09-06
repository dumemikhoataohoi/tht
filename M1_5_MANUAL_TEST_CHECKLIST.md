# M1.5 — Manual Test Checklist (Unity Editor thật)

Điền checklist này khi thực hiện `UNITY_SETUP.md` (24 bước). Mọi dòng bắt đầu ở trạng
thái **NOT VERIFIED** — chưa có bước nào trong tài liệu này từng được chạy bằng Unity
Editor thật (sandbox viết code không có Unity Editor). Cập nhật cột PASS/FAIL và NOTES
khi bạn tự chạy; đừng để ai (kể cả tôi) đánh dấu PASS thay cho một bước chưa thực sự
chạy.

Cột **REF** trỏ tới bước tương ứng trong `UNITY_SETUP.md`.

| TEST ID | CATEGORY | ACTION | EXPECTED RESULT | PASS/FAIL | NOTES |
|---|---|---|---|---|---|
| ENV-001 | Environment | Clone repo, checkout `feature/m1.5-unity-integration`, mở project bằng Unity Hub (REF: bước 2–4) | Unity Hub nhận diện project, bắt đầu import không lỗi nghiêm trọng | NOT VERIFIED | |
| ENV-002 | Environment | Đợi import + biên dịch xong, kiểm tra Console (REF: bước 5–6) | Không có compile error nào trong Console | NOT VERIFIED | |
| SETUP-001 | Setup | Chạy `GemGrid ▸ Setup ▸ 0. Create All Required Data Assets` (REF: bước 7–8) | `BlockShapeSet.asset` (7 shape) + `GameplayConfig.asset` xuất hiện tại `Assets/_Project/ScriptableObjects/`, không lỗi Console | NOT VERIFIED | |
| SETUP-002 | Setup | Chạy `GemGrid ▸ Setup ▸ 5. Create Boot And Gameplay Scenes`, thêm cả 2 vào Build Settings (Boot trước) (REF: bước 9–10) | `Boot.unity` + `Gameplay.unity` xuất hiện tại `Assets/_Project/Scenes/`, cả 2 nằm trong Scenes In Build đúng thứ tự | NOT VERIFIED | |
| BOOT-001 | Boot | Mở `Boot.unity`, bấm Play (REF: bước 11–12) | Console không có exception khi Play | NOT VERIFIED | |
| BOOT-002 | Boot | Quan sát scene sau ~1 frame (REF: bước 13) | Scene active chuyển từ `Boot` sang `Gameplay`; `GameManagerBehaviour.Instance != null`; `Game.State == Playing` | NOT VERIFIED | |
| GRID-001 | Grid | Kiểm tra Hierarchy dưới "GameplayRoot" khi đang Play (REF: bước 14) | 64 GameObject con `Cell_x_y` (8x8) được tạo bởi `GridController` | NOT VERIFIED | Cells có thể không hiển thị màu nếu chưa gán `cellSprite` — đây không phải lỗi logic |
| INPUT-001 | Input | Gọi `BlockDragController.BeginDrag(...)` thủ công, di chuột trong Game view (REF: bước 15) | Transform được kéo theo di chuyển đúng theo `ScreenToWorldPoint` | NOT VERIFIED | Chưa có UI tray thật (M2) để test bằng thao tác chuột bình thường |
| INPUT-002 | Input | Đọc code `UnityPointerInputSource`, hoặc test trên thiết bị/emulator Android thật (REF: bước 16) | `Input.touchCount > 0` được ưu tiên trước mouse fallback; kéo-thả bằng ngón tay hoạt động tương tự mouse | NOT VERIFIED | Không có thiết bị Android trong sandbox viết code |
| GAME-001 | Gameplay | `TryPlaceBlock(0, new Int2(0,0))` với slot có shape hợp lệ (REF: bước 17) | Trả về `true`; ô `(0,0)` occupied; `Score.TotalScore` tăng | NOT VERIFIED | |
| GAME-002 | Gameplay | `TryPlaceBlock` nhắm vào ô đã occupied (REF: bước 18) | Trả về `false`; `Score.TotalScore` không đổi | NOT VERIFIED | |
| GAME-003 | Gameplay | Lấp đầy 1 hàng bằng nhiều `TryPlaceBlock` (REF: bước 19) | Hàng tự động clear; toàn bộ ô trong hàng trở lại `IsCellOccupied == false` | NOT VERIFIED | |
| GAME-004 | Gameplay | Lấp đầy 1 cột bằng nhiều `TryPlaceBlock` (REF: bước 20) | Cột tự động clear tương tự GAME-003 | NOT VERIFIED | |
| GAME-005 | Gameplay | Lấp đầy đồng thời 1 hàng + 1 cột giao nhau trong cùng 1 lần đặt | Cả hàng và cột cùng clear trong 1 `PlacementResult` (đã verify ở mức logic — `GridModelTests.Place_FillingRowAndColumnAtOnce_ClearsBothSimultaneously`, xem GAME-010) | NOT VERIFIED | Verify logic thuần đã PASS qua `dotnet test`; đây là verify lại qua Unity thật |
| GAME-006 | Gameplay | Theo dõi `Score.TotalScore` qua các bước GAME-001→005 (REF: bước 21) | Điểm tăng đúng công thức `ScoreRules` (base + multi-line bonus + combo multiplier) | NOT VERIFIED | |
| GAME-007 | Gameplay | Theo dõi `Combo.CurrentCombo`/`ComboChanged` qua các lượt clear liên tiếp và lượt không clear (REF: bước 21) | Combo tăng khi clear liên tiếp, reset về 0 khi có lượt không clear | NOT VERIFIED | |
| GAME-008 | Gameplay | Tiếp tục đặt block tới khi không còn nước đi (REF: bước 22) | `Game.State == GameOver`; `TryPlaceBlock` sau đó luôn `false` | NOT VERIFIED | |
| GAME-009 | Gameplay | Gọi `GameManagerBehaviour.Instance.Restart()` sau GAME-008 (REF: bước 22) | Grid trống lại, `Score.TotalScore == 0`, `State == Playing` | NOT VERIFIED | |
| GAME-010 | Gameplay (tham chiếu) | — | 44 EditMode test đã chạy thật bằng `dotnet test` ngoài Unity, PASS 44/44 — xem TEST-001 để verify lại qua chính Unity Test Runner | VERIFIED (qua dotnet test, chưa qua Unity) | Xem `README_M1.md` §2 |
| TEST-001 | Automated | Test Runner ▸ EditMode ▸ `GemGrid.Tests.EditMode` ▸ Run All (REF: bước 23) | 44/44 pass | NOT VERIFIED | |
| TEST-002 | Automated | Test Runner ▸ PlayMode ▸ `GemGrid.Tests.PlayMode` ▸ Run All (REF: bước 24) | 2/2 pass (`Boot_LoadsGameplayScene_...`, `Restart_ResetsScoreAndGrid_...`) | NOT VERIFIED | Rủi ro timing frame-count cao nhất trong toàn bộ checklist |

## Quy ước điền checklist

- Chỉ đánh **PASS** khi đã tự tay chạy đúng ACTION và quan sát đúng EXPECTED RESULT.
- Đánh **FAIL** kèm NOTES mô tả chính xác sai khác (log Console, ảnh chụp nếu cần).
- Không xoá dòng NOT VERIFIED — đó là ghi nhận trung thực trạng thái tại thời điểm
  tài liệu này được tạo, không phải placeholder cần xoá.
- Nếu một bước không áp dụng được (VD: không có thiết bị Android cho INPUT-002), ghi
  **N/A** kèm lý do trong NOTES, không để trống.
