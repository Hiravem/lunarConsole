namespace Lunar.Core.Model.Inventory;

public sealed class ItemStack
{
    public string ItemId { get; }
    public int Quantity { get; private set; }

    public ItemStack(string itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }

    public void Add(int amount) => Quantity += amount;
}
