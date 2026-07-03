# Changelog — Lunar Console Prototype

Tài liệu ghi lại các task, thay đổi và bug fix từ khi bắt đầu dự án đến hiện tại.

**Stack:** .NET 10.0 · C# · Layered Architecture (Exception / Model / Repository / Service / UI / Util)

**Solution:** `Lunar.slnx` — `Lunar.Core`, `Lunar.Console`, `Lunar.Core.Tests`

**Save path:** `%LocalAppData%\Lunar\save.json`

---

## [Unreleased]

*(Chưa có thay đổi mới sau v0.2.0)*

---

## [0.2.0] — 2026-07-03

### Changed

- **Tái cấu trúc folder phân tầng** — toàn project chuyển từ Clean Architecture 4 lớp (Domain / Application / Infrastructure / Presentation) sang 6 thư mục:

  | Tầng | Project | Nội dung |
  |------|---------|----------|
  | **Exception** | `Lunar.Core/Exception/` | `GameException`, `EntityNotFoundException` |
  | **Model** | `Lunar.Core/Model/` | Entity, aggregate, DTO, `GameSession`, `GameState` |
  | **Repository** | `Lunar.Core/Repository/` + `Lunar.Console/Repository/` | `ISaveRepository`, `JsonSaveRepository` |
  | **Service** | `Lunar.Core/Service/` | Use cases (Explore, Combat, Save, Merchant, …) |
  | **UI** | `Lunar.Console/UI/` | Screens, Presenter, Input/Output |
  | **Util** | `Lunar.Core/Util/` + `Lunar.Console/Util/` | `IEventBus`, `IRandomService`, `DomainEventPublisher`, `RandomService`, `InMemoryEventBus` |

- **Namespace migration:**

  | Cũ | Mới |
  |----|-----|
  | `Lunar.Core.Domain.*` | `Lunar.Core.Model.*` |
  | `Lunar.Core.Application.UseCases` | `Lunar.Core.Service` |
  | `Lunar.Core.Application.DTOs` | `Lunar.Core.Model.Dto` |
  | `Lunar.Core.Application` (session/state) | `Lunar.Core.Model` |
  | `Lunar.Core.Application.Interfaces.ISaveRepository` | `Lunar.Core.Repository` |
  | `Lunar.Core.Application.Interfaces` (event, random) | `Lunar.Core.Util` |
  | `Lunar.Console.Presentation` | `Lunar.Console.UI` |
  | `Lunar.Console.Infrastructure` | `Lunar.Console.Repository` / `Util` |

- **`ItemFactory` / `BossFactory`** — throw `EntityNotFoundException` thay vì `KeyNotFoundException` khi id không hợp lệ.

### Fixed

- **Combat screen biến mất khi đánh quái thường** — `HandleExplore()` gọi `RunCombat()` nhưng thiếu `StartCombat(session, enemyId)` → `ActiveCombatSession` null, vòng combat không chạy.
- **Log lượt quái bị mất** — `CombatSession.Execute()` gộp log tấn công enemy sau lượt player vào `CommandResult`.

### Verified

- Build: OK
- Tests: **27/27 passed** (`Lunar.Core.Tests`)

---

## [0.1.0] — Sprint 1–4 (Console Demo)

Phiên bản đầu tiên chơi được end-to-end trên console. Cấu trúc lúc này dùng **Domain / Application / Infrastructure / Presentation** (trước khi refactor v0.2.0).

---

## Phase 0 — Kiến trúc & Thiết kế

### Tài liệu

- Viết và chỉnh sửa `Console_Prototype_Architecture.md` — kiến trúc Console prototype trước Unity.
- Đổi tên dự án **HeIsComing → Lunar** trong toàn bộ tài liệu và code.
- Review kiến trúc: bổ sung **Demo Rules**, **GameSession**, **GameState**, luồng combat thống nhất qua `ICombatCommand`.
- Chuẩn hóa Sprint Roadmap (Sprint 1–4), module structure, design patterns, UI mockup console.

