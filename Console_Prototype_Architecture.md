# Console Prototype Architecture (OOP)

## Mục tiêu

Trước khi phát triển game trên Unity, xây dựng một phiên bản Console bằng C# để kiểm thử:

- Kiến trúc hệ thống
- Luồng gameplay
- Combat
- Inventory
- Equipment
- Boss AI
- Save/Load
- Event System

Mục tiêu là tách hoàn toàn **Business Logic** khỏi Unity để có thể tái sử dụng khi chuyển sang Unity.

Nguyên tắc thiết kế: **Clean Architecture + Rich Domain Model (OOP)** — logic nằm trên entity/aggregate, Application layer mỏng, tránh Manager god class.

---

## Quy tắc gameplay (Demo Rules)

Các quy tắc cố định cho prototype console — tránh mơ hồ khi implement loop và UI.

| Quy tắc | Mô tả |
|---|---|
| **1 ngày = 1 chu kỳ** | Mỗi ngày player chọn **một hành động chính** trước khi sang ngày mới |
| **Explore** | Tối đa **1 lần/ngày** — random encounter từ bảng weighted theo difficulty |
| **Rest** | Tối đa **1 lần/ngày** — hồi HP cố định (ví dụ 30% MaxHP), không stack với explore cùng ngày |
| **Inventory / Equipment** | Mở bất cứ lúc nào trong ngày, **không** tiêu tốn lượt ngày |
| **Next Day** | Kết thúc ngày → `AdvanceDayUseCase` → kiểm tra boss trigger |
| **Boss day** | Ngày boss: **không explore/rest** — vào thẳng `BossBattleUseCase` |
| **Combat — Flee** | Thoát combat an toàn, **không loot**, quay về gameplay screen, **đã dùng lượt explore** |
| **Combat — thua** | `PlayerDied` → Game Over → Main Menu (meta progression Sprint 4+) |
| **Combat — thắng boss** | Reward loot + tăng `Difficulty` + reset `CurrentDay` về 1, **giữ inventory/equipment** |
| **Effective stats** | Combat luôn dùng `Player.GetEffectiveStats()` — không tự cộng tay ở UseCase |

---

## Nguyên tắc OOP

| Nguyên tắc | Áp dụng |
|---|---|
| **Rich Domain Model** | Hành vi (equip, use item, nhận damage) nằm trên entity/aggregate |
| **Composition over Inheritance** | `Character` has-a `Health`, `Stats`, `StatusEffects` — kế thừa tối đa 2–3 cấp |
| **Polymorphism** | `Item`, `IExploreEncounter`, `IBossPhase` — mở rộng bằng class mới |
| **Open/Closed** | Thêm Enemy/Boss/Item mới không sửa code cũ |
| **Single Responsibility** | Mỗi class một lý do thay đổi |
| **Interface Segregation** | `IDamageable`, `IInventoryHolder`, `ICombatCommand` — interface nhỏ, cụ thể |

---

## Kiến trúc dự án

```text
Lunar.Core
│
├── Domain
│   ├── Characters          # Entity hierarchy + value objects
│   ├── Combat              # CombatSession, commands, states, calculators
│   ├── Inventory           # Aggregate: Inventory, EquipmentLoadout
│   ├── Items               # Item polymorphism + ItemFactory
│   ├── Bosses              # Boss + IBossPhase strategy
│   ├── World               # IExploreEncounter, ExploreResolver
│   ├── Events              # IDomainEvent records (pure data)
│   └── Skills              # SkillDefinition (data-driven, Sprint 2+)
│
├── Application
│   ├── GameSession         # Runtime state — single source of truth
│   ├── UseCases            # StartGame, Explore, Combat, Equip, Save
│   ├── Interfaces          # IEventBus, ISaveRepository, IRandomService
│   └── DTOs                # CombatResultDto, ExploreResultDto, ...
│
└── (không phụ thuộc Console/Unity)

Lunar.Console
│
├── Infrastructure
│   ├── Save                # JsonSaveRepository
│   ├── Random              # RandomService
│   ├── Events              # InMemoryEventBus
│   └── Logger              # FileLogger
│
├── Presentation
│   ├── Menus
│   ├── Screens
│   ├── Input
│   └── Output
│
└── Program.cs
```

### Luồng phụ thuộc (Clean Architecture)

