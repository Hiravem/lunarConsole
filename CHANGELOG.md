# Changelog — Lunar Console Prototype

Tài liệu này ghi lại các task, thay đổi và bug fix từ khi bắt đầu dự án đến hiện tại.

**Stack:** .NET 10.0 · C# · Clean Architecture (Domain / Application / Infrastructure / Presentation)

**Solution:** `Lunar.slnx` — `Lunar.Core`, `Lunar.Console`, `Lunar.Core.Tests`

**Save path:** `%LocalAppData%\Lunar\save.json`

---

## [Unreleased]

### Fixed

- **Combat screen biến mất khi đánh quái thường** — `HandleExplore()` gọi `RunCombat()` nhưng không gọi `StartCombat()` trước đó, khiến `ActiveCombatSession` null và vòng lặp combat không chạy. Đã thêm `StartCombat(session, enemyId)` trước khi vào combat.
- **Log lượt quái bị mất** — `CombatSession.Execute()` giờ gộp log tấn công của enemy sau lượt player vào `CommandResult` trả về.

---

## Phase 0 — Kiến trúc & Thiết kế

### Tài liệu

- Viết và chỉnh sửa `Console_Prototype_Architecture.md` — kiến trúc Console prototype trước Unity.
- Đổi tên dự án **HeIsComing → Lunar** trong toàn bộ tài liệu và code.
- Review kiến trúc: bổ sung **Demo Rules**, **GameSession**, **GameState**, luồng combat thống nhất qua `ICombatCommand`.
- Chuẩn hóa Sprint Roadmap (Sprint 1–4), module structure, design patterns, UI mockup console.

### Nguyên tắc thiết kế đã chốt

| Nguyên tắc | Mô tả |
|---|---|
| Clean Architecture | Presentation → Application → Domain ← Infrastructure |
| Rich Domain Model | Logic trên entity/aggregate, Application layer mỏng |
| Single source of truth | `GameSession` giữ runtime state |
| Domain events | Aggregate collect events, UseCase publish qua `IEventBus` |
| Unity-ready | `Lunar.Core` tái sử dụng khi chuyển sang Unity |

---

## Sprint 1 — Nền tảng + Combat + Loop ngày

**Mục tiêu:** Chơi được end-to-end trên console — explore, combat, rest, next day.

### Domain (`Lunar.Core`)

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

### Application

| Task | Chi tiết |
|---|---|
| Runtime state | `GameSession`, `DayFlags` (HasExplored, HasRested) |
| Use cases | `StartGameUseCase`, `ExploreUseCase`, `CombatUseCase`, `RestUseCase`, `AdvanceDayUseCase` |
| DTOs | `CombatResultDto`, `ExploreResultDto`, `GameplayMenuDto` |
| Interfaces | `IRandomService`, `IEventBus` |

### Infrastructure & Presentation (`Lunar.Console`)

| Task | Chi tiết |
|---|---|
| Random | `RandomService` |
| Events | `InMemoryEventBus` |
| I/O | `InputReader`, `OutputWriter` |
| Screens | `MainMenuScreen`, `GameplayScreen`, `CombatScreen` |
| Wiring | `ConsoleGamePresenter` — DI, screen routing, game loop |
| Loot cơ bản | Thắng combat → thêm item vào inventory |

### Gameplay loop Sprint 1

- Main Menu → New Game → Day Loop
- Mỗi ngày: **Explore hoặc Rest** (1 lần) → Next Day
- Explore gặp quái → Combat → loot nếu thắng
- Boss day trigger qua `AdvanceDayUseCase`
- Flee: thoát an toàn, không loot

---

## Sprint 2 — Inventory, Equipment & Skills

**Mục tiêu:** Hệ thống item đầy đủ, trang bị ảnh hưởng stats, skill trong combat.

### Domain

| Task | Chi tiết |
|---|---|
| Item polymorphism | `Item`, `Consumable`, `Weapon`, `Armor`, `Ring` |
| Factory | `ItemFactory` — đăng ký item mặc định (potion, sword, armor, ring) |
| Inventory | `Inventory`, `ItemStack`, `EquipmentLoadout` |
| Skills | `SkillDefinition`, `PlayerSkillState`, cooldown |
| Commands | `UseItemCommand`, `SkillCommand` |
| Calculators | `CriticalCalculator` |
| Loot | `LootTable`, roll loot theo bảng |

**Items mặc định:** `health_potion`, `rusty_dagger`, `iron_sword`, `leather_armor`, `copper_ring`

### Application

| Task | Chi tiết |
|---|---|
| Use cases | `EquipItemUseCase`, `UseItemUseCase`, `ApplyLootUseCase` |
| Domain events | `ItemEquipped`, `LootGranted`, `DomainEventPublisher` |
| Combat | Skill + Use Item trong combat qua `CombatUseCase` |

### Presentation

| Task | Chi tiết |
|---|---|
| Screens | `InventoryScreen`, `EquipmentScreen` |
| Combat UI | Attack, Hero Strike (skill), Use Potion, Run |

---

## Sprint 3 — Boss, World Encounters & Merchant

**Mục tiêu:** Boss nhiều phase, explore đa dạng, cửa hàng.

### Domain — Boss

| Task | Chi tiết |
|---|---|
| Boss entity | `Boss : Enemy`, `IBossPhase` strategy |
| Phases | Phase 1 / 2 / Enraged — chuyển phase theo % HP |
| Factory | `BossFactory`, `BossDatabase` |
| Commands | `BossSkillCommand` |
| Events | `BossPhaseChanged`, `BossDefeated` |
| Boss mặc định | Skeleton King (`skeleton_king`) |

