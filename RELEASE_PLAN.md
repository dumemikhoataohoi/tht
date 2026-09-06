# GemGrid — Release Plan (Android / Google Play)

## 1. Thông tin build

| Mục | Giá trị đề xuất | Ghi chú |
|---|---|---|
| Package name (applicationId) | `com.gemgrid.game` (placeholder) | Cần Product Owner xác nhận domain/công ty thật trước khi đăng ký trên Play Console — đây là quyết định **không đảo ngược được** sau khi publish lần đầu |
| Tên hiển thị (app label) | GemGrid (tạm) | Cần rà soát trademark trước khi chốt tên chính thức để tránh trùng app đã có trên Play Store |
| Min SDK | Android 7.0 (API 24) | Cân bằng độ phủ thiết bị & chi phí hỗ trợ |
| Target SDK | Theo yêu cầu mới nhất của Google Play tại thời điểm submit (kiểm tra lại ở M10, chính sách Google Play cập nhật hằng năm) | Bắt buộc để app được duyệt |
| Kiến trúc | ARM64 + ARMv7 (chuẩn Unity IL2CPP) | Bắt buộc theo chính sách Google Play (64-bit) |
| Định dạng build | Android App Bundle (.aab) | Google Play yêu cầu AAB cho app mới |
| Scripting backend | IL2CPP | Bắt buộc cho 64-bit, hiệu năng tốt hơn Mono |
| Orientation | Portrait, khoá xoay | Đúng thiết kế chơi một tay |

## 2. Signing & bảo mật

- Keystore release **không bao giờ commit vào git**. Lưu ở nơi an toàn (password
  manager của Product Owner) hoặc secret store của CI.
- Nếu dùng CI để build, dùng biến môi trường/secret cho keystore path, alias,
  password — không hard-code trong file cấu hình build commit vào repo.
- Google Play App Signing: khuyến nghị bật (Google quản lý key ký cuối, giảm rủi ro
  mất keystore).

## 3. Versioning

- Semantic version cho `bundleVersion` (VD: `1.0.0`).
- `versionCode` tăng đơn điệu mỗi lần submit lên Play Console (kể cả internal
  testing), không bao giờ giảm hoặc lặp lại.

## 4. Store listing (chuẩn bị ở cuối M10, sau khi UI/art hoàn thiện)

- Icon, feature graphic, screenshot: **100% tự sản xuất** từ chính app, không dùng
  ảnh mẫu/stock có bản quyền không rõ nguồn, không dùng hình ảnh gợi liên tưởng tới
  sản phẩm khác.
- Mô tả app: viết mới hoàn toàn, không sao chép mô tả của app khác.
- Danh mục: Puzzle / Casual.
- Privacy Policy URL: bắt buộc (app có Ads + IAP). Cần Product Owner cung cấp domain
  để host trang chính sách, hoặc dùng dịch vụ tạo privacy policy rồi host tĩnh.
- Data Safety form: điền theo SDK thực tế tích hợp (AdMob, Unity IAP, Analytics
  provider) — thực hiện ở M10 khi đã chốt vendor.
- Content rating questionnaire: điền trung thực theo nội dung thực tế (không bạo
  lực, không nội dung người lớn — dự kiến rating thấp, phù hợp mọi lứa tuổi, nhưng
  quảng cáo có thể ảnh hưởng phân loại tuỳ nội dung ads network trả về).

## 5. Release channel strategy

1. **Internal testing**: nội bộ Product Owner + tester tin cậy, kiểm tra crash-free,
   luồng IAP sandbox, ads test ID.
2. **Closed testing** (alpha/beta nhỏ): mở rộng nhóm test, thu thập phản hồi UX, cân
   bằng economy.
3. **Production — staged rollout**: bắt đầu 5–10% người dùng, theo dõi crash rate/ANR
   qua Play Console Vitals, tăng dần lên 100% nếu số liệu ổn định.

## 6. Pre-launch checklist (trước khi submit production)

- [ ] Toàn bộ acceptance criteria M0–M9 đạt.
- [ ] Ad Unit ID thật (không phải test ID) được cấu hình đúng cho bản release.
- [ ] Sản phẩm IAP đã tạo và active trên Google Play Console, khớp Product ID trong
      code.
- [ ] Privacy Policy URL hoạt động, Data Safety form đã điền.
- [ ] Build ký bằng release keystore, kiểm tra chạy được trên thiết bị thật (không
      chỉ trong Editor).
- [ ] Kiểm tra `LICENSE_MANIFEST.md` — không còn asset chưa rõ nguồn/license.
- [ ] Kiểm tra tên app, package name, mô tả, hình ảnh không trùng/gây nhầm lẫn với
      sản phẩm thương mại khác.
- [ ] Rà soát chính sách Google Play mới nhất tại thời điểm submit (chính sách có
      thể thay đổi).

## 7. Rủi ro cần Product Owner quyết định trước M10

- Tên chính thức của app (ảnh hưởng trademark/branding — không đảo ngược sau khi
  đăng ký package name công khai).
- Domain lưu trữ Privacy Policy.
- Tài khoản Google Play Console Developer (cần Product Owner sở hữu, tôi không thể
  tự tạo tài khoản/thanh toán phí đăng ký nhà phát triển thay).
- Ad Unit ID / tài khoản AdMob thật, tài khoản merchant cho IAP — cần Product Owner
  tạo vì liên quan thanh toán/pháp lý.