```text
Presentation
      │
      ▼
Application (Use Cases + GameSession)
      │
      ▼
Domain (OOP core — Rich Model)
      ▲
      │
Infrastructure (implements Application interfaces)
```

**Domain** không phụ thuộc Unity, Console, JSON, hay file I/O. **Domain không gọi `IEventBus`** — aggregate collect events nội bộ, UseCase publish sau khi thực thi.

---

## Application Layer — Runtime State

### GameSession (single source of truth)

Giữ toàn bộ state đang chơi. Presentation và UseCase đọc/ghi qua đây — không scatter state trong Presenter.

```text
GameSession
├── Player
├── CurrentDay, Difficulty
├── DayFlags                    # HasExplored, HasRested (reset mỗi ngày)
├── ActiveCombatSession?        # null khi không trong combat
├── IsBossDay                   # computed hoặc set bởi AdvanceDayUseCase
├── MetaProgression             # Sprint 4+
└── ClearDomainEvents()         # drain events từ aggregates → publish qua IEventBus
```

**Lifecycle combat:** `CombatUseCase` tạo `CombatSession`, gán vào `GameSession.ActiveCombatSession`. Khi `IsFinished`, clear session, sync HP player, apply loot nếu thắng.

### GameState (snapshot cho Save)

Immutable-friendly DTO — tách khỏi runtime mutability của `GameSession`.

```text
GameState
├── PlayerSaveData (hp, base stats, gold)
├── InventorySaveData[]         # { itemId, quantity } — không serialize object graph
├── EquipmentSaveData           # { weaponId?, armorId?, ringId? }
├── CurrentDay, Difficulty
├── DayFlags
├── MetaProgression data
├── ToSaveData() / FromSaveData(ItemFactory)
└── Restore(GameSession)        # LoadGameUseCase
```

---

## Domain Layer (OOP Core)

### Value Objects

Immutable, không có identity — tránh primitive obsession.

```text
Health          # Current, Max — TakeDamage(), Heal(), IsDead
Stats           # ATK, DEF, CRIT — ApplyModifier(), Combine()
Damage          # Amount, DamageType
ItemStack       # ItemId + quantity (reference Item qua factory khi cần)
Gold            # Amount — Sprint 3 (Merchant)
```

### Entity Hierarchy (Characters)

```text
Character (abstract)
├── Health, Stats, StatusEffectCollection
├── IDamageable — TakeDamage(Damage), IsAlive
│
├── Player
│   ├── Inventory, EquipmentLoadout, Gold
│   ├── GetEffectiveStats()     # base Stats + equipment + active buffs
│   ├── Rest()                  # heal theo quy tắc ngày
│   └── CanPerformDayAction()   # check DayFlags
│
├── Enemy
│   ├── IEnemyAI                # chọn ICombatCommand mỗi lượt
│   └── LootTable
│
└── Boss : Enemy
    ├── IBossPhase _currentPhase
    ├── CheckPhaseTransition()  # gọi sau khi nhận damage trong CombatSession
    └── BossSkillSet            # data refs → SkillDefinition
```

**Combat actions:** Mọi hành động combat (player, enemy, boss) đi qua **`ICombatCommand` duy nhất** — không dùng `ICombatAction` riêng. Enemy/Boss AI **trả về** command, `CombatSession.Execute()` xử lý thống nhất.

**Composition:** Variants (FireGoblin, IceGoblin…) = data + `IEnemyAI`, không subclass sâu.

### Combat

```text
CombatSession (aggregate)
├── Player, Enemy/Boss
├── CombatState (State pattern)
│   ├── PlayerTurnState         # chờ input từ Presentation
│   ├── EnemyTurnState          # auto: IEnemyAI → Execute(command)
│   ├── ResolveEffectsState     # tick status effects end-of-round
│   └── CombatEndState
├── Execute(ICombatCommand) → CommandResult
├── PendingDomainEvents[]       # EnemyDefeated, BossPhaseChanged, ...
├── IsFinished, Winner, Fled
└── OnEnemyDamaged → Boss.CheckPhaseTransition()

ICombatCommand (Command pattern)
├── AttackCommand               # Sprint 1
├── FleeCommand                 # Sprint 1 — set Fled, end combat
├── UseItemCommand              # Sprint 2 — consumable từ inventory
└── SkillCommand                # Sprint 2+ — ref SkillDefinition id

Domain Services (Combat/)
├── DamageCalculator            # input: effective Stats, output: Damage
├── CriticalCalculator          # Sprint 2+ (Sprint 1: crit cố định hoặc bỏ)
└── StatusEffectResolver        # Sprint 3+
```

