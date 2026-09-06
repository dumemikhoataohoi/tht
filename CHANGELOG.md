# Changelog

Định dạng dựa trên [Keep a Changelog](https://keepachangelog.com/), versioning theo
milestone cho tới khi có bản release chính thức (sau đó chuyển sang SemVer thật).

## [Unreleased]

### M0 — Project setup (đang thực hiện)

#### Added
- `PROJECT_PLAN.md`, `ARCHITECTURE.md`, `GAME_DESIGN.md`, `ECONOMY_DESIGN.md`,
  `MONETIZATION.md`, `QA_PLAN.md`, `RELEASE_PLAN.md`, `LICENSE_MANIFEST.md`,
  `CHANGELOG.md` — bộ tài liệu thiết kế/kế hoạch ban đầu.
- Cấu trúc thư mục Unity project skeleton (`Assets/_Project/...`, `Assets/Tests/...`,
  `Packages/manifest.json`, `.gitignore`, `README.md`).

#### Notes
- Chưa viết code gameplay — theo đúng yêu cầu chờ Product Owner xác nhận trước khi
  bắt đầu M1.
- Môi trường phát triển hiện tại không có Unity Editor cài sẵn — ghi nhận là blocker
  ảnh hưởng tới việc build/test thực tế (xem `QA_PLAN.md` §1 và `PROJECT_PLAN.md` §3).
