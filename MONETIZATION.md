# GemGrid — Monetization Design

## 1. Nguyên tắc chung

FREE GAME + ADS + REWARDED ADS + IAP. Monetization không được phá vỡ trải nghiệm:
- Không interstitial trong lúc đang chơi (chỉ giữa các màn hình, ngoài gameplay).
- Không spam quảng cáo (frequency capping bắt buộc, cấu hình được).
- Rewarded ads **luôn chủ động** — người chơi bấm nút mới xem, không tự phát.

## 2. Ad placements

| Loại | Vị trí | Điều kiện hiển thị |
|---|---|---|
| Interstitial | Sau khi đóng màn Game Over, trước khi vào lại Main Menu/ván mới | Tối thiểu N phút hoặc M ván kể từ lần hiện trước (cấu hình trong `AdConfig`, khởi điểm đề xuất: tối đa 1 lần / 3 phút, không quá 1 lần / ván) |
| Interstitial | Khi rời game về Main Menu từ Settings/Shop (không phải giữa gameplay) | Cùng frequency cap như trên |
| Rewarded | Nút "Xem quảng cáo hồi sinh" ở màn Game Over (continue 1 lần/ván) | Chủ động, không giới hạn ngoài giới hạn tự nhiên 1 continue/ván |
| Rewarded | Nút "Xem quảng cáo nhận thưởng" ở Shop/Daily Mission (Coin/power-up miễn phí) | Chủ động, giới hạn số lần/ngày để tránh lạm dụng (cấu hình, ví dụ tối đa 5 lần/ngày) |

- Người chơi mua **Remove Ads** sẽ tắt Interstitial hoàn toàn; Rewarded ads vẫn hiển
  thị nút (vì đó là lựa chọn chủ động của người chơi, không phải quảng cáo ép buộc)
  nhưng có thể ẩn nếu Product Owner quyết định khác (không phải quyết định kỹ thuật
  nhỏ — sẽ hỏi khi tới M5 nếu cần).

## 3. Ad network / mediation

- **Lựa chọn mặc định**: Google AdMob (phổ biến nhất, tài liệu tốt, tương thích Google
  Play policy, hỗ trợ mediation nếu cần mở rộng sau này).
- Được trừu tượng hoá hoàn toàn qua `IAdsService` — có thể thay bằng mediation khác
  (LevelPlay, MAX...) sau này mà không ảnh hưởng Gameplay/UI.
- Đây là quyết định kỹ thuật triển khai (đứng sau interface, dễ đổi), không phải
  quyết định monetization chiến lược — chọn theo tiêu chí đơn giản/ổn định/ít
  dependency. Nếu Product Owner muốn network khác, có thể đổi ở M5 mà không ảnh hưởng
  kiến trúc.

## 4. IAP catalog (v1.0)

| Sản phẩm | Loại | Nội dung | Ghi chú |
|---|---|---|---|
| Gem Pack Small | Consumable | X Gem | Giá theo khu vực qua Google Play Console |
| Gem Pack Medium | Consumable | Y Gem (tỉ lệ giá/Gem tốt hơn Small) | |
| Gem Pack Large | Consumable | Z Gem (tỉ lệ tốt nhất) | |
| Power-up Pack | Consumable | Bộ power-up hỗn hợp (VD: 3 Bomb + 3 Hammer + 3 Shuffle) | |
| Starter Pack | Consumable, one-time (ẩn sau khi mua) | Gem + power-up cơ bản, giá trị tốt | Xem `ECONOMY_DESIGN.md` §6 |
| Remove Ads | Non-consumable | Tắt Interstitial vĩnh viễn | Không ảnh hưởng Rewarded (người chơi vẫn có thể chủ động xem để nhận thưởng) |

Giá cụ thể để trống ở giai đoạn thiết kế, quyết định ở M6 khi cấu hình Google Play
Console (theo từng khu vực, tuân thủ chính sách giá của Google Play).

## 5. IAP flow

- `IIAPService` khởi tạo qua Unity IAP (Google Play Billing).
- Mọi purchase phải qua `IPurchaseFulfiller` để cộng thưởng — tách khỏi luồng UI mua
  hàng, dễ test bằng mock, tránh mất reward nếu app bị đóng giữa chừng (lưu pending
  transaction, xử lý lại khi mở app — chi tiết kỹ thuật ở M6).
- Restore Purchases bắt buộc có cho Non-consumable (Remove Ads).

## 6. Compliance

- App có Ads + IAP → bắt buộc có **Privacy Policy URL** khi nộp Google Play.
- Điền **Data Safety form** đầy đủ theo SDK thực tế dùng (AdMob, Unity IAP,
  Analytics) khi tới M10.
- Tuân thủ chính sách quảng cáo Google Play (không ads gây hiểu nhầm, không ads che
  nút quan trọng, có nút đóng rõ ràng, không ads nhắm trẻ em nếu app không khai báo
  đối tượng trẻ em — quyết định phân loại đối tượng để ở M10).

## 7. Acceptance criteria — M5 (Ads) & M6 (IAP)

**M5:**
- [ ] Interstitial không bao giờ hiện trong lúc `GameSessionController.State == Playing`.
- [ ] Frequency cap hoạt động đúng theo config (test bằng mock clock).
- [ ] Rewarded ad chỉ trigger qua hành động bấm nút của người chơi, không tự động.
- [ ] `IAdsService` có mock/no-op implementation dùng được khi test không có mạng.

**M6:**
- [ ] Mua hàng thành công cộng đúng reward, không cộng trùng khi restart giữa chừng.
- [ ] Restore Purchases khôi phục đúng Remove Ads.
- [ ] Toàn bộ giao dịch có log qua `IAnalyticsService`.