### Domain — World

| Task | Chi tiết |
|---|---|
| Encounters | `ChestEncounter`, `EmptyEncounter`, `EventEncounter`, `MerchantEncounter` |
| Events | Gold/heal/damage từ event encounter |
| Gold | `Player.AddGold()`, dùng cho merchant |

### Application

| Task | Chi tiết |
|---|---|
| Use cases | `BossBattleUseCase`, `MerchantUseCase` |
| Boss rewards | Thắng boss → loot + `Difficulty++` + reset `CurrentDay = 1` |
| Meta hook | `MetaProgression.BossesDefeated` tăng khi hạ boss |

### Presentation

| Task | Chi tiết |
|---|---|
| Screen | `MerchantScreen` — mua/bán item |
| Boss flow | `HandleBoss()` → `StartBossFight()` → Combat (boss UI) |
| Event bus UI | Log `[Event] Equipped`, `[Event] Boss phase` |

---

## Sprint 4 — Save / Load & Meta Progression

**Mục tiêu:** Lưu/tải game, Continue từ menu, auto-save.

### Application

| Task | Chi tiết |
|---|---|
| Snapshot | `GameState` v1 — HP, stats, inventory `{ itemId, qty }`, equipment, day, difficulty, meta |
| Meta | `MetaProgression` — bosses defeated, persistent qua save |
| Use cases | `SaveGameUseCase`, `LoadGameUseCase` |
| Interface | `ISaveRepository` |
| Events | `GameSaved`, `DayAdvanced` |

### Infrastructure

| Task | Chi tiết |
|---|---|
| Persistence | `JsonSaveRepository` — JSON file tại `%LocalAppData%\Lunar\` |
| Validation | Không save khi đang trong combat |

### Presentation

| Task | Chi tiết |
|---|---|
| Main Menu | **Continue** — load save nếu có |
| Manual save | Action Save trong gameplay |
| Auto-save | Khi advance day, khi thắng combat |
| Game Over | Xóa save, quay Main Menu |

---

## Testing — `Lunar.Core.Tests`

**Framework:** xUnit · **Trạng thái:** 27/27 passed

| File | Phạm vi |
|---|---|
| `Sprint1Tests.cs` | Health, effective stats, combat attack/flee, rest, explore, advance day |
| `Sprint2Tests.cs` | Item factory, equip/swap, use item, skill cooldown, apply loot |
| `Sprint3Tests.cs` | Boss phases, chest/event/merchant encounters, merchant buy/sell, boss rewards |
| `Sprint4Tests.cs` | GameState round-trip, JSON save/load, save during combat blocked, delete save |

**Test helpers:** `TestHelpers`, `FixedRandomService`, `InMemoryEventBus`

---

## Cấu trúc project hiện tại

```text
lunarConsole/
├── CHANGELOG.md
├── Console_Prototype_Architecture.md
├── Lunar.slnx
├── Lunar.Core/              # Domain + Application (~55 files)
├── Lunar.Console/           # Infrastructure + Presentation (~16 files)
└── Lunar.Core.Tests/        # Unit tests (4 sprint test files)
```

### Screens (Presentation)

| Screen | Chức năng |
|---|---|
| `MainMenuScreen` | New Game / Continue / Exit |
| `GameplayScreen` | Explore, Rest, Inventory, Equipment, Face Boss, Next Day, Save, Exit |
| `CombatScreen` | Attack, Skill, Potion, Flee |
| `InventoryScreen` | Xem/dùng item, equip |
| `EquipmentScreen` | Xem loadout |
| `MerchantScreen` | Mua/bán |

### Use Cases (Application)

`StartGame`, `LoadGame`, `SaveGame`, `Explore`, `Combat`, `Rest`, `AdvanceDay`, `BossBattle`, `EquipItem`, `UseItem`, `ApplyLoot`, `Merchant`

### Domain Events

`EnemyDefeated`, `LootGranted`, `DayAdvanced`, `ItemEquipped`, `PlayerDied`, `BossPhaseChanged`, `ChestOpened`, `BossDefeated`, `GameSaved`

---

## Khác biệt so với Architecture doc (chưa implement)

Các mục trong `Console_Prototype_Architecture.md` **chưa có trong code** hoặc **implement khác**:

| Doc | Code hiện tại |
|---|---|
| `CombatState` State pattern (4 states) | `CombatPhase` enum (`PlayerTurn`, `EnemyTurn`, `Finished`) |
| `ResolveEffectsState` | Chưa có — status effects chưa implement |
| `StatusEffectCollection`, `StatusEffectResolver` | Chưa có |
| `FileLogger` | Chưa có |
| `BossScreen` riêng | Reuse `CombatScreen` với boss header |
| Settings menu | Stub — defer Sprint 4+ |
| `Lunar.Unity` project | Chưa tạo — planned |

---

## Git history

| Commit | Mô tả |
|---|---|
| `36f4868` | Update — initial project structure |
| `3d1e9f5` | Update 0.1 — Sprint 1–4 implementation |

*(Các thay đổi sau commit cuối — bug fix combat screen, log enemy turn — chưa commit.)*

---

## Roadmap tiếp theo (đề xuất)

- [ ] Hoàn thiện `CombatState` State pattern theo architecture doc
- [ ] `StatusEffect` system
- [ ] `FileLogger` cho debug
- [ ] Settings screen (volume, log level)
- [ ] Data-driven content tables (JSON/YAML cho items, bosses, encounters)
- [ ] `Lunar.Unity` — port Presentation layer
- [ ] Class diagram & sequence diagram trong Docs/
