# GemGrid — Game Design Document V2 (Meta-progression & Fair Monetization)

**Trạng thái**: Thiết kế, chưa code. Tài liệu này KHÔNG thay đổi core grid/block/score
logic đã implement và verify (M1–M3.x) — nó thiết kế lớp **meta-loop** (level, độ khó,
progression, reward, monetization) bọc quanh engine hiện có, để chuẩn bị cho các
milestone code tiếp theo (không phải milestone này).

## 0. Vì sao cần V2

`GAME_DESIGN.md` (v1) mô tả đúng **engine** (grid 8x8, tray 3 slot, drag/drop, clear,
combo, game-over detection) — phần này đã code xong, đã test, **không đổi**. Nhưng v1
coi "Classic mode, chơi vô hạn tới khi thua" là chế độ duy nhất. Chế độ vô hạn có vấn đề
với đúng các mục tiêu Product Owner đặt ra cho V2:

| Mục tiêu V2 | Vấn đề nếu chỉ có Classic vô hạn |
|---|---|
| 2. Càng chơi càng khó | Độ khó chỉ tự nhiên tăng do board đầy dần — không có đường cong khó thiết kế được |
| 3. Cảm giác chinh phục | Không có "đích" nào để chinh phục — chỉ có "chưa thua" |
| 4. Muốn thử lại khi thất bại | Thua ở ván vô hạn = mất hết tiến độ điểm, không có mục tiêu ngắn hạn cụ thể để "thử lại cho đạt" |
| 5. Người chơi free vẫn hoàn thành được game | Không thể "hoàn thành" một chế độ vô hạn |

**Giải pháp**: giữ nguyên engine, thêm **Journey Mode** (chuỗi level có mục tiêu, có đầu
có cuối, độ khó tăng dần thiết kế được) làm chế độ chính. **Classic Mode** (vô hạn, như
hiện tại) được giữ lại làm chế độ phụ, luôn mở, không giới hạn — đây chính là chế độ M1–M3
đã build, không bỏ đi bất kỳ dòng code nào.

## 1. Design Pillars (7 mục tiêu → 7 cơ chế cụ thể)

| # | Mục tiêu | Cơ chế đảm bảo (chi tiết ở các mục dưới) |
|---|---|---|
| 1 | Dễ hiểu khi bắt đầu | Level 1–3 là "tutorial ẩn" — mục tiêu cực đơn giản (đặt hết khay, clear 1 hàng), tray chỉ chứa shape nhỏ 1–3 ô, không combo yêu cầu. Tutorial overlay (đã code) dạy luật; level đầu dạy "áp dụng luật" |
| 2 | Càng chơi càng khó | `DifficultyCurveConfig` tăng dần: target điểm/hàng, tỉ trọng shape phức tạp, thỉnh thoảng thêm ô chướng ngại — xem §6 |
| 3 | Cảm giác chinh phục | Mỗi level có mục tiêu rõ + hệ sao (1–3★) + level map trực quan (chinh phục từng chặng) — xem §5, §13 |
| 4 | Muốn thử lại khi thất bại | Thất bại luôn hiển thị **rõ vì sao** + **gần đạt bao nhiêu %** + retry ngay lập tức, miễn phí, không phạt nặng — xem §10, §13 |
| 5 | Free chơi xong được cả game | Mọi level có "zero-spend clear path" bắt buộc (quy tắc thiết kế, không phải lời hứa marketing) — xem §17 |
| 6 | Trả phí chỉ giúp dễ hơn | Mọi power-up/energy/continue trả phí đều có bản thay thế miễn phí tương đương (ad hoặc chờ) — kế thừa nguyên tắc đã có ở `MONETIZATION.md` v1, áp dụng cho toàn bộ mục dưới |
| 7 | Không tạo độ khó giả để ép trả tiền | Cấm dynamic difficulty phản ứng theo hành vi mua hàng; độ khó chỉ đến từ config tĩnh, test được — xem §18 |

## 2. Core Loop (không đổi, tái sử dụng nguyên engine M1–M3)

1. Grid 8x8, tray 3 block ngẫu nhiên (trọng số theo `BlockShapeSet` + `DifficultyCurveConfig`).
2. Kéo block vào grid — preview ghost xanh/đỏ (đã code — `PlacementPreviewController`).
3. Đặt hợp lệ → cộng điểm, hàng/cột đầy → clear + combo (công thức `ScoreRules`/`ComboRules`
   hiện tại, **không đổi**).
