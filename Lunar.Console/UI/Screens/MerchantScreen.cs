using Lunar.Core.Model;
using Lunar.Core.Model.Dto;
using Lunar.Core.Service;

namespace Lunar.Console.UI.Screens;

public sealed class MerchantScreen
{
    private readonly InputReader _input;
    private readonly OutputWriter _output;
    private readonly MerchantUseCase _merchant;

    public MerchantScreen(InputReader input, OutputWriter output, MerchantUseCase merchant)
    {
        _input = input;
        _output = output;
        _merchant = merchant;
    }

    public void Show(GameSession session)
    {
        while (true)
        {
            _output.WriteLine();
            _output.WriteHeader("Merchant");
            _output.WriteLine($"Your gold: {session.Player.Gold}");
            _output.WriteSeparator('-');

            var offers = _merchant.GetOffers();
            for (var i = 0; i < offers.Count; i++)
            {
                var o = offers[i];
                _output.WriteLine($"{i + 1}. {o.DisplayName} — Buy: {o.BuyPrice}g / Sell: {o.SellPrice}g");
            }

            _output.WriteLine();
            _output.WriteLine("1. Buy item");
            _output.WriteLine("2. Sell item");
            _output.WriteLine("0. Leave");
            var menu = _input.ReadChoice("> ", 0, 2);
            if (menu == 0) return;

            if (menu == 1)
                HandleBuy(session, offers);
            else
                HandleSell(session, offers);
        }
    }

    private void HandleBuy(GameSession session, IReadOnlyList<MerchantOffer> offers)
    {
        _output.WriteLine("Which item to buy?");
        for (var i = 0; i < offers.Count; i++)
            _output.WriteLine($"{i + 1}. {offers[i].DisplayName} ({offers[i].BuyPrice}g)");

        _output.WriteLine("0. Cancel");
        var choice = _input.ReadChoice("> ", 0, offers.Count);
        if (choice == 0) return;

        var result = _merchant.Buy(session, offers[choice - 1].ItemId);
        _output.WriteLine(result.Success ? result.Message : result.Error!);
        _output.Pause();
    }

    private void HandleSell(GameSession session, IReadOnlyList<MerchantOffer> offers)
    {
        var inventory = GameplayPresenterMapper.ToInventoryDto(session.Player)
            .Where(i => offers.Any(o => o.ItemId == i.ItemId))
            .ToList();

        if (inventory.Count == 0)
        {
            _output.WriteLine("Nothing to sell.");
            _output.Pause();
            return;
        }

        _output.WriteLine("Which item to sell?");
        for (var i = 0; i < inventory.Count; i++)
        {
            var item = inventory[i];
            var sellPrice = offers.First(o => o.ItemId == item.ItemId).SellPrice;
            _output.WriteLine($"{i + 1}. {item.DisplayName} x{item.Quantity} ({sellPrice}g each)");
        }

        _output.WriteLine("0. Cancel");
        var choice = _input.ReadChoice("> ", 0, inventory.Count);
        if (choice == 0) return;

        var result = _merchant.Sell(session, inventory[choice - 1].ItemId);
        _output.WriteLine(result.Success ? result.Message : result.Error!);
        _output.Pause();
    }
}
