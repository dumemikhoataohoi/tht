# GemGrid — Architecture

## 1. Nguyên tắc thiết kế

- **Separation of concerns**: Gameplay, UI, Economy, Save, Ads, IAP, Analytics, Audio,
  Configuration là các module độc lập, giao tiếp qua interface.
- **Data-driven**: mọi số liệu tuning (block shapes, giá power-up, tần suất ads, reward
  daily mission...) nằm trong ScriptableObject assets, không hard-code.
- **Composition root duy nhất**: `GameBootstrap` (chạy ở scene `Boot`) khởi tạo mọi
  service theo interface và inject vào nơi cần dùng qua constructor/method injection
  đơn giản (không dùng framework DI ngoài để giảm dependency — tự viết
  `ServiceLocator` nhẹ, chỉ dùng nội bộ, không public tràn lan).
- **Testability**: logic gameplay thuần (grid, clear detection, scoring) tách khỏi
  MonoBehaviour để test được bằng NUnit thông thường (EditMode), không cần chạy scene.
- **Ít dependency ngoài nhất có thể**: chỉ thêm package khi thật sự cần (Unity IAP,
  TextMeshPro, Unity Test Framework, Google AdMob Unity Mediation).

## 2. Assembly Definitions (asmdef)

Để ép ranh giới module ở compile-time và tăng tốc compile:

```
Assets/_Project/Scripts/Core/            -> GemGrid.Core.asmdef
Assets/_Project/Scripts/Configuration/   -> GemGrid.Configuration.asmdef  (deps: Core)
Assets/_Project/Scripts/Gameplay/        -> GemGrid.Gameplay.asmdef      (deps: Core, Configuration)
Assets/_Project/Scripts/Economy/         -> GemGrid.Economy.asmdef       (deps: Core, Configuration)
Assets/_Project/Scripts/Save/            -> GemGrid.Save.asmdef          (deps: Core)
Assets/_Project/Scripts/Ads/             -> GemGrid.Ads.asmdef           (deps: Core)
Assets/_Project/Scripts/IAP/             -> GemGrid.IAP.asmdef           (deps: Core, Economy)
Assets/_Project/Scripts/Analytics/       -> GemGrid.Analytics.asmdef     (deps: Core)
Assets/_Project/Scripts/Audio/           -> GemGrid.Audio.asmdef         (deps: Core, Gameplay — AudioHookListener subscribes to GameManagerBehaviour's events)
Assets/_Project/Scripts/UI/              -> GemGrid.UI.asmdef            (deps: Core, Gameplay, Economy)
Assets/_Project/Scripts/Bootstrap/       -> GemGrid.Bootstrap.asmdef     (deps: tất cả — composition root)
Assets/Tests/EditMode/                   -> GemGrid.Tests.EditMode.asmdef (deps: tương ứng module test)
```

Quy tắc: **Gameplay không được reference UI, Ads, IAP, Analytics**. UI chỉ gọi vào
Gameplay/Economy qua interface + event, không ngược lại.

> **Cập nhật sau M1**: các MonoBehaviour/ScriptableObject adapter (cần UnityEngine)
> nằm cạnh logic thuần trong cùng assembly để giữ đúng ranh giới module, nhưng được
> tách vào file/thư mục riêng để không lẫn với phần đã unit-test được:
> `Assets/_Project/Scripts/Gameplay/View/` chứa `GameManagerBehaviour` (composition
> root của scene gameplay — không phải `Bootstrap/`, vốn dành cho entry point cấp
> app ở milestone sau), `GridController`, `BlockDragController`,
> `HapticHookListener`, `GameplayAnimationHooks`. Toàn bộ phần còn lại của
> `Gameplay.asmdef` (`GridModel`, `GameManager`, `ScoreManager`, `ComboManager`,
> `BlockSpawner`, `GameOverChecker`, `BlockPlacement`...) là C# thuần, không
> `using UnityEngine`, để có thể biên dịch + chạy unit test cả trong lẫn ngoài Unity
> Editor (xem README_M1.md).

## 3. Module map & interfaces chính

| Module | Trách nhiệm | Interface tiêu biểu |
|---|---|---|
| Configuration | Load ScriptableObject config (block shapes, economy table, power-up table, ad frequency) | `IConfigProvider` |
| Gameplay | Grid model, block spawn, placement validation, line/column clear, combo, score, game-over detection | `IGridModel`, `IBlockSpawner`, `IScoreCalculator`, `IGameSessionController` |
| Economy | Ví Coin/Gem/Energy, power-up inventory, giao dịch (earn/spend), validate đủ tiền | `IWalletService`, `IPowerUpInventory` |
| Save | Đọc/ghi trạng thái người chơi (wallet, progression, settings) dạng JSON local | `ISaveService` |
| Ads | Hiển thị Interstitial/Rewarded, frequency capping, callback kết quả rewarded | `IAdsService` |
| IAP | Khởi tạo store, mua hàng, khôi phục giao dịch, map product → reward | `IIAPService`, `IPurchaseFulfiller` |
| Analytics | Ghi nhận event chuẩn hoá (game_start, game_over, purchase, ad_watched...) | `IAnalyticsService` |
| Audio | Phát SFX/nhạc nền, mute/volume theo settings | `IAudioService` |
| UI | Màn hình (Main Menu, HUD, Game Over, Shop, Settings), điều hướng | `IScreen`, `IUINavigator` |