4. Hết khay → sinh khay mới.
5. Không còn nước đi hợp lệ cho bất kỳ shape nào trong khay → kết thúc lượt chơi
   (`GameOverChecker`, không đổi).
6. **Mới ở V2**: bước 5 giờ được diễn giải theo **mode** đang chơi:
   - **Journey**: kết thúc lượt = thất bại nếu chưa đạt mục tiêu level, hoặc thành công nếu
     mục tiêu đã đạt trước khi hết nước đi.
   - **Classic**: kết thúc lượt = Game Over như hiện tại, ghi điểm cao nhất, không có khái
     niệm "thành công/thất bại".

Không có thay đổi nào ở bước 1–5 cần sửa `GridModel`/`GameManager`/`ScoreManager`/
`ComboManager` — engine hiện có đã đủ. V2 chỉ cần một lớp mới quan sát các event có sẵn
(`BlockPlaced`, `LinesClearedEvent`, `GameOver`, `StateChanged`) để biết khi nào mục tiêu
level đã đạt, tương tự cách `AudioHookListener`/`GameplayHud` đã làm — đây là ghi chú kiến
trúc cho milestone code sau, **không code ở task này**.

## 3. Game Structure — 2 mode

| | Journey Mode (mới, chính) | Classic Mode (hiện có, phụ) |
|---|---|---|
| Mục tiêu | Hoàn thành chuỗi level, mở khoá chương mới | Không giới hạn, chơi tự do lấy điểm cao |
| Board | Reset mới mỗi level | Reset mới mỗi ván |
| Kết thúc | Đạt mục tiêu = thắng; hết nước đi trước khi đạt = thua | Hết nước đi = Game Over (như hiện tại) |
| Tốn gì để chơi | Gem Energy (§8) | **Không tốn gì** — luôn free, luôn mở, y hệt hiện tại |
| Có thể "hoàn thành game" | Có — hết chương cuối = hoàn thành | Không — vô hạn theo thiết kế |
| Vai trò trong V2 | Trải nghiệm chính, nơi diễn ra difficulty curve/reward/progression | Sân chơi luyện tập + chế độ dự phòng khi hết Energy — không bao giờ khoá |

Đây là lý do §0 nói "không bỏ dòng code nào": Classic Mode = đúng flow Boot→MainMenu→
Gameplay→GameOver hiện tại, không đổi. Journey Mode là một **luồng bổ sung**, dùng chung
`GridController`/`BlockDragController`/`GameplayHud`/`PlacementPreviewController` v.v.,
chỉ thêm object mới ở tầng cao hơn (level definition, objective tracker, level-select map).

## 4. Onboarding — 3 level đầu tiên

- **Level 1**: mục tiêu "Đặt hết khay" (không cần clear hàng nào) — không thể thua trừ khi
  cố tình rời app. Dạy thao tác kéo/thả.
- **Level 2**: mục tiêu "Clear 1 hàng hoặc cột" — tray chỉ gồm shape ≤3 ô, gần như chắc
  chắn thành công nếu làm theo preview.
- **Level 3**: mục tiêu "Đạt 100 điểm" (đạt được chỉ bằng 2–3 lần clear thường) — lần đầu
  người chơi tự lên chiến thuật nhẹ, không cần combo.
- Từ Level 4 trở đi mới vào `DifficultyCurveConfig` thật (§6).
- 3 level này **không tốn Gem Energy** (né mọi khả năng người chơi mới bị "khoá" trước khi
  hiểu game — vi phạm mục tiêu 1 nếu tốn).

## 5. Level Structure

- Đơn vị tổ chức: **Chapter** (chương) gồm 15 level. Chapter có tên/theme màu riêng (không
  đổi mechanic, chỉ đổi palette nền — tái dùng `GemPalette`).
