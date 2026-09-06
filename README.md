# GemGrid (working title)

Mobile casual puzzle game (Android) — grid 8x8 line-clear, original mechanics/art/audio.
Xem toàn bộ tài liệu thiết kế & kế hoạch:

- [`PROJECT_PLAN.md`](PROJECT_PLAN.md) — kế hoạch tổng thể, tech stack, milestone.
- [`ARCHITECTURE.md`](ARCHITECTURE.md) — kiến trúc code, module boundary, interfaces.
- [`GAME_DESIGN.md`](GAME_DESIGN.md) — Game Design Document.
- [`ECONOMY_DESIGN.md`](ECONOMY_DESIGN.md) — thiết kế kinh tế trong game.
- [`MONETIZATION.md`](MONETIZATION.md) — chiến lược Ads/IAP.
- [`QA_PLAN.md`](QA_PLAN.md) — kế hoạch kiểm thử.
- [`RELEASE_PLAN.md`](RELEASE_PLAN.md) — kế hoạch build & phát hành Google Play.
- [`LICENSE_MANIFEST.md`](LICENSE_MANIFEST.md) — theo dõi license mọi asset bên thứ ba.
- [`CHANGELOG.md`](CHANGELOG.md) — lịch sử thay đổi theo milestone.

## Yêu cầu để mở project

- **Unity 2022.3 LTS** (khuyến nghị bản mới nhất trong nhánh 2022.3, xem
  `ProjectSettings/ProjectVersion.txt` cho version tham chiếu ban đầu).
- Android Build Support module (bao gồm Android SDK & NDK, OpenJDK) cài qua Unity Hub.

> Project hiện tại là **skeleton** (M0): chỉ có cấu trúc thư mục, `Packages/manifest.json`,
> và tài liệu — chưa có code gameplay/scene/asset. Khi mở lần đầu bằng Unity Editor,
> Unity sẽ tự sinh các file `.meta` và phần còn lại của `ProjectSettings/` cần thiết.
> Đây là hành vi bình thường của Unity với một project mới từ git.

## Chạy test (khi có Unity Editor)

Mở **Window → General → Test Runner** trong Unity Editor, chọn tab **EditMode**, bấm
**Run All**. Hoặc chạy headless qua CLI:

```bash
Unity -batchmode -projectPath . -runTests -testPlatform EditMode \
      -testResults ./TestResults.xml -logFile ./test.log -quit
```

## Trạng thái hiện tại

Xem `PROJECT_PLAN.md` §5 (Milestone roadmap) và `CHANGELOG.md` cho tiến độ mới nhất.

## Bản quyền / IP

Dự án tuân thủ nghiêm ngặt chính sách không sao chép asset/branding/mechanic cụ thể
từ bất kỳ sản phẩm thương mại nào (Block Blast!, Candy Crush, Tetris, v.v.). Mọi asset
bên thứ ba phải được ghi nhận trong `LICENSE_MANIFEST.md` trước khi sử dụng.