### Nguyên tắc thiết kế

| Nguyên tắc | Mô tả |
|---|---|
| Phân tầng (v0.2+) | UI → Service → Model; Repository/Util implement contract |
| Rich Domain Model | Logic trên entity/aggregate, Service layer mỏng |
| Single source of truth | `GameSession` giữ runtime state |
| Domain events | Aggregate collect events, Service publish qua `IEventBus` |
| Result pattern | Lỗi nghiệp vụ qua `Success` + `Error` DTO — không có exception layer riêng |
| Unity-ready | `Lunar.Core` tái sử dụng khi chuyển sang Unity |

---

## Sprint 1 — Nền tảng + Combat + Loop ngày

**Mục tiêu:** Chơi được end-to-end trên console — explore, combat, rest, next day.

### Model (`Lunar.Core/Model`)

| Task | Chi tiết |
|---|---|
| Value objects | `Health`, `Stats`, `Damage` |
| Characters | `Character`, `Player`, `Enemy`, `IDamageable` |
| Effective stats | `Player.GetEffectiveStats()` — base + equipment |
| Combat core | `CombatSession`, `CombatPhase`, `ICombatCommand` |
| Commands | `AttackCommand`, `FleeCommand` |
| Calculators | `DamageCalculator` |
| World | `EnemyFactory`, `EncounterTable`, `EnemyEncounter` |
| AI | `IEnemyAI` — quái chọn action mỗi lượt |

### Service + Util

| Task | Chi tiết |
|---|---|
| Runtime state | `GameSession`, `DayFlags` (HasExplored, HasRested) |
| Services | `StartGameUseCase`, `ExploreUseCase`, `CombatUseCase`, `RestUseCase`, `AdvanceDayUseCase` |
| DTOs | `CombatResultDto`, `ExploreResultDto`, `GameplayMenuDto` |
| Util | `IRandomService`, `IEventBus` |

### UI + Util (`Lunar.Console`)

| Task | Chi tiết |
|---|---|
| Random | `RandomService` |
| Events | `InMemoryEventBus` |
| I/O | `InputReader`, `OutputWriter` |
| Screens | `MainMenuScreen`, `GameplayScreen`, `CombatScreen` |
| Wiring | `ConsoleGamePresenter` — DI, screen routing, game loop |
| Loot cơ bản | Thắng combat → thêm item vào inventory |

### Gameplay loop

- Main Menu → New Game → Day Loop
- Mỗi ngày: **Explore hoặc Rest** (1 lần) → Next Day
- Explore gặp quái → Combat → loot nếu thắng
- Boss day trigger qua `AdvanceDayUseCase`
- Flee: thoát an toàn, không loot

---

## Sprint 2 — Inventory, Equipment & Skills

**Mục tiêu:** Hệ thống item đầy đủ, trang bị ảnh hưởng stats, skill trong combat.

### Model

| Task | Chi tiết |
|---|---|
| Item polymorphism | `Item`, `Consumable`, `Weapon`, `Armor`, `Ring` |
| Factory | `ItemFactory` — potion, sword, armor, ring |
| Inventory | `Inventory`, `ItemStack`, `EquipmentLoadout` |
| Skills | `SkillDefinition`, `PlayerSkillState`, cooldown |
| Commands | `UseItemCommand`, `SkillCommand` |
| Calculators | `CriticalCalculator` |
| Loot | `LootTable`, roll loot theo bảng |

**Items mặc định:** `health_potion`, `rusty_dagger`, `iron_sword`, `leather_armor`, `copper_ring`

### Service

| Task | Chi tiết |
|---|---|
| Services | `EquipItemUseCase`, `UseItemUseCase`, `ApplyLootUseCase` |
| Events | `ItemEquipped`, `LootGranted`, `DomainEventPublisher` |
| Combat | Skill + Use Item trong combat qua `CombatUseCase` |

### UI