- Mỗi **Level** = 1 bộ dữ liệu (`LevelDefinition`, tương lai là ScriptableObject, theo đúng
  nguyên tắc "config-driven, không hard-code" đã áp dụng toàn dự án):
  - `LevelId`, `ChapterId`
  - 1–2 **Objective** (§7) — level dễ chỉ có 1 mục tiêu, level khó có thể có mục tiêu phụ
    tuỳ chọn (không bắt buộc, chỉ để đạt 3★)
  - `ShapeWeightOverride` (tuỳ chọn) — ghi đè trọng số shape mặc định của chapter đó
  - `MoveBudget` (tuỳ chọn, mặc định **không giới hạn nước đi** — độ khó đến từ mục tiêu +
    độ phức tạp shape, không đến từ đếm ngược nước đi giả tạo, đúng mục tiêu 7)
  - `StarThresholds` — điều kiện đạt 2★/3★ (ví dụ: 2★ = đạt mục tiêu + còn ≥1 slot trống
    lớn trên board; 3★ = đạt mục tiêu + đạt combo ≥2 ít nhất 1 lần trong level đó)
- Level map hiển thị dạng đường đi (path) qua từng chapter, node đã qua sáng, node hiện
  tại có thể chơi, node sau bị khoá tới khi qua node trước — tạo cảm giác chinh phục theo
  từng bước (mục tiêu 3), không phải mở khoá bằng tiền (mở khoá chỉ bằng chơi qua).

## 6. Difficulty Curve

Độ khó tăng qua **4 đòn bẩy**, tất cả nằm trong config (không hard-code), tất cả **tĩnh**
(không phản ứng theo hành vi mua hàng — xem §18):

1. **Objective target tăng dần**: ví dụ target điểm level 4 = 150 → level 30 ≈ 900 (đường
   cong tăng chậm dần, không tuyến tính — tăng nhanh ở đầu để tạo cảm giác tiến bộ rõ, tăng
   chậm ở cuối để không nhảy vọt).
2. **Trọng số shape phức tạp tăng dần**: đầu game ưu tiên shape 1–3 ô (dễ đặt); qua mỗi
   chapter, tỉ trọng shape 4–5 ô (L, T, S, chữ nhật lớn) tăng dần. Không bao giờ loại bỏ
   hoàn toàn shape nhỏ (luôn còn đường giải hợp lý — mục tiêu 7).
3. **Mật độ "near-full" ban đầu** (tuỳ chọn, từ chapter 3+): một số level bắt đầu với vài ô
   đã chiếm sẵn (không phải ô "chướng ngại vĩnh viễn" — chỉ là ô thường, clear được bình
   thường) để tạo cảm giác gấp rút ngay từ đầu, không phải tăng độ khó bằng cách chặn ô
   không bao giờ clear được (chặn ô vĩnh viễn = "độ khó giả", vi phạm mục tiêu 7 → **không
   dùng**).
4. **Mục tiêu phụ (bonus objective) cho 3★**: không bắt buộc để qua level, chỉ để lấy sao —
   đây là nơi độ khó "thật sự cao" nằm, dành cho người chơi muốn thử thách, không chặn
   người chơi thường (giữ đúng mục tiêu 1 & 5 đồng thời).

**Không dùng làm đòn bẩy độ khó**: giới hạn thời gian (timer) — game vốn "chơi rảnh 2-5
phút", thêm timer phá vỡ mục tiêu 1; giảm kích thước grid; RNG shape ác ý cụ thể nhắm vào
một người chơi (không thể kiểm chứng công bằng).

## 7. Level Objectives (các loại, dùng lặp lại + phối hợp)

| Loại | Mô tả | Dùng từ |
|---|---|---|
| Score Target | Đạt N điểm trong lượt chơi | Level 3+ |
| Lines Cleared | Clear tổng cộng N hàng/cột trong lượt chơi | Level 2+ |
| Combo Target | Đạt combo ≥K ít nhất 1 lần | Chapter 2+ |
| Multi-Clear | Clear ≥2 hàng/cột cùng một lần đặt, N lần | Chapter 3+ |
| Fill Slot | Đặt hết toàn bộ khay N lần liên tiếp không bỏ lỡ | Level 1 (onboarding), thỉnh thoảng tái dùng làm biến tấu nhẹ |
| Survive N Placements | Đặt được ít nhất N block trước khi hết nước đi (không cần clear) | Chapter khó cuối, dùng xen kẽ để đổi vị |

Tất cả objective đo bằng **counter đã có sẵn** trong engine hiện tại (Score.TotalScore,
tổng LinesCleared, Combo.CurrentCombo, số BlockPlaced) — không cần thêm phép tính điểm
mới, không đổi `ScoreRules`/`ComboRules`.