**Trách nhiệm:** `CombatSession` giữ state và luật lượt. `DamageCalculator` nhận **effective stats** từ `Player.GetEffectiveStats()`, không đọc equipment trực tiếp.

### Skills (Sprint 2+)

```text
SkillDefinition (data)
├── Id, Name, Multiplier, CooldownTurns
└── Loaded từ config / hardcode registry

PlayerSkillState
├── Cooldowns per skill id
└── CanUse(skillId), MarkUsed(skillId)
```

Sprint 1: menu Skill ẩn hoặc map tạm sang `AttackCommand` × 1.5.

### Items (Polymorphism)

```text
Item (abstract)
├── Id, Name, Description
├── abstract ItemUseResult Use(IItemContext)
├── abstract bool CanEquip(IEquipmentContext)
│
├── Weapon    → WeaponSlot, stat modifier
├── Armor     → ArmorSlot
├── Ring      → RingSlot
├── Consumable → Use() — heal, buff
└── Artifact  → passive khi equipped (Sprint 3+)

ItemFactory / ItemRegistry
└── Create(itemId) → Item       # dùng cho loot, save/load, merchant
```

### Inventory & Equipment (Aggregates)

```text
Inventory (aggregate root)
├── InventorySlot[]
├── Add(itemId, qty), Remove(), HasSpace()
├── StackOrAdd()
└── enforce capacity nội bộ

EquipmentLoadout (aggregate root)
├── WeaponSlot, ArmorSlot, RingSlot
├── Equip(Item) → unequip old, return unequipped item to caller
├── Unequip(Slot)
└── GetStatsModifier() → Stats   # modifier only; Player combines
```

`EquipItemUseCase`: equip → nếu có item cũ → `Inventory.Add` item cũ (fail nếu full).

### Boss (Strategy)

```text
Boss
├── IBossPhase _currentPhase
├── CheckPhaseTransition()      # HP threshold → đổi phase, emit BossPhaseChanged
└── IEnemyAI → phase chọn command

IBossPhase (Strategy)
├── Phase1Behavior
├── Phase2Behavior
└── EnragedBehavior

BossFactory + BossDatabase      # data-driven: HP, thresholds, skill ids
```

### World & Explore

```text
IExploreEncounter
├── EnemyEncounter    → EncounterResult.StartCombat(enemyId)
├── ChestEncounter    → EncounterResult.GrantLoot(lootRoll)
├── MerchantEncounter → EncounterResult.OpenShop (Sprint 3 — cần Gold)
├── EventEncounter    → EncounterResult.ApplyEffect (Sprint 3)
└── EmptyEncounter    → EncounterResult.None

ExploreResolver
└── Resolve(encounter, ExploreContext) → EncounterResult

EncounterTable                  # weighted list theo Difficulty
└── PickRandom(IRandomService) → IExploreEncounter
```

**Loot:** `EncounterResult` / `CombatSession` emit `LootGranted` hoặc `EnemyDefeated`. `ApplyLootUseCase` (hoặc logic trong `CombatUseCase`/`ExploreUseCase`) gọi `Inventory.Add` — Sprint 1 có thể inline, Sprint 2+ qua event handler.

### Domain Events

Pure records trong Domain — **không** reference `IEventBus`.

```text
IDomainEvent
├── EnemyDefeated(EnemyId, LootTableId)
├── LootGranted(IReadOnlyList<ItemStack>)
├── ChestOpened(LootTableId)
├── DayAdvanced(int DayNumber)
├── BossPhaseChanged(BossId, PhaseId)
├── BossDefeated(BossId, LootTableId)
├── ItemEquipped(ItemId, Slot)
├── PlayerDied()
└── GameSaved()

Luồng publish:
  Aggregate thêm vào PendingDomainEvents
       → UseCase drain sau Execute
       → IEventBus.Publish (Application interface, Infrastructure implement)
       → Handlers: logging, UI messages (Sprint 4: auto-save optional)
```

