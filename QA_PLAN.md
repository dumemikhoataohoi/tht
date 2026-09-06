# GemGrid — QA Plan

## 1. Giới hạn môi trường hiện tại (quan trọng)

Sandbox phát triển hiện tại **không có Unity Editor** cài đặt (không có Unity Hub,
không có `Unity.exe`/binary Editor, không có license). Vì vậy:
- Tôi sẽ viết unit test (EditMode/NUnit) đầy đủ cho toàn bộ logic thuần theo từng
  milestone, đúng chuẩn Unity Test Framework.
- Việc **chạy thực tế** các test này qua Unity Test Runner, build Play Mode, build
  APK/AAB, và test trên thiết bị/emulator Android cần thực hiện khi project được mở
  bằng Unity Editor thật (Unity Hub trên máy có GUI) hoặc qua CI có Unity license
  (Unity Cloud Build, GameCI trên GitHub Actions...).
- Tôi sẽ cung cấp hướng dẫn chạy test/build rõ ràng trong `README.md` của project để
  Product Owner (hoặc CI) thực hiện bước này.

## 2. Test pyramid

| Tầng | Công cụ | Phạm vi |
|---|---|---|
| Unit test (nhiều nhất) | NUnit / Unity Test Framework — EditMode | Grid logic, scoring, combo, game-over detection, economy transaction, save serialize/deserialize, power-up effect |
| Integration test | Unity Test Framework — PlayMode | Luồng input → grid → UI cập nhật, luồng mua hàng mock → wallet cập nhật |
| Manual/exploratory | Checklist thủ công | UX, ads flow thực tế, IAP sandbox, hiệu năng thiết bị thật |

## 3. Checklist unit test theo milestone

- **M1**: đặt hợp lệ/không hợp lệ, clear 1 hàng/cột, clear nhiều dòng cùng lúc, combo
  tăng/giảm/reset, game-over detection (còn nước đi / hết nước đi) với các bộ grid
  dựng sẵn (bao gồm edge case: grid gần đầy, shape 1 ô, shape 5 ô).
- **M3**: tính XP/level, điều kiện hoàn thành mission, điều kiện mở achievement.
- **M4**: cộng/trừ ví không cho âm, mua power-up đủ/thiếu tiền, nâng cấp đổi đúng
  hiệu ứng.
- **M5**: frequency cap logic (dùng clock giả lập), không trigger interstitial khi
  `State == Playing`.
- **M6**: purchase fulfillment cộng đúng reward, không cộng trùng (idempotent theo
  transaction id), restore purchase.
- **M7**: save/load round-trip giữ nguyên dữ liệu, xử lý version cũ (migration an
  toàn khi thêm field mới không làm hỏng save cũ).
- **M8**: event được ghi đúng tên/tham số theo schema chuẩn hoá.

## 4. Manual test checklist (thực hiện khi có Unity Editor + thiết bị)

- [ ] Chơi thử một tay trên màn hình 5.5"–6.7" (kiểm tra vùng chạm khay/nút thoải mái).
- [ ] Test trên thiết bị Android cấu hình thấp (kiểm tra FPS ổn định, không giật khi
      hiệu ứng clear/combo nhiều).
- [ ] Kiểm tra ảnh hưởng của các tỉ lệ màn hình khác nhau (16:9, 18:9, 19.5:9, tablet)
      — UI không bị cắt, grid luôn hiển thị đầy đủ, vuông.
- [ ] Interstitial không bao giờ xuất hiện đè lên gameplay đang diễn ra.
- [ ] Rewarded ad flow: bấm nút → xem quảng cáo → nhận đúng thưởng; huỷ giữa chừng
      không nhận thưởng và không mất gì.
- [ ] IAP sandbox: mua từng sản phẩm, kiểm tra reward, kiểm tra Restore Purchases sau
      khi cài lại app.
- [ ] Save/load: đóng app giữa ván, mở lại đúng trạng thái (hoặc reset ván hợp lý nếu
      thiết kế không lưu giữa ván dở — quyết định rõ ở M7).
- [ ] Âm thanh: mute/volume settings áp dụng đúng, không có SFX chồng lấn gây khó chịu.
- [ ] Kiểm tra không có văn bản/hình ảnh nào trùng với sản phẩm thương mại khác (đối
      chiếu với `LICENSE_MANIFEST.md` và mục IP trong `PROJECT_PLAN.md`).

## 5. Performance targets

- 60 FPS trên thiết bị tầm trung (2021+), tối thiểu 30 FPS ổn định trên thiết bị
  thấp cấp được hỗ trợ chính thức.
- Kích thước APK/AAB ban đầu mục tiêu < 150MB (điều chỉnh theo asset thực tế).
- Thời gian cold start < 3 giây trên thiết bị tầm trung.
- Không rò rỉ bộ nhớ qua nhiều ván chơi liên tiếp (kiểm tra bằng Unity Profiler khi
  có Editor).

## 6. Quy trình sau mỗi milestone

1. Chạy toàn bộ unit test liên quan milestone (khi có Unity Editor) — 100% pass.
2. Review code theo nguyên tắc kiến trúc (`ARCHITECTURE.md`) — không God Class, đúng
   asmdef boundary.
3. Cập nhật acceptance criteria trong design doc tương ứng.
4. Cập nhật `CHANGELOG.md`.
5. Chạy manual checklist liên quan (nếu milestone có phần UI/device-facing).

## 7. Bug tracking

Trong giai đoạn này (không có issue tracker riêng), bug được ghi nhận trực tiếp vào
`CHANGELOG.md` mục "Fixed" theo từng milestone, kèm mô tả ngắn gọn nguyên nhân/khắc
phục.

## 8. Acceptance criteria — M9 (QA)

- [ ] 100% unit test toàn bộ module pass.
- [ ] Manual checklist mục 4 hoàn thành, không còn mục nghiêm trọng (P0/P1) chưa xử lý.
- [ ] Performance targets mục 5 đạt trên ít nhất 1 thiết bị tầm trung và 1 thiết bị
      thấp cấp thực tế.
- [ ] Không phát hiện asset/text/mechanic trùng lặp với sản phẩm thương mại khác.
