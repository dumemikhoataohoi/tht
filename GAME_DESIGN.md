# GemGrid — Game Design Document (GDD)

## 1. Tổng quan

- **Thể loại**: Casual puzzle, grid line-clear.
- **Nền tảng**: Android, portrait, chơi một tay.
- **Phiên chơi**: 2–5 phút mỗi ván (session ngắn, phù hợp chơi lúc rảnh).
- **Đối tượng người chơi**: Casual, mọi lứa tuổi, không yêu cầu kỹ năng phản xạ.
- **Định vị nguyên bản**: Cơ chế "đặt hình vào lưới, đầy hàng/cột thì clear" là một
  mechanic phổ biến, thuộc dạng ý tưởng chung (không thể copyright ý tưởng gameplay
  ở mức khái quát). GemGrid triển khai bộ hình khối, tỉ lệ xuất hiện, công thức điểm,
  hệ combo, bộ power-up, UI, art style, âm thanh **của riêng mình**, không sao chép
  bố cục/level data/artwork/âm thanh/branding của Block Blast!, Candy Crush, Tetris
  hay bất kỳ sản phẩm thương mại nào khác.

## 2. Core loop

1. Người chơi thấy grid 8x8 và một khay chứa 3 block (shape) ngẫu nhiên.
2. Kéo từng block từ khay thả vào grid.
3. Nếu một hàng hoặc cột được lấp đầy hoàn toàn → hàng/cột đó được clear, cộng điểm.
4. Clear nhiều hàng/cột cùng lúc hoặc liên tiếp trong thời gian ngắn → combo, bonus điểm.
5. Khi khay hết 3 block → sinh khay mới.
6. Sau mỗi lần đặt/sinh khay, hệ thống kiểm tra: còn shape nào trong khay đặt được vào
   grid không? Nếu không còn vị trí hợp lệ cho **bất kỳ** shape nào trong khay → Game Over.
7. Người chơi xem điểm số, nhận thưởng Coin theo điểm, có thể chọn xem Rewarded Ad để
   hồi sinh (continue) hoặc nhận thưởng thêm, rồi chơi lại.

## 3. Grid & Block system

- Grid: 8x8 ô vuông, trạng thái mỗi ô: trống / đã chiếm (kèm màu/loại gem để vẽ).
- Khay (tray): luôn giữ 3 slot; khi cả 3 đã dùng hết mới sinh lại đồng loạt 3 block mới
  (không sinh từng cái một — giữ tính "lập kế hoạch" cho người chơi, đây là quyết định
  thiết kế của GemGrid, không cố định phải giống bất kỳ game nào khác).
- **Bộ block shape**: định nghĩa dạng polyomino (tổ hợp ô vuông liền kề) kích thước từ
  1 đến 5 ô, được liệt kê trong `ScriptableObject BlockShapeSet` — dữ liệu do GemGrid
  tự thiết kế (số lượng shape, hình dạng cụ thể, trọng số xuất hiện) để tránh trùng bộ
  hình + tỉ lệ của bất kỳ sản phẩm thương mại cụ thể nào.
- Trọng số xuất hiện của từng shape thay đổi theo "độ khó hiện tại" (tăng dần theo
  điểm số hoặc số lần clear liên tiếp) — dữ liệu cấu hình trong `DifficultyCurveConfig`.
- Mỗi shape có 1 màu/skin gem gán ngẫu nhiên trong bảng palette gốc (không dùng icon
  kẹo, không dùng ký hiệu Tetris chữ cái I/O/T/S/Z/J/L đặt tên thương hiệu — GemGrid
  tự đặt tên nội bộ theo id, ví dụ `shape_01`..`shape_NN`).

## 4. Input

- Drag & drop bằng một ngón tay: chạm vào block trong khay, kéo tới vị trí trên grid,
  thả để đặt (nếu hợp lệ) hoặc bật lại vị trí khay (nếu không hợp lệ).
- Hiển thị preview/ghost tại vị trí grid sẽ đặt, đổi màu preview (hợp lệ/không hợp lệ).
- Vùng chạm block được nới rộng hơn hình thực tế một chút (touch target lớn hơn visual)
  để thao tác một tay trên màn hình lớn dễ hơn.

## 5. Scoring

- Điểm cơ bản khi đặt block = số ô của shape × hệ số nhỏ (khuyến khích lấp đầy).
- Điểm khi clear 1 hàng/cột = `BaseLineScore` (giá trị cấu hình, ví dụ 10 × level hiện
  tại), nhân thêm hệ số nếu clear nhiều hàng/cột cùng lúc (multi-clear bonus).
