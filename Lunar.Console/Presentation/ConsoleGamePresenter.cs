using Lunar.Console.Infrastructure;
using Lunar.Console.Presentation.Screens;
using Lunar.Core.Application;
using Lunar.Core.Application.Interfaces;
using Lunar.Core.Application.UseCases;
using Lunar.Core.Domain.Bosses;
using Lunar.Core.Domain.Events;
using Lunar.Core.Domain.Items;
using Lunar.Core.Domain.World;

namespace Lunar.Console.Presentation;

public sealed class ConsoleGamePresenter
{
    private readonly InputReader _input;
    private readonly OutputWriter _output;
    private readonly ISaveRepository _saveRepository;
    private readonly StartGameUseCase _startGame;
    private readonly LoadGameUseCase _loadGame;
    private readonly SaveGameUseCase _saveGame;
    private readonly ExploreUseCase _explore;
    private readonly CombatUseCase _combat;
    private readonly RestUseCase _rest;
    private readonly AdvanceDayUseCase _advanceDay;
    private readonly BossBattleUseCase _bossBattle;
    private readonly MainMenuScreen _mainMenu;
    private readonly GameplayScreen _gameplay;
    private readonly CombatScreen _combatScreen;
    private readonly InventoryScreen _inventory;
    private readonly EquipmentScreen _equipment;
    private readonly MerchantScreen _merchant;

    private GameSession? _session;

    public ConsoleGamePresenter(
        InputReader input,
        OutputWriter output,
        IRandomService random,
        IEventBus eventBus,
        ItemFactory itemFactory,
        ISaveRepository? saveRepository = null)
    {
        _input = input;
        _output = output;
        _saveRepository = saveRepository ?? new JsonSaveRepository();

        var enemyFactory = new EnemyFactory();
        var bossFactory = new BossFactory();
        var applyLoot = new ApplyLootUseCase(random, eventBus, itemFactory);

        _startGame = new StartGameUseCase(itemFactory);
        _loadGame = new LoadGameUseCase(_saveRepository, itemFactory);
        _saveGame = new SaveGameUseCase(_saveRepository, eventBus);
        _explore = new ExploreUseCase(random, applyLoot);
        _combat = new CombatUseCase(enemyFactory, bossFactory, applyLoot, eventBus, random);
        _rest = new RestUseCase();
        _advanceDay = new AdvanceDayUseCase(eventBus);
        _bossBattle = new BossBattleUseCase(_combat);

        _mainMenu = new MainMenuScreen(input, output);
        _gameplay = new GameplayScreen(input, output);
        _combatScreen = new CombatScreen(input, output);
        _inventory = new InventoryScreen(
            input, output,
            new UseItemUseCase(itemFactory),
            new EquipItemUseCase(itemFactory, eventBus));
        _equipment = new EquipmentScreen(output);
        _merchant = new MerchantScreen(input, output, new MerchantUseCase(itemFactory));

        eventBus.Subscribe<ItemEquipped>(e =>
            _output.WriteLine($"[Event] Equipped: {itemFactory.GetDisplayName(e.ItemId)}"));
        eventBus.Subscribe<BossPhaseChanged>(e =>
            _output.WriteLine($"[Event] Boss phase: {e.PhaseName}"));
        eventBus.Subscribe<DayAdvanced>(_ => AutoSave());
    }

    public void Run()
    {
        _output.WriteHeader("LUNAR");

        while (true)
        {
            var choice = _mainMenu.Show(_saveRepository.HasSave());
            switch (choice)
            {
                case MainMenuChoice.NewGame:
                    HandleNewGame();
                    break;
                case MainMenuChoice.Continue:
                    HandleContinue();
                    break;
                case MainMenuChoice.Exit:
                    return;
            }
        }
    }

    private void HandleNewGame()
    {
        if (_saveRepository.HasSave())
        {
            _output.WriteLine("Note: Existing save remains until you save again or game over.");
            _output.Pause();
        }

        _session = _startGame.Execute();
        RunGameLoop();
    }

    private void HandleContinue()
    {
        var result = _loadGame.Execute();
        if (!result.Success)
        {
            _output.WriteLine(result.Error!);
            _output.Pause();
            return;
        }

        _session = result.Session;
        _output.WriteLine(result.Message);
        _output.Pause();
        RunGameLoop();
    }