## 8. Gem Energy (resource giới hạn lượt chơi Journey)

- **Chỉ áp dụng cho Journey Mode.** Classic Mode **không bao giờ** tốn Energy — đây là quy
  tắc cứng, không đổi (đúng nguyên tắc "Classic mode luôn free" đã có ở `ECONOMY_DESIGN.md`
  v1 §1, kế thừa nguyên vẹn).
- Cơ chế: mỗi lần **vào** một level tốn 1 Gem Energy. Dung lượng tối đa 5. Hồi 1 Energy mỗi
  20 phút tự nhiên (không cần mở app). Level 1–3 (onboarding, §4) **miễn phí, không tốn
  Energy**.
- **Quan trọng — chống "độ khó giả"**: Energy chỉ tốn khi **vào** level, không tốn thêm khi
  **thua**. Người chơi có thể thử lại level đang chơi ngay lập tức bằng đúng 1 Energy đã
  trả cho lượt vào đó, miễn là chưa thoát ra menu chọn level (giữ đúng mục tiêu 4 — "muốn
  thử lại ngay", không phạt kép khi thua).
- Nguồn hồi thêm miễn phí: xem quảng cáo (+1 Energy, tối đa 3 lần/giờ), lên Player Rank
  (đầy bình), hoàn thành Daily Challenge (§15).
- Nguồn trả phí: mua Energy bằng Gem (tuỳ chọn), hoặc gói IAP "Energy không giới hạn 30
  phút" — **luôn có lựa chọn ad miễn phí tương đương về giá trị** (đúng mục tiêu 6).
- Test công bằng bắt buộc trước khi cân bằng số liệu thật: một người chơi hoàn toàn không
  trả tiền, không xem ad, chỉ chơi 15–20 phút/ngày (đủ hồi ~1 bình Energy đầy) phải chơi hết
  toàn bộ Journey trong vài tuần mà không bị chặn cứng — đây là tiêu chí nghiệm thu, không
  phải con số cuối cùng (số liệu thật tinh chỉnh sau playtest).

## 9. Power-ups — Hint, Hammer, Shuffle, Undo (+ kế thừa Bomb/Line Clear/Double Score từ v1)

| Power-up | Hiệu ứng | Giá (Coin, khởi điểm) | Miễn phí lần đầu? | Ghi chú công bằng |
|---|---|---|---|---|
| **Hint** *(mới ở V2)* | Gợi ý 1 vị trí hợp lệ cho 1 block trong khay (highlight ô, không tự đặt) | 20 | 1 hint free/level | Rẻ nhất, chỉ là "trợ lý", không thay đổi kết quả — giải quyết đúng vấn đề "người chơi mới không biết đặt đâu" đã nêu ở task UX trước, nhưng là cơ chế **được thiết kế**, không phải ẩn/bug |
| **Hammer** | Phá 1 ô bất kỳ | 50 | Không, nhưng rẻ | Kế thừa v1, không đổi |
| **Shuffle** | Đổi mới cả 3 block trong khay hiện tại | 60 | Không | Kế thừa v1, không đổi |
| **Undo** | Hoàn tác lượt đặt gần nhất | 100 | 1 undo free/level | Kế thừa v1; giới hạn 1 bước — tránh biến thành "chơi thử vô hạn" (sẽ phá cảm giác chinh phục, mục tiêu 3) |
| Bomb, Line Clear, Double Score | Không đổi so với `ECONOMY_DESIGN.md` v1 §3 | Như v1 | Không | Giữ nguyên, không thiết kế lại ở V2 |

**Quy tắc bắt buộc cho mọi power-up (áp dụng mục tiêu 6 & 7)**:
1. Không power-up nào là **điều kiện cần** để qua bất kỳ level nào — mỗi level phải có lời
   giải không dùng power-up (quy tắc thiết kế bắt buộc, QA kiểm bằng cách giải thử level
   trước khi phát hành — xem §17).
2. Power-up chỉ **rút ngắn thời gian/giảm rủi ro thất bại**, không mở ra kết quả mà chơi
   tay không đạt được (ví dụ: không có power-up "+500 điểm ảo").
3. Mọi power-up mua bằng Coin đều **kiếm được miễn phí** qua chơi/reward — Gem chỉ là
   đường tắt, không phải kênh duy nhất.

## 10. Continue (hồi sinh sau thất bại — khác Undo)

- Xuất hiện khi thua 1 level Journey (không đạt mục tiêu trước khi hết nước đi).
- Hiệu ứng: giữ nguyên board + điểm hiện tại, được thêm ngay 1 nước đi gỡ (ví dụ: phá 1 ô
  ngẫu nhiên đang chặn + tray mới) rồi chơi tiếp cùng lượt đó, **không tính là level mới**.
- **Giới hạn cứng: tối đa 1 continue/lượt chơi** (dù trả bằng Gem hay ad) — để thất bại vẫn
  có ý nghĩa thật (mục tiêu 3 cần thua có giá), đồng thời vẫn cho một cơ hội "gỡ" hợp lý
  (mục tiêu 4).
- Lựa chọn: xem rewarded ad (miễn phí) **hoặc** trả Gem (mặc định 10 Gem) — hai lựa chọn
  ngang hàng, không lựa chọn nào bị làm khó hơn lựa chọn kia (đúng nguyên tắc v1
  `MONETIZATION.md` §1 & `ECONOMY_DESIGN.md` §7, kế thừa nguyên vẹn).
- Nếu từ chối continue → level tính là thất bại, quay lại level map, **có thể chơi lại
  ngay** bằng 1 Gem Energy khác — đây là vòng lặp "thử lại" chính (mục tiêu 4), không phụ
  thuộc Continue.

## 11. Rewards

**Trên mỗi level**:
- Hoàn thành = 1★ (tối thiểu) tới 3★ (theo `StarThresholds`, §5) + Coin theo công thức
  hiện có (điểm cuối ván quy đổi Coin, không đổi so với v1).
- Lần đầu qua 1 level (không tính lần chơi lại) → thưởng Coin cố định thêm (khuyến khích
  tiến độ, không thưởng lặp lại khi replay để farm).

**Trên mỗi Chapter (15 level)**:
- Hoàn thành toàn bộ level trong chapter → mở **Chest** (rương thưởng): Coin lớn + 1
  power-up ngẫu nhiên + hiếm khi vài Gem.
- Đạt đủ 3★ tất cả level trong chapter (thử thách tuỳ chọn, không bắt buộc mở chapter
  sau) → Chest cấp cao hơn + cosmetic (theme màu grid mới — không phải nội dung ảnh hưởng
  gameplay, chỉ trang trí).

**Thiết kế "muốn thử lại" (mục tiêu 4)**:
- Màn thất bại luôn hiển thị: mục tiêu chưa đạt còn thiếu bao nhiêu % (ví dụ "Đạt 720/900
  điểm — còn 20%"), không chỉ hiện "Thất bại" chung chung — kế thừa & mở rộng nguyên tắc
  "giải thích lý do Game Over" đã code ở bản UX trước.
- Nút "Thử lại" là hành động chính, to nhất, một chạm — không có bước trung gian nào giữa
  màn thất bại và ván chơi lại.

## 12. Gems (premium currency)

- Vai trò không đổi so với v1: mua power-up cấp cao, Energy, Continue; **không** mua điểm
  số/thứ hạng trực tiếp.
- **Nguồn miễn phí** (quan trọng để giữ mục tiêu 5 — "free chơi hết được game"):
  - 3★ đầu tiên của mỗi level: +1 Gem (một lần, không lặp lại khi replay).
  - Chest chapter (§11): 3–8 Gem tuỳ cấp.
  - Achievement mốc lớn, Daily Challenge streak (§15).
  - Player Rank lên cấp (§16).
- **Nguồn trả phí**: IAP Gem Pack (không đổi so với `MONETIZATION.md` v1 §4).
- **Nguyên tắc cân bằng bắt buộc**: tổng Gem free kiếm được nếu chơi hết Journey (từ level
  đầu tới cuối, không mua gì) phải **đủ** để mua ít nhất vài power-up cấp cao mỗi chapter
  khó — đây là tiêu chí thiết kế, số liệu chính xác chốt sau playtest, nhưng là điều kiện
  bắt buộc trước khi khoá số liệu final (không được để "chơi hết Journey mà gần như không
  có Gem" — vi phạm mục tiêu 5).

## 13. Daily Challenges

- 1 board đặc biệt/ngày, seed cố định (mọi người chơi cùng ngày nhận cùng 1 bố cục — công
  bằng nếu sau này có leaderboard, không nằm trong scope V2 này).
- Mục tiêu ngắn (1 objective, hoàn thành trong ≤5 phút trung bình).
- **Không tốn Gem Energy** — tách hoàn toàn khỏi Journey, không cạnh tranh tài nguyên với
  progression chính (tránh Daily Challenge trở thành "thêm áp lực trả phí").
- Thưởng: Coin + streak bonus (chơi liên tiếp N ngày → thưởng tăng dần, reset nếu bỏ 1
  ngày — khuyến khích quay lại, không phạt nặng nếu lỡ 1 ngày ngoài việc mất streak).
- Không ảnh hưởng khả năng hoàn thành Journey — bỏ qua Daily Challenge hoàn toàn không
  chặn bất kỳ nội dung chính nào.

## 14. Progression (tổng hợp)

- **Level Map**: chapter → level, tiến độ chính, đã mô tả ở §5.
- **Player Rank (XP)**: cộng dồn từ mọi nguồn điểm (Journey + Classic), lên cấp → mở
  cosmetic (theme màu, không phải mechanic), + đầy 1 bình Gem Energy, + đôi khi Gem nhỏ.
  Không có "cấp yêu cầu" để chơi tiếp nội dung nào (không giống RPG chặn nội dung theo
  level — XP chỉ là ghi nhận lâu dài).
- **Achievements**: kế thừa nguyên vẹn v1 (`GAME_DESIGN.md` §7) — mốc dài hạn, thưởng một
  lần.
- **Classic Mode leaderboard cá nhân** (Best Score, đã có `BestScoreStore`): giữ nguyên,
  không đổi, không phải nội dung trả phí.

## 15. Fair Monetization Rules (tổng hợp, ràng buộc cứng)

1. Không mode chính nào (Journey lẫn Classic) bị khoá sau paywall — cả hai chơi được ngay
   từ đầu, mãi mãi, không giới hạn theo thời gian dùng thử.
2. Mọi lợi ích trả phí (Energy, Continue, power-up) đều có đường thay thế miễn phí (ad
   hoặc thời gian chờ hoặc chơi tích luỹ) với giá trị tương đương — không có lợi ích nào
   **chỉ** mua được bằng tiền thật.
3. Không bán điểm số, thứ hạng, hay kết quả level trực tiếp.
4. Interstitial ads không bao giờ xuất hiện giữa lúc đang chơi (kế thừa `MONETIZATION.md`
   v1 §1) — áp dụng cho cả Journey lẫn Classic.
5. Mỗi level phải có "zero-spend clear path" đã kiểm chứng trước khi phát hành (§17).
6. Độ khó không được điều chỉnh dựa trên lịch sử mua hàng/từ chối mua hàng của người chơi
   (§18 — cấm dynamic pay-wall difficulty).
7. Toàn bộ số liệu giá/thưởng/độ khó nằm trong config (ScriptableObject tương lai), không
   hard-code — kế thừa nguyên tắc kiến trúc đã áp dụng xuyên suốt dự án, giúp cân bằng lại
   được sau playtest mà không cần sửa code.

## 16. Giới hạn chống Pay-to-Win / chống Frustration (chi tiết hoá mục tiêu 7)

| Rủi ro | Giới hạn thiết kế |
|---|---|
| Level "không thể qua" nếu không mua power-up | **Cấm tuyệt đối.** Mỗi `LevelDefinition` phải kèm ít nhất 1 lời giải zero-spend đã kiểm chứng thủ công/bằng simulation trước khi đưa vào chapter phát hành. Đây là điều kiện release, không phải khuyến nghị. |
| Difficulty tăng bí mật sau khi người chơi từ chối mua hàng | **Cấm tuyệt đối.** Toàn bộ tham số độ khó chỉ phụ thuộc `LevelId`/`ChapterId`, không phụ thuộc lịch sử giao dịch, số lần thua, hay bất kỳ tín hiệu hành vi mua hàng nào. |
| Continue/Energy trở thành bắt buộc trả phí trong thực tế (ad mệt/khó xem) | Giới hạn ad rewarded theo tần suất hợp lý (kế thừa v1), luôn hiện rõ 2 lựa chọn ngang hàng (ad hoặc trả phí), không làm nút ad nhỏ hơn/khó bấm hơn nút trả phí. |
| Power-up làm mất cảm giác "tự mình chinh phục" nếu dùng quá dễ dàng | Hint/Undo chỉ có 1 lần miễn phí/level (không phải không giới hạn) — đủ giúp gỡ khó, không đủ để "auto-solve" cả level. |
| Energy hết → cảm giác bị chặn hoàn toàn | Classic Mode luôn mở miễn phí không giới hạn khi hết Energy — người chơi luôn có việc để chơi, không bao giờ "không còn gì để làm" (khác hẳn nhiều game energy-gated khác). |
| Daily Challenge / streak tạo áp lực "phải chơi mỗi ngày" | Mất streak chỉ mất phần thưởng tăng dần, không mất tiến độ Journey, không mất vật phẩm đã có — áp lực chỉ ở mức "tiếc", không phải "mất mát". |

## 17. Quy trình QA thiết kế bắt buộc trước khi một Chapter được coi là "sẵn sàng"

(Ghi chú quy trình cho milestone code/QA sau này — không phải code ở task này)

1. Với mỗi level: chơi thử (hoặc simulate) ít nhất 1 lần **không dùng bất kỳ power-up
   nào** và xác nhận có thể đạt mục tiêu 1★ — nếu không, level bị coi là fail thiết kế,
   phải chỉnh lại target/shape weight trước khi phát hành.
2. Xác nhận độ khó tăng **đơn điệu không giật cục** trong 1 chapter (không có level giữa
   chapter khó hơn hẳn level cuối chapter đó).
3. Xác nhận tổng Coin/Gem free kiếm được hết 1 chapter đủ mua ít nhất 1 power-up cấp cao
   cho chapter kế tiếp (đúng §12).
4. Xác nhận không level nào yêu cầu combo/multi-clear vượt quá khả năng lý thuyết của
   trọng số shape hiện có tại chapter đó (tránh mục tiêu toán học không khả thi).

## 18. Delta so với tài liệu v1 (GAME_DESIGN.md / ECONOMY_DESIGN.md / MONETIZATION.md)

| Tài liệu v1 | Trạng thái ở V2 |
|---|---|
| Grid/Block/Input/Game-Over-detection (`GAME_DESIGN.md` §3–6) | **Không đổi**, tái sử dụng nguyên vẹn |
| Scoring & Combo formula (`GAME_DESIGN.md` §5, `ScoreRules`/`ComboRules`) | **Không đổi** |
| "Classic mode duy nhất" (`GAME_DESIGN.md` §9) | **Bổ sung**, không thay thế — Classic giữ nguyên làm mode phụ |
| Power-up list (`GAME_DESIGN.md` §8, `ECONOMY_DESIGN.md` §3–4) | **Giữ nguyên** + thêm Hint |
| Coin/Gem/Energy currency (`ECONOMY_DESIGN.md` §1) | **Energy được kích hoạt thật** (v1 để "dự phòng, ngoài scope"), áp dụng đúng như nguyên tắc v1 đã đặt ra: chỉ gate Journey, không gate Classic |
| Anti-pay-to-win (`ECONOMY_DESIGN.md` §7) | **Giữ nguyên nguyên tắc**, bổ sung ràng buộc cụ thể cho Journey (§16–17 tài liệu này) |
| Daily Missions (`GAME_DESIGN.md` §7) | Đổi tên/khung thành **Daily Challenge** có board riêng (cụ thể hoá hơn v1, cùng tinh thần) |
| Ads placement, IAP catalog (`MONETIZATION.md`) | **Không đổi**, áp dụng cho cả Journey lẫn Classic |

## 19. Ngoài phạm vi task này (không thiết kế chi tiết, không code)

- Số liệu cân bằng chính xác (target điểm từng level, giá Gem, tốc độ hồi Energy) — cần
  playtest thật, chỉ nêu nguyên tắc/tiêu chí nghiệm thu ở tài liệu này.
- Leaderboard/social features.
- Cụ thể hoá `LevelDefinition`/`EnergyConfig`/`DifficultyCurveConfig` thành ScriptableObject
  thật (việc của milestone code kế tiếp, sau khi Product Owner duyệt thiết kế này).
- Bất kỳ thay đổi nào tới `GridModel`/`GameManager`/`ScoreManager`/`ComboManager`/
  `GameOverChecker` — không cần và không được đổi theo tài liệu này.