| Task | Chi tiết |
|---|---|
| Screens | `InventoryScreen`, `EquipmentScreen` |
| Combat UI | Attack, Hero Strike (skill), Use Potion, Run |

---

## Sprint 3 — Boss, World Encounters & Merchant

**Mục tiêu:** Boss nhiều phase, explore đa dạng, cửa hàng.

### Model — Boss

| Task | Chi tiết |
|---|---|
| Boss entity | `Boss : Enemy`, `IBossPhase` strategy |
| Phases | Phase 1 / 2 / Enraged — chuyển phase theo % HP |
| Factory | `BossFactory`, `BossDatabase` |
| Commands | `BossSkillCommand` |
| Events | `BossPhaseChanged`, `BossDefeated` |
| Boss mặc định | Skeleton King (`skeleton_king`) |

### Model — World

| Task | Chi tiết |
|---|---|
| Encounters | `ChestEncounter`, `EmptyEncounter`, `EventEncounter`, `MerchantEncounter` |
| Events | Gold/heal/damage từ event encounter |
| Gold | `Player.AddGold()`, dùng cho merchant |

### Service + UI

| Task | Chi tiết |
|---|---|
| Services | `BossBattleUseCase`, `MerchantUseCase` |
| Boss rewards | Thắng boss → loot + `Difficulty++` + reset `CurrentDay = 1` |
| Meta | `MetaProgression.BossesDefeated` |
| Screen | `MerchantScreen` — mua/bán |
| Boss flow | `HandleBoss()` → `StartBossFight()` → Combat (boss UI) |
| Event bus UI | Log `[Event] Equipped`, `[Event] Boss phase` |

---

## Sprint 4 — Save / Load & Meta Progression

**Mục tiêu:** Lưu/tải game, Continue từ menu, auto-save.

### Model + Repository + Service

| Task | Chi tiết |
|---|---|
| Snapshot | `GameState` v1 — HP, stats, inventory `{ itemId, qty }`, equipment, day, difficulty, meta |
| Meta | `MetaProgression` — bosses defeated, persistent qua save |
| Services | `SaveGameUseCase`, `LoadGameUseCase` |
| Repository | `ISaveRepository` (Core), `JsonSaveRepository` (Console) |
| Events | `GameSaved`, `DayAdvanced` |

### UI

| Task | Chi tiết |
|---|---|
| Main Menu | **Continue** — load save nếu có |
| Manual save | Action Save trong gameplay |
| Auto-save | Khi advance day, khi thắng combat |
| Game Over | Xóa save, quay Main Menu |

**Validation:** Không save khi đang trong combat.

---

## Testing — `Lunar.Core.Tests`

**Framework:** xUnit · **Trạng thái:** 27/27 passed (v0.2.0)

| File | Phạm vi |
|---|---|
| `Sprint1Tests.cs` | Health, effective stats, combat attack/flee, rest, explore, advance day |
| `Sprint2Tests.cs` | Item factory, equip/swap, use item, skill cooldown, apply loot |
| `Sprint3Tests.cs` | Boss phases, chest/event/merchant encounters, merchant buy/sell, boss rewards |
| `Sprint4Tests.cs` | GameState round-trip, JSON save/load, save during combat blocked, delete save |

**Test helpers:** `TestHelpers`, `FixedRandomService`, `InMemoryEventBus`

---

## Cấu trúc project hiện tại (v0.2.0)

```text
lunarConsole/
├── CHANGELOG.md
├── Console_Prototype_Architecture.md
├── Lunar.slnx
│
├── Lunar.Core/
│   ├── Exception/           GameException, EntityNotFoundException
│   ├── Model/
│   │   ├── Characters/, Combat/, Inventory/, Items/, Bosses/, World/, Events/, Skills/
│   │   ├── Dto/             CombatResultDto, ExploreResultDto, ...
│   │   ├── GameSession.cs, GameState.cs, MetaProgression.cs
│   ├── Repository/          ISaveRepository
│   ├── Service/             *UseCase, GameplayPresenterMapper
│   └── Util/                IEventBus, IRandomService, DomainEventPublisher
│
├── Lunar.Console/
│   ├── UI/                  ConsoleGamePresenter, InputReader, OutputWriter, Screens/
│   ├── Repository/          JsonSaveRepository
│   ├── Util/                RandomService, InMemoryEventBus
│   └── Program.cs
│
└── Lunar.Core.Tests/        Sprint1–4 tests + TestHelpers
```