Tất cả implementation cụ thể (VD: `AdMobAdsService : IAdsService`,
`JsonFileSaveService : ISaveService`) nằm trong cùng module nhưng file riêng, để có
thể thay bằng bản mock/no-op khi test hoặc khi đổi vendor.

## 4. Cấu trúc thư mục Unity

```
Assets/
  _Project/
    Scripts/
      Core/            (event bus, service locator nhẹ, base types, utils)
      Configuration/   (ScriptableObject definitions + loader)
      Gameplay/        (grid, block, spawner, session controller, combo, scoring)
      Economy/         (wallet, power-up inventory, transaction log)
      Save/            (save schema, JSON serializer, save service)
      Ads/             (ads service interface + AdMob implementation + no-op mock)
      IAP/             (IAP service, product catalog, purchase fulfillment)
      Analytics/       (event schema, analytics service + no-op implementation)
      Audio/           (audio service, sound bank)
      UI/              (screens, HUD, popups, navigator)
      Bootstrap/       (GameBootstrap - composition root)
    Art/
      Sprites/
      UI/
    Audio/
      Music/
      SFX/
    Prefabs/
    Scenes/            (Boot, MainMenu, Gameplay)
    ScriptableObjects/ (data assets: BlockShapeSet, EconomyConfig, PowerUpConfig, AdConfig, MissionConfig)
    Resources/         (chỉ đặt asset thật sự cần load runtime bằng Resources.Load)
  Tests/
    EditMode/
    PlayMode/
Packages/
  manifest.json
ProjectSettings/
```

Lý do đặt mọi thứ trong `_Project/` (dấu gạch dưới để nổi lên đầu danh sách): tách
bạch rõ asset của game với asset của package bên thứ ba nằm trong `Assets/Plugins`
hoặc `Assets/ThirdParty` (nếu có), tránh trộn lẫn khi audit license.

## 5. Data flow (vòng lặp gameplay chính)

```
Input (drag) -> UI (BlockDragHandler)
             -> Gameplay.IGridModel.CanPlace(shape, position)
             -> nếu hợp lệ: IGridModel.Place(...)
                 -> phát event GridChanged
                 -> IGridModel kiểm tra hàng/cột đầy -> ClearLines(...)
                 -> IScoreCalculator.OnLinesCleared(count, comboState) -> điểm + combo
                 -> IWalletService nhận coin thưởng (nếu có) qua event, KHÔNG gọi trực tiếp
             -> IBlockSpawner kiểm tra khay hết block -> sinh khay mới theo
                IConfigProvider (trọng số shape tăng dần độ khó theo score)
             -> Gameplay kiểm tra "còn nước đi hợp lệ" cho mọi shape trong khay hiện tại
                -> nếu không còn -> GameOver event
UI lắng nghe event (GridChanged, LinesCleared, ComboChanged, GameOver) để cập nhật
hiển thị — UI không chứa logic tính toán.
```

Giao tiếp giữa module dùng **event/message nội bộ** (C# event hoặc EventBus đơn giản
trong `Core`), không dùng `UnityEvent` public tràn lan trên Inspector cho logic quan
trọng (khó test, dễ đứt liên kết khi refactor).

## 6. State machine

`GameSessionController` quản lý state: `Idle -> Playing -> Paused -> GameOver`.
Power-up "Undo" lưu snapshot state grid gần nhất (giới hạn N bước) để hoàn tác.

## 7. Testing strategy

- **EditMode/NUnit**: test thuần logic không cần scene — grid placement rules, line
  clear detection, scoring formula, combo escalation, economy transaction (đủ/thiếu
  tiền), save schema serialize/deserialize, power-up effect logic.
- **PlayMode**: test tương tác giữa GameObject thực (input → grid → UI), chạy khi có
  Unity Editor.
- Toàn bộ implementation Ads/IAP/Analytics dùng interface → test Gameplay/Economy
  bằng mock, không phụ thuộc mạng hoặc store thật.

## 8. Coding conventions

- Namespace theo module: `GemGrid.Gameplay`, `GemGrid.Economy`, v.v.
- `PascalCase` cho class/public member, `camelCase` cho private field (prefix `_`),
  `UPPER_SNAKE_CASE` chỉ cho hằng số thật sự bất biến.
- Không dùng `FindObjectOfType`/`GameObject.Find` trong logic gameplay (chỉ chấp nhận
  trong bootstrap wiring nếu thật cần thiết).
- Không comment giải thích "cái gì" (tên đã rõ); chỉ comment khi có lý do "tại sao"
  không hiển nhiên.