```text
IEventBus                         # Application/Interfaces
├── Publish(IDomainEvent)
└── Subscribe<T>(Action<T>)

InMemoryEventBus                  # Infrastructure
```

---

## Application Layer (Use Cases)

Orchestrate Domain + `GameSession` — không chứa business rules (damage formula, equip rules, …).

```text
StartGameUseCase          → new GameSession, new Player, day = 1
ExploreUseCase            → check DayFlags → pick encounter → ExploreResolver
                            → Enemy: start CombatUseCase
                            → Chest: ApplyLoot inline
CombatUseCase             → create/run CombatSession on GameSession
                            → map input → ICombatCommand
                            → on win: ApplyLoot; on flee: no loot
RestUseCase               → check DayFlags → Player.Rest()
EquipItemUseCase          → EquipmentLoadout.Equip + return old to Inventory
UseItemUseCase            → Item.Use() (ngoài combat — menu inventory)
AdvanceDayUseCase         → reset DayFlags, increment day, set IsBossDay
BossBattleUseCase         → CombatUseCase với Boss từ BossFactory
ApplyLootUseCase          → Inventory.Add từ LootTable roll (shared)
SaveGameUseCase           → GameSession → GameState → ISaveRepository
LoadGameUseCase           → ISaveRepository → GameState → GameSession
GameOverUseCase           → clear session, emit PlayerDied handling
```

### DTOs (Presentation boundary)

```text
CombatResultDto           # log lines, winner, fled, playerHp
ExploreResultDto          # encounter type, narrative text
GameplayMenuDto           # stats, day, boss countdown, flags (explored/rested)
InventoryDto, EquipmentDto
```

---

## Infrastructure Layer

```text
JsonSaveRepository    : ISaveRepository
RandomService         : IRandomService
InMemoryEventBus      : IEventBus
FileLogger            : ILogger
```

---

## Presentation Layer

Chỉ I/O — render text, đọc input, gọi UseCase, hiển thị DTO.

```text
ConsoleGamePresenter    # DI wiring, screen routing, owns GameSession ref
MainMenuScreen          # New / Continue / Exit (Settings = stub Sprint 1)
GameplayScreen          # menu ngày — gọi UseCase, không Next Day nếu chưa explore/rest xong tùy design
CombatScreen            # loop: read command → CombatUseCase → render CombatResultDto
InventoryScreen
EquipmentScreen
BossScreen              # CombatScreen + phase UI (có thể reuse)
InputReader
OutputWriter
```

Presentation **không** gọi `DamageCalculator`, `Inventory.Add()`, hay `CombatSession.Execute()` trực tiếp.

**Screen routing:**

```text
MainMenu → (New/Load) → GameplayScreen
GameplayScreen → Explore → CombatScreen → (end) → GameplayScreen
GameplayScreen → Boss day → CombatScreen (boss) → GameplayScreen / GameOver
GameplayScreen → Next Day → AdvanceDayUseCase → GameplayScreen (or BossScreen)
```

---

## Design Patterns

| Pattern | Dùng ở đâu | Lý do |
|---|---|---|
| **Command** | `ICombatCommand` | Một pipeline cho player/enemy/boss; test, log, replay |
| **Strategy** | `IBossPhase`, `IEnemyAI` | Đổi hành vi không sửa CombatSession |
| **State** | `CombatState` | Luật lượt tách khỏi entity |
| **Factory** | `ItemFactory`, `BossFactory`, `EnemyFactory` | Tạo entity từ id + difficulty |
| **Observer** | Domain Events + `IEventBus` | UI messages, optional auto-save |
| **Aggregate** | `Inventory`, `EquipmentLoadout`, `CombatSession` | Invariants tập trung |

---

## Gameplay Loop

```text
Start Game
     │
     ▼
Main Menu ──Continue──► LoadGameUseCase
     │
     New Game
     ▼
Create Player (StartGameUseCase) → GameSession
     │
     ▼
┌─ Day Loop ─────────────────────────────────────────────┐
│  Gameplay Screen                                        │
│    ├── Inventory / Equipment (anytime, free)           │
│    ├── Explore (1×/day) ──► Encounter                  │
│    │       ├── Enemy → Combat → Loot (if win)          │
│    │       ├── Chest → Loot                            │
│    │       ├── Merchant (Sprint 3)                     │
│    │       └── Empty / Event                           │
│    ├── Rest (1×/day, mutually exclusive w/ Explore*)  │
│    ├── Save                                            │
│    └── Next Day → AdvanceDayUseCase                     │
│              │                                          │
│              ▼                                          │
│         Boss day? ──No──► Day Loop                      │
│              │                                          │
│             Yes                                         │
│              ▼                                          │
│         BossBattle → Win: loot + Difficulty++ + day=1  │
│                   → Lose: GameOver → Main Menu         │
└─────────────────────────────────────────────────────────┘

* Sprint 1 đơn giản: Explore OR Rest mỗi ngày (chọn một).
```