    private void RunGameLoop()
    {
        while (_session is not null)
        {
            if (_session.Player.Health.IsDead)
            {
                HandleGameOver();
                return;
            }

            var action = _gameplay.Show(GameplayPresenterMapper.ToMenuDto(_session));

            switch (action)
            {
                case GameplayAction.Explore:
                    HandleExplore();
                    break;
                case GameplayAction.Rest:
                    HandleRest();
                    break;
                case GameplayAction.Inventory:
                    _inventory.Show(_session);
                    break;
                case GameplayAction.Equipment:
                    _equipment.Show(_session);
                    break;
                case GameplayAction.FaceBoss:
                    HandleBoss();
                    break;
                case GameplayAction.NextDay:
                    HandleNextDay();
                    break;
                case GameplayAction.Save:
                    HandleSave(silent: false);
                    break;
                case GameplayAction.ExitToMenu:
                    _session = null;
                    return;
            }
        }
    }

    private void HandleExplore()
    {
        var result = _explore.Execute(_session!);
        _output.WriteLine(result.Message);

        if (!result.Success)
        {
            _output.WriteLine(result.Error!);
            _output.Pause();
            return;
        }

        if (result.EffectMessage is not null)
            _output.WriteLine(result.EffectMessage);

        if (result.StartsCombat && result.EnemyId is not null)
        {
            var start = _combat.StartCombat(_session!, result.EnemyId);
            if (!start.Success)
            {
                _output.WriteLine(start.Error!);
                _output.Pause();
                return;
            }

            _output.Pause();
            RunCombat(isBoss: false);
            return;
        }

        if (result.OpensMerchant)
        {
            _output.Pause();
            _merchant.Show(_session!);
            return;
        }

        _output.Pause();
    }

    private void HandleRest()
    {
        var result = _rest.Execute(_session!);
        _output.WriteLine(result.Success ? $"You rest and recover {result.HealedAmount} HP." : result.Error!);
        _output.Pause();
    }

    private void HandleBoss()
    {
        var start = _bossBattle.StartBossFight(_session!);
        if (!start.Success)
        {
            _output.WriteLine(start.Error!);
            _output.Pause();
            return;
        }

        _output.WriteLine("The Skeleton King blocks your path!");
        _output.Pause();
        RunCombat(isBoss: true);
    }

    private void HandleNextDay()
    {
        var result = _advanceDay.Execute(_session!);
        _output.WriteLine(result.Message);
        if (result.Success)
            _output.WriteLine("(Auto-saved)");
        _output.Pause();
    }

    private void HandleSave(bool silent)
    {
        var result = _saveGame.Execute(_session!, silent);
        if (!result.Success)
            _output.WriteLine(result.Error!);
        else if (!string.IsNullOrEmpty(result.Message))
            _output.WriteLine(result.Message);

        if (!silent)
            _output.Pause();
    }

    private void AutoSave()
    {
        if (_session is null) return;
        _saveGame.Execute(_session, silent: true);
    }

    private void HandleGameOver()
    {
        _output.WriteLine("\n=== GAME OVER ===");
        _saveRepository.DeleteSave();
        _output.WriteLine("Save deleted.");
        _output.Pause();
        _session = null;
    }

    private void RunCombat(bool isBoss)
    {
        while (_session!.ActiveCombatSession is not null)
        {
            var combat = _session.ActiveCombatSession;
            var action = _combatScreen.Show(_session, combat!);
            var result = ResolveCombatAction(action, isBoss);

            foreach (var line in result.LogLines)
                _output.WriteLine(line);

            if (!result.Success && result.Error is not null)
                _output.WriteLine(result.Error);

            if (result.LootMessage is not null)
                _output.WriteLine(result.LootMessage);

            if (result.CombatEnded)
            {
                if (result.PlayerDied)
                {
                    HandleGameOver();
                    return;
                }

                AutoSave();
                _output.Pause();
                return;
            }
        }
    }

    private Core.Application.DTOs.CombatResultDto ResolveCombatAction(CombatActionResult action, bool isBoss)
    {
        var session = _session!;
        return action.Action switch
        {
            CombatAction.Attack => isBoss ? _bossBattle.Attack(session) : _combat.Attack(session),
            CombatAction.Skill => isBoss ? _bossBattle.Skill(session) : _combat.Skill(session),
            CombatAction.Item => isBoss ? _bossBattle.UseItem(session, action.ItemId!) : _combat.UseItem(session, action.ItemId!),
            CombatAction.Flee => isBoss ? _bossBattle.Flee(session) : _combat.Flee(session),
            _ => new Core.Application.DTOs.CombatResultDto { Success = false, Error = "Invalid action." }
        };
    }
}
