# GemGrid — License Manifest

## 1. Chính sách

Mọi asset (art, audio, font, code library bên thứ ba) đưa vào project **bắt buộc**
phải có một dòng trong bảng dưới đây trước khi merge, ghi rõ nguồn và quyền sử dụng
thương mại. Không có ngoại lệ. Asset không có entry hợp lệ ở đây coi như **không được
phép** đưa vào project.

Nghiêm cấm tuyệt đối (theo yêu cầu IP của dự án):
- Bất kỳ asset nào sao chép/phái sinh từ Block Blast!, Candy Crush, Tetris, hoặc bất
  kỳ sản phẩm thương mại nào khác (tên, logo, nhân vật, artwork, UI, animation, âm
  thanh, nhạc, font riêng, screenshot, text, level data, branding, store listing).
- Asset không rõ nguồn gốc/license ("tìm thấy trên mạng" không phải là license hợp lệ).

## 2. Bảng license asset

| # | Asset | Mô tả | Nguồn | License | Cho phép dùng thương mại | Ghi chú |
|---|---|---|---|---|---|---|
| _(chưa có asset nào ở M0 — dự án chỉ mới thiết lập cấu trúc project)_ | | | | | | |

> Bảng sẽ được cập nhật liên tục từ M2 (UI/Art) trở đi khi bắt đầu thêm sprite, âm
> thanh, font vào project.

## 3. Engine & package dependencies

| # | Thành phần | License | Nguồn | Ghi chú |
|---|---|---|---|---|
| 1 | Unity Editor & Runtime (2022.3 LTS) | Unity EULA (Personal/Plus/Pro tuỳ doanh thu) | unity.com | Product Owner cần xác nhận tier Unity phù hợp doanh thu dự kiến trước khi phát hành thương mại |
| 2 | Unity Test Framework | Unity Companion License | Unity Package Manager | Dùng cho unit/integration test |
| 3 | TextMeshPro | Unity Companion License (bundled) | Unity Package Manager | Hiển thị text UI |
| 4 | Unity IAP | Unity Companion License | Unity Package Manager | Tích hợp Google Play Billing |
| 5 | Google AdMob Unity Mediation Plugin | Apache 2.0 (SDK Google) | Google (thêm vào ở M5) | Sẽ thêm entry chi tiết + version cụ thể khi tích hợp ở M5 |

## 4. Quy trình thêm asset mới

1. Xác định nguồn: tự sản xuất (in-house) hoặc bên thứ ba có license thương mại rõ
   ràng (VD: Google Fonts – OFL, thư viện âm thanh có giấy phép Royalty-Free/CC0/
   Commercial License mua có hoá đơn).
2. Thêm dòng vào bảng mục 2 ngay khi thêm file vào `Assets/`.
3. Nếu license yêu cầu credit/attribution, ghi rõ nội dung attribution cần hiển thị
   (VD: trong màn Credits/Settings) ở cột Ghi chú.
4. Không merge nếu thiếu bất kỳ thông tin bắt buộc nào (Nguồn, License, Cho phép
   thương mại).

## 5. Trademark & tên gọi

- "GemGrid" là tên làm việc tạm thời. Trước khi công bố công khai/đăng ký trên
  Google Play, cần tra cứu trùng lặp thương hiệu (Google Play Store search, USPTO/
  cơ quan sở hữu trí tuệ liên quan) — việc này thuộc quyết định của Product Owner
  (ảnh hưởng pháp lý), không phải quyết định kỹ thuật tự động.