### Luồng phụ thuộc

```text
UI (Lunar.Console)
      │
      ▼
Service (Lunar.Core)
      │
      ▼
Model (Lunar.Core)
      ▲
      │
Repository / Util (implement interface → inject từ Program.cs)
```

### Screens (`Lunar.Console/UI/Screens`)

| Screen | Chức năng |
|---|---|
| `MainMenuScreen` | New Game / Continue / Exit |
| `GameplayScreen` | Explore, Rest, Inventory, Equipment, Face Boss, Next Day, Save, Exit |
| `CombatScreen` | Attack, Skill, Potion, Flee |
| `InventoryScreen` | Xem/dùng item, equip |
| `EquipmentScreen` | Xem loadout |
| `MerchantScreen` | Mua/bán |

### Services (`Lunar.Core/Service`)

`StartGame`, `LoadGame`, `SaveGame`, `Explore`, `Combat`, `Rest`, `AdvanceDay`, `BossBattle`, `EquipItem`, `UseItem`, `ApplyLoot`, `Merchant`

### Domain Events (`Lunar.Core/Model/Events`)

`EnemyDefeated`, `LootGranted`, `DayAdvanced`, `ItemEquipped`, `PlayerDied`, `BossPhaseChanged`, `ChestOpened`, `BossDefeated`, `GameSaved`

### Exception (`Lunar.Core/Exception`)

| Class | Dùng khi |
|---|---|
| `GameException` | Base exception cho lỗi game |
| `EntityNotFoundException` | Item/boss id không tồn tại trong factory |

*Lỗi nghiệp vụ thông thường (cooldown, không đủ gold, …) vẫn dùng Result pattern — không throw exception.*

---

## Khác biệt so với Architecture doc

`Console_Prototype_Architecture.md` vẫn mô tả Clean Architecture cũ (Domain/Application/Infrastructure/Presentation). Code v0.2.0 đã chuyển sang phân tầng mới — doc chưa cập nhật.

| Doc | Code v0.2.0 |
|---|---|
| `Domain/` | `Model/` |
| `Application/UseCases/` | `Service/` |
| `Application/DTOs/` | `Model/Dto/` |
| `Infrastructure/` | `Repository/` + `Util/` |
| `Presentation/` | `UI/` |
| `CombatState` State pattern (4 states) | `CombatPhase` enum |
| `ResolveEffectsState`, StatusEffect | Chưa có |
| `FileLogger` | Chưa có |
| `BossScreen` riêng | Reuse `CombatScreen` |
| Settings menu | Chưa có |
| `Lunar.Unity` project | Chưa tạo |

---

## Git history

| Commit / Tag | Mô tả |
|---|---|
| `36f4868` | Update — initial project structure |
| `3d1e9f5` | Update 0.1 — Sprint 1–4 implementation |
| `bd74471` | v0.1 tag |
| *(uncommitted)* | v0.2.0 — folder restructure, combat fix, exception types |

---

## Roadmap tiếp theo

- [ ] Cập nhật `Console_Prototype_Architecture.md` theo cấu trúc phân tầng mới
- [ ] Hoàn thiện `CombatState` State pattern
- [ ] `StatusEffect` system
- [ ] `FileLogger` trong `Util`
- [ ] Settings screen
- [ ] Data-driven content tables (JSON/YAML)
- [ ] `Lunar.Unity` — port UI layer
- [ ] Class diagram & sequence diagram trong Docs/
