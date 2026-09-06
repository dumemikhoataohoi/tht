# GemGrid — Economy Design

## 1. Currencies

| Currency | Loại | Nguồn nhận | Dùng để |
|---|---|---|---|
| **Coin** | Soft/free | Điểm cuối ván, daily mission, achievement, quảng cáo rewarded, level up | Mua/nâng cấp power-up (giá rẻ), một phần cosmetic |
| **Gem** | Premium | IAP, achievement lớn, hiếm khi từ daily mission, milestone combo cao | Mua power-up cao cấp, gói power-up, mua nhanh khi hết Coin, cosmetic cao cấp |
| **Energy** | Đặc biệt (dự phòng) | Hồi theo thời gian, IAP, ad | Chỉ dùng cho mode giới hạn (ngoài scope v1.0) — Classic mode KHÔNG tốn Energy |

Nguyên tắc: **Classic mode luôn chơi được miễn phí, không giới hạn bởi Energy.**
Energy chỉ ảnh hưởng mode phụ tương lai, tránh việc chặn core loop bằng năng lượng
(giữ đúng yêu cầu "monetization không phá UX").

## 2. Sources & Sinks

**Coin — nguồn:**
- Quy đổi từ điểm cuối ván (công thức trong `EconomyConfig`, ví dụ 1 Coin / N điểm).
- Daily mission (2–3 mission/ngày).
- Achievement.
- Rewarded ad (nút "Xem quảng cáo nhận Coin" ở màn Game Over/Shop, chủ động bấm).
- Player level up (thưởng một lần).

**Coin — sink:**
- Mua power-up cấp thấp (Hammer, Shuffle giá Coin).
- Nâng cấp power-up bậc đầu.

**Gem — nguồn:**
- IAP (nguồn chính).
- Achievement mốc lớn, combo milestone hiếm.
- Starter Pack (một lần).

**Gem — sink:**
- Mua power-up cao cấp (Bomb, Undo, Double Score, Line Clear ở bậc cao).
- Mua power-up pack.
- Continue sau Game Over bằng Gem (**tuỳ chọn**, người chơi luôn có lựa chọn thay thế
  miễn phí là xem rewarded ad để continue — không bắt buộc trả phí).
- Bỏ qua thời gian chờ (không áp dụng ở Classic mode vì không có timer).

## 3. Power-up pricing (khởi điểm, cân bằng lại sau playtest)

| Power-up | Giá Coin (bậc 1) | Giá Gem (mua nhanh) | Ghi chú cân bằng |
|---|---|---|---|
| Hammer | 50 | 5 | Rẻ nhất, hiệu ứng nhỏ (1 ô) |
| Shuffle | 60 | 5 | Không phá ô, ít quyền lực nhất theo hướng "cứu ván" |
| Line Clear | 150 | 15 | Mạnh, giá cao hơn |
| Bomb | 180 | 18 | Mạnh, phạm vi rộng |
| Undo | 100 | 10 | Giá theo số bước hoàn tác cho phép |
| Double Score | 120 | 12 | Ảnh hưởng thời lượng, không ảnh hưởng khả năng sống sót — cân bằng an toàn |

Tất cả giá trị trên nằm trong `EconomyConfig`/`PowerUpConfig` (ScriptableObject),
**không hard-code trong code**, để tinh chỉnh sau khi có dữ liệu chơi thật.

## 4. Power-up upgrade

- Mỗi power-up có tối đa 3 bậc nâng cấp: bậc 1 (mặc định), bậc 2, bậc 3.
- Nâng cấp bằng Coin+Gem hỗn hợp ở bậc cao để giữ giá trị Gem.
- Hiệu ứng nâng cấp ví dụ: Undo bậc 2 hoàn tác 2 bước, Bomb bậc 2 tăng phạm vi,
  Double Score bậc 2 tăng thời lượng hiệu lực. Chi tiết số liệu để trong config, có
  thể tinh chỉnh không cần sửa code.

## 5. Daily Missions & Achievements (khung thưởng)

- Daily mission: thưởng nhỏ, chủ yếu Coin (30–80 Coin/mission), thỉnh thoảng power-up
  miễn phí, hiếm khi vài Gem.
- Achievement: thưởng một lần, tăng dần theo độ khó mốc, có thể gồm Gem ở mốc cao để
  tạo cảm giác thành tựu mà không tốn tiền thật.

## 6. Starter Pack (IAP) — nội dung tham khảo

- Một lượng Gem vừa phải + vài power-up mỗi loại + không bao gồm Remove Ads (bán
  riêng) — mục tiêu: giá trị tốt, mua một lần, không tạo cảm giác "phải mua mới chơi
  được" (giá/nội dung chính thức quyết định ở M6, sau khi có dữ liệu thị trường/giá
  Google Play theo khu vực).

## 7. Nguyên tắc chống Pay-to-Win / bảo vệ UX

- Power-up giúp **kéo dài** ván chơi, không đảm bảo **không bao giờ** thua — Game Over
  vẫn có thể xảy ra dù dùng hết power-up, giữ tính thử thách & công bằng.
- Không bán trực tiếp "điểm số" hay "thứ hạng" (không có leaderboard trả phí ở v1.0).
- Không giới hạn Classic mode bằng currency phải trả phí để mở khoá.
- Rewarded ad luôn là lựa chọn thay thế miễn phí cho các lợi ích tương đương IAP nhỏ
  (VD: continue, power-up miễn phí) — người chơi không bao giờ bị ép chỉ có một lựa
  chọn trả phí duy nhất cho một lợi ích.

## 8. Acceptance criteria — M4 (Economy)

- [ ] `IWalletService` cộng/trừ Coin/Gem đúng, không cho số âm.
- [ ] Giao dịch mua power-up kiểm tra đủ tiền trước khi trừ, thất bại không làm mất
      tiền/power-up.
- [ ] Toàn bộ số liệu giá/thưởng đọc từ ScriptableObject, thay đổi asset không cần
      sửa code.
- [ ] Unit test cho các case: đủ tiền, thiếu tiền, mua liên tiếp, nâng cấp power-up
      thay đổi đúng hiệu ứng theo bậc.