---

## UI Mockup (Console)

### Main Menu

```text
=========================================
        LUNAR
=========================================

1. New Game
2. Continue
3. Exit

>
```

(Settings defer — Sprint 4+)

### Gameplay Screen

```text
=========================================
Day : 3

HP  : 120 / 120
ATK : 18  (effective — gồm equipment)
DEF : 10
Gold: 50

Boss trong: 2 ngày
Hôm nay: [Chưa explore/rest]
-----------------------------------------

1. Explore          (1 lần/ngày)
2. Rest             (1 lần/ngày)
3. Inventory
4. Equipment
5. Next Day
6. Save
7. Exit to Menu

>
```

### Combat Screen

```text
====================================
Player
HP : 100    ATK: 15
------------------------------------
Goblin
HP : 40
------------------------------------
1. Attack
2. Item             (Sprint 2)
3. Run
>

Player attacks!
Damage : 15
Goblin HP : 25

Goblin attacks!
Damage : 4
Player HP : 96
```

### Boss Battle

```text
====================================
Skeleton King
Phase : 1
HP : 450 / 450
------------------------------------
1. Attack
2. Skill            (Sprint 2)
3. Item
4. Run
------------------------------------

Boss uses Fireball!
Player takes 18 damage!
```

---

## Module Structure (chi tiết)

```text
Domain/
├── Characters/
│   ├── Character.cs, Player.cs, Enemy.cs, Boss.cs
│   ├── Health.cs, Stats.cs, Gold.cs
│   └── StatusEffectCollection.cs
│
├── Combat/
│   ├── CombatSession.cs, ICombatCommand.cs
│   ├── Commands/               # Attack, Flee, UseItem, Skill
│   ├── States/
│   ├── IEnemyAI.cs
│   ├── DamageCalculator.cs
│   ├── CriticalCalculator.cs   # Sprint 2+
│   └── StatusEffect.cs         # Sprint 3+
│
├── Inventory/
│   ├── Inventory.cs, InventorySlot.cs
│   └── EquipmentLoadout.cs
│
├── Items/
│   ├── Item.cs, ItemFactory.cs, ItemRegistry.cs
│   └── Weapon.cs, Armor.cs, Ring.cs, Consumable.cs
│
├── Skills/                     # Sprint 2+
│   └── SkillDefinition.cs, PlayerSkillState.cs
│
├── Bosses/
│   ├── IBossPhase.cs, Phases/
│   ├── BossFactory.cs, BossDatabase.cs
│   └── BossSkill.cs
│
├── World/
│   ├── IExploreEncounter.cs, Encounters/
│   ├── ExploreResolver.cs
│   └── EncounterTable.cs
│
└── Events/
    ├── IDomainEvent.cs
    └── Events/

Application/
├── GameSession.cs
├── GameState.cs
├── UseCases/
├── Interfaces/
│   ├── IEventBus.cs
│   ├── ISaveRepository.cs
│   └── IRandomService.cs
└── DTOs/

Infrastructure/
├── JsonSaveRepository.cs
├── RandomService.cs
├── InMemoryEventBus.cs
└── FileLogger.cs

Presentation/
├── ConsoleGamePresenter.cs
├── Screens/
├── InputReader.cs
└── OutputWriter.cs
```

---

## Sprint Roadmap (OOP-first)

### Sprint 1 — Nền + Combat + Loop ngày

- Value objects: `Health`, `Stats`, `Damage`
- `Character`, `Player`, `Enemy`, `Player.GetEffectiveStats()`
- `GameSession`, `CombatSession`, `AttackCommand`, `FleeCommand`, `CombatState` (4 states)
- `DamageCalculator` (no crit hoặc crit cố định)
- `EncounterTable` + `EnemyEncounter` only
- Use cases: `StartGame`, `Explore`, `Combat`, `Rest`, `AdvanceDay`
- `GameplayScreen` + `CombatScreen` + loop 1 ngày (Explore **hoặc** Rest → Next Day)
- Loot đơn giản: inline `Inventory.Add` khi thắng (hardcode 1 item)