- **Combo**: clear liên tiếp trong các lượt đặt gần nhau (không có lượt đặt nào "không
  clear" xen giữa) làm tăng combo counter; mỗi mốc combo tăng hệ số nhân điểm, đạt
  ngưỡng nhất định (Combo Level) sẽ mở khoá hiệu ứng hình ảnh/âm thanh + góp vào
  progression "Combo Level" dài hạn (mục 7).
- Công thức chi tiết đặt trong `ScoreConfig` (ScriptableObject), không hard-code.

## 6. Game Over detection

Sau mỗi lần: (a) đặt block xong, (b) sinh khay mới — hệ thống chạy thuật toán:
với mỗi shape còn lại trong khay, thử tất cả vị trí (x, y) trên grid 8x8 xem có tồn
tại vị trí đặt hợp lệ hay không (kiểm tra tất cả ô của shape đều nằm trong grid và đều
đang trống). Nếu **không có shape nào** trong khay có **bất kỳ** vị trí hợp lệ → Game
Over. Thuật toán này là logic thuần (không phụ thuộc UI) để unit test dễ dàng.

## 7. Progression

- **Combo Level**: tích luỹ combo tốt nhất/điểm combo theo thời gian dài hạn, mở khoá
  mốc thưởng (Coin, đôi khi Gem nhỏ) — dữ liệu trong `ComboProgressionConfig`.
- **Power-up Upgrade**: mỗi power-up có thể nâng cấp (giảm giá dùng, tăng hiệu ứng,
  hoặc tăng số lượng mang theo tối đa) bằng Coin/Gem — xem `ECONOMY_DESIGN.md`.
- **Daily Missions**: 3 mission ngẫu nhiên mỗi ngày (VD: "Clear 20 hàng", "Đạt combo
  x3 một lần", "Chơi 3 ván") — reset theo giờ địa phương, thưởng Coin/Gem/power-up.
- **Achievements**: mốc dài hạn (VD: tổng số hàng đã clear, điểm cao nhất, số ngày
  chơi liên tiếp) — thưởng một lần.
- **Player Level**: XP cộng dồn từ điểm mỗi ván (quy đổi), lên cấp mở khoá slot
  power-up mang theo nhiều hơn hoặc theme màu grid mới (không phải nội dung trả phí).

## 8. Power-ups

| Power-up | Hiệu ứng | Giới hạn dùng |
|---|---|---|
| Hammer | Phá 1 ô bất kỳ trên grid | Dùng ngoài lượt đặt, không giới hạn số lần trong ván (chỉ giới hạn bởi số lượng sở hữu) |
| Bomb | Phá vùng 3x3 quanh ô chọn | Giống trên |
| Shuffle | Đổi mới toàn bộ 3 block trong khay hiện tại (không tốn lượt) | Giống trên |
| Undo | Hoàn tác lượt đặt gần nhất (khôi phục grid + khay trước đó) | Giới hạn N bước lịch sử (cấu hình, mặc định 1 bước gần nhất ở bản free, nhiều hơn nếu nâng cấp) |
| Line Clear | Clear ngay 1 hàng hoặc cột chỉ định, không cần lấp đầy | Dùng ngoài lượt đặt |
| Double Score | Nhân đôi điểm nhận được trong một khoảng thời gian/số lượt giới hạn | Kích hoạt theo thời lượng cấu hình |

Power-up được mua bằng Coin/Gem hoặc nhận từ mission/achievement/rewarded ad. Chi
tiết giá & cân bằng: `ECONOMY_DESIGN.md`.

## 9. Chế độ chơi

- **Classic** (v1.0): mô tả ở trên, không giới hạn thời gian, không tốn Energy.
- **(Tương lai, ngoài scope v1.0)**: mode giới hạn bằng Energy, mode thử thách hàng
  ngày với seed cố định (để leaderboard công bằng) — ghi nhận là backlog, không thiết
  kế chi tiết ở giai đoạn này.

## 10. Art & Audio direction (nguyên bản)

- **Chủ đề hình ảnh**: tinh thể/gem trừu tượng (abstract crystal), bảng màu riêng của
  GemGrid, hình khối bo góc nhẹ, không dùng icon kẹo/trái cây theo phong cách match-3
  thương mại nào, không dùng chữ cái đặt tên khối kiểu Tetris.
- **UI**: tối giản, tương phản cao để chơi ngoài trời, toàn bộ icon/nút tự thiết kế.
- **Âm thanh**: SFX đặt/clear/combo tự sản xuất hoặc từ nguồn có license thương mại rõ
  ràng (ghi trong `LICENSE_MANIFEST.md`), nhạc nền loop nhẹ nhàng không lời.
- **Font**: dùng font mã nguồn mở giấy phép thương mại rõ ràng (VD: Google Fonts,
  OFL license) — không dùng font có bản quyền không rõ nguồn gốc.

## 11. Acceptance criteria — M1 (Core gameplay)

- [ ] Grid 8x8 khởi tạo đúng trạng thái trống.
- [ ] Đặt block hợp lệ cập nhật đúng trạng thái ô.
- [ ] Đặt block không hợp lệ (chồng ô đã chiếm, ra ngoài biên) bị chặn, không thay
      đổi trạng thái grid.
- [ ] Hàng/cột đầy đủ 8 ô được clear đúng, đúng số hàng/cột clear cùng lúc.
- [ ] Công thức điểm & combo tính đúng theo `ScoreConfig` (unit test cover các case:
      clear 1 hàng, clear nhiều hàng cùng lúc, combo liên tiếp, combo bị ngắt).
- [ ] Thuật toán game-over detection trả đúng kết quả cho các bộ test case grid/khay
      dựng sẵn (còn nước đi / hết nước đi).
- [ ] Toàn bộ logic trên có unit test EditMode pass, không phụ thuộc UI/scene.
