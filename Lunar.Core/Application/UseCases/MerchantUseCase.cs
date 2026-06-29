using Lunar.Core.Application.DTOs;
using Lunar.Core.Domain.Items;

namespace Lunar.Core.Application.UseCases;

public sealed class MerchantOffer
{
    public string ItemId { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public int BuyPrice { get; init; }
    public int SellPrice { get; init; }
}

public sealed class MerchantUseCase
{
    private readonly ItemFactory _itemFactory;

    private static readonly (string itemId, int buy, int sell)[] Catalog =
    {
        ("health_potion", 20, 10),
        ("rusty_dagger", 35, 15),
        ("iron_sword", 60, 30),
        ("leather_armor", 50, 25),
        ("copper_ring", 45, 20)
    };

    public MerchantUseCase(ItemFactory itemFactory) => _itemFactory = itemFactory;

    public IReadOnlyList<MerchantOffer> GetOffers() =>
        Catalog.Select(c => new MerchantOffer
        {
            ItemId = c.itemId,
            DisplayName = _itemFactory.GetDisplayName(c.itemId),
            BuyPrice = c.buy,
            SellPrice = c.sell
        }).ToList();

    public MerchantResultDto Buy(GameSession session, string itemId)
    {
        var offer = GetOffers().FirstOrDefault(o => o.ItemId == itemId);
        if (offer is null)
            return MerchantResultDto.Fail("Item not sold here.");

        if (!session.Player.TrySpendGold(offer.BuyPrice))
            return MerchantResultDto.Fail($"Not enough gold. Need {offer.BuyPrice}, have {session.Player.Gold}.");

        if (!session.Player.Inventory.Add(itemId))
        {
            session.Player.AddGold(offer.BuyPrice);
            return MerchantResultDto.Fail("Inventory full.");
        }

        return MerchantResultDto.Ok($"Bought {offer.DisplayName} for {offer.BuyPrice} gold.");
    }

    public MerchantResultDto Sell(GameSession session, string itemId)
    {
        var offer = GetOffers().FirstOrDefault(o => o.ItemId == itemId);
        if (offer is null)
            return MerchantResultDto.Fail("Merchant won't buy that item.");

        if (!session.Player.Inventory.Remove(itemId))
            return MerchantResultDto.Fail("You don't have that item.");

        session.Player.AddGold(offer.SellPrice);
        return MerchantResultDto.Ok($"Sold {offer.DisplayName} for {offer.SellPrice} gold.");
    }
}