### Sprint 2 — Inventory & Equipment

- `Item` polymorphism + `ItemFactory` + save schema `{ itemId, qty }`
- `Inventory`, `EquipmentLoadout`, `EquipItemUseCase`
- `UseItemCommand`, `UseItemUseCase`
- `SkillDefinition` + `SkillCommand` (optional)
- `ApplyLootUseCase`, domain events drain pattern
- `CriticalCalculator`

### Sprint 3 — Boss & World

- `Boss`, `IBossPhase`, `CheckPhaseTransition`
- Encounters: Chest, Event, Empty; `ChestOpened` / `LootGranted` events
- `Gold`, `MerchantEncounter`
- `BossBattleUseCase`, `BossFactory`, `BossDatabase`
- `StatusEffectResolver` (nếu cần)

### Sprint 4 — Save & Meta

- `GameState` / `JsonSaveRepository`, Save/Load/New Game Continue
- `MetaProgression`, difficulty scaling data-driven
- Event handlers: optional auto-save on `DayAdvanced`
- Settings stub → volume/log level (console)

---

## Chuyển sang Unity

Business Logic (`Lunar.Core`) giữ nguyên.

```text
Lunar.Core
├── Domain/          ← copy nguyên
└── Application/     ← copy nguyên (GameSession, UseCases, DTOs)

Lunar.Console              Lunar.Unity
├── Infrastructure              ├── UnitySaveRepository : ISaveRepository
└── Presentation (Console)      └── Presentation (MonoBehaviour + UI)
                                      ├── GameplayPresenter → GameSession + UseCases
                                      ├── CombatPresenter   → CombatUseCase
                                      └── InventoryUI       → EquipItemUseCase
```

Chỉ thay **Presentation** và **Infrastructure bindings**.

---

## So sánh: Manager-centric vs OOP

| Tiêu chí | Trước (Manager) | Sau (OOP) |
|---|---|---|
| Runtime state | Rải rác trong Manager | `GameSession` tập trung |
| Logic combat | `CombatManager` | `CombatSession` + `ICombatCommand` |
| Effective stats | Tính tay nhiều nơi | `Player.GetEffectiveStats()` |
| Thêm content | Sửa switch lớn | Factory + data id |
| Unit test | Mock Manager | Test command/aggregate riêng |
| Unity migration | Copy Manager | Copy `Lunar.Core` |

---

## Lợi ích

- Prototype gameplay nhanh trên Console — Sprint 1 đã chơi được end-to-end
- Test logic trước Unity — không cần Editor
- Tái sử dụng **~80–90%** Domain + Application
- Gameplay rules explicit — implementer không phải đoán loop
- Mở rộng content bằng data id + factory

---

## Solution Structure

```text
Lunar/
├── Lunar.Core           # Domain + Application
├── Lunar.Console        # Infrastructure + Presentation (Console)
├── Lunar.Unity          # (sau) Infrastructure + Presentation (Unity)
├── Docs/
│   ├── SRS.md
│   ├── Architecture.md
│   ├── Backlog.md
│   ├── Console_Prototype_Architecture.md
│   └── Risk_Register.md
└── README.md
```

### Vai trò các Project

| Project | Vai trò |
|---|---|
| **Lunar.Core** | Domain, GameSession, UseCases, interfaces, DTOs |
| **Lunar.Console** | Prototype Console — JSON save, text UI, DI wiring |
| **Lunar.Unity** | Presentation Unity — MonoBehaviour, UI, asset binding |
| **Docs** | SRS, Architecture, Backlog, Risk Register |

---

## Tài liệu liên quan (nên bổ sung)

- Class Diagram (Domain entities + GameSession)
- Sequence Diagram: Combat turn, Explore → Combat → Loot, AdvanceDay → Boss
- Data Dictionary: Item/Boss/Encounter tables (id → stats)
- Save format JSON schema (`GameState` v1)
- Event catalog: event → handler → side effect

Các diagram trên mô tả chi tiết implementation của tài liệu này.
