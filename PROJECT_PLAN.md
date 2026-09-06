# GemGrid — Project Plan

Working title: **GemGrid** (tên tạm thời, có thể đổi trước khi release vì lý do trademark).

## 1. Mục tiêu dự án

Xây dựng một mobile casual puzzle game 2D cho Android, thể loại "grid line-clear",
với lối chơi nguyên bản (original mechanics ở mức ý tưởng chung, không sao chép bất kỳ
sản phẩm thương mại nào), monetization cân bằng (Ads + Rewarded Ads + IAP) và
architecture đủ sạch để mở rộng lâu dài (progression, events, thêm mode...).

## 2. Phạm vi (Scope)

**Trong phạm vi (v1.0):**
- Grid 8x8, kéo-thả block, clear hàng/cột, combo, score, game over detection.
- Progression: combo level, power-up upgrade, daily missions, achievements, player level.
- Economy: Coin, Gem, (Energy để dành cho mode đặc biệt tương lai — không bắt buộc ở v1.0).
- Power-up: Hammer, Bomb, Shuffle, Undo, Line Clear, Double Score.
- Monetization: Interstitial Ads (ngoài gameplay), Rewarded Ads (opt-in), IAP (Gems, power-up
  packs, Starter Pack, Remove Ads).
- Save/local persistence, Analytics (event tracking cơ bản), Audio (SFX + music, gốc hoặc licensed).
- Android build pipeline (AAB) sẵn sàng nộp Google Play (internal testing track).

**Ngoài phạm vi (v1.0, có thể làm sau):**
- Leaderboard/social, live-ops events, multiplayer, iOS build, mode Energy đầy đủ.

## 3. Tech stack

| Hạng mục | Lựa chọn | Lý do |
|---|---|---|
| Engine | Unity **2022.3 LTS** | Ổn định, hỗ trợ Android tốt, LTS dài hạn |
| Ngôn ngữ | C# | Chuẩn Unity |
| Render pipeline | Built-in 2D (Sprite Renderer + URP tuỳ chọn sau) | Đơn giản, ít dependency, phù hợp casual mobile |
| Ads | Google AdMob (qua interface `IAdsService`, có thể đổi mediation sau) | Phổ biến nhất, tài liệu tốt, tương thích Play policy |
| IAP | Unity IAP (qua interface `IIAPService`) | Tích hợp sẵn với Unity, hỗ trợ Google Play Billing |
| Save | JSON local file (qua interface `ISaveService`) | Đơn giản, không cần backend ở v1.0 |
| Analytics | Interface `IAnalyticsService`, implementation ban đầu là local-log/no-op, gắn Firebase Analytics sau nếu cần | Không khoá cứng vendor |
| Testing | Unity Test Framework (NUnit) — EditMode cho pure logic | Chạy được headless/CI |
| VCS | Git | Theo yêu cầu |

> **Lưu ý môi trường thực thi hiện tại:** Sandbox này KHÔNG có Unity Editor cài sẵn.
> Tôi sẽ viết toàn bộ code, cấu hình, test theo đúng chuẩn Unity, nhưng việc build
> APK/AAB thực tế, chạy Play Mode, và test trên thiết bị Android cần được thực hiện
> khi project được mở bằng Unity Editor (Unity Hub) trên máy có GUI, hoặc qua Unity
> Cloud Build / self-hosted CI có Unity license. Việc này được ghi rõ ở mục Blockers
> trong báo cáo trạng thái.

## 4. Nguyên tắc kỹ thuật (bắt buộc)

- Không God Class, không viết toàn bộ logic trong 1 file.
- Tách rõ module: Gameplay / UI / Economy / Save / Ads / IAP / Analytics / Audio / Configuration.
- Mỗi module giao tiếp qua interface (`IXxxService`), không phụ thuộc trực tiếp vào
  implementation cụ thể → dễ test, dễ thay vendor.
- Dữ liệu cấu hình (số liệu economy, power-up cost, block shapes, ad frequency...) đặt
  trong ScriptableObject, không hard-code trong code logic.
- Composition root duy nhất (`GameBootstrap`) khởi tạo và "wire" các service — tránh
  singleton tràn lan.

## 5. Milestone roadmap

| # | Milestone | Nội dung chính | Trạng thái |
|---|---|---|---|
| M0 | Project setup | Unity project skeleton, folder structure, git config, package manifest, docs | 🔵 Đang thực hiện |
| M1 | Core gameplay | Grid model, block spawn, drag&drop, line/column clear, score, combo, game over | ⬜ Chờ xác nhận |
| M2 | UI | Main menu, HUD gameplay, game over screen, settings, original UI style | ⬜ |
| M3 | Progression | Combo level, player level, daily missions, achievements | ⬜ |
| M4 | Economy | Coin/Gem/Energy, power-up cost & upgrade, ScriptableObject data | ⬜ |
| M5 | Ads | AdMob interstitial + rewarded, frequency capping, opt-in rewarded flow | ⬜ |
| M6 | IAP | Unity IAP, Gem packs, Starter Pack, Remove Ads | ⬜ |
| M7 | Save | Local JSON save/load, migration-safe schema | ⬜ |
| M8 | Analytics | Event schema, service interface, no-op/local implementation | ⬜ |
| M9 | QA | Full regression, performance pass, device matrix checklist | ⬜ |
| M10 | Android release | Build signing, AAB, store listing prep, staged rollout plan | ⬜ |

Mỗi milestone: build (nếu có thể trong sandbox) → test → sửa lỗi → cập nhật
`ARCHITECTURE.md`/design docs liên quan → cập nhật `CHANGELOG.md`. Không chuyển
milestone tiếp theo nếu chưa đạt acceptance criteria (xem cuối `QA_PLAN.md` và
từng mục milestone tương ứng trong các doc thiết kế).

## 6. Vai trò

Toàn bộ vai trò Lead Developer / Game Designer / Technical Director / QA / Release
Engineer do Claude đảm nhiệm trong phiên làm việc này. Product Owner (bạn) ra quyết
định ở các điểm rẽ nhánh lớn (gameplay, monetization, legal/IP, architecture, cost,
release strategy); các quyết định kỹ thuật nhỏ được tự quyết theo tiêu chí: đơn giản,
ổn định, dễ bảo trì, phù hợp mobile, ít dependency.

## 7. Định nghĩa "Done" cho một milestone

1. Code compile không lỗi (kiểm tra bằng cấu trúc/cú pháp chuẩn C#/Unity; build binary
   thực tế cần Unity Editor như đã nêu ở mục 3).
2. Unit test cho logic thuộc milestone đó pass (EditMode, chạy được qua Unity Test
   Runner khi có Unity Editor).
3. Tài liệu liên quan được cập nhật.
4. `CHANGELOG.md` được cập nhật.
5. Acceptance criteria của milestone được liệt kê rõ trong `GAME_DESIGN.md` /
   `ARCHITECTURE.md` / `QA_PLAN.md` đều thoả mãn.
