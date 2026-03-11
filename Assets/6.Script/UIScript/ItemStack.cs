[System.Serializable]
public class ItemStack
{
    public ItemDefinition item;
    public int count;
    public int durability;
    public int slot_index;

    public bool IsEmpty => item == null || count <= 0;
    public ItemStack()
    {
        item = null;
        count = 0;
        durability = -1;
        slot_index = -1;
    }

    public ItemStack(ItemDefinition item, int count)
    {
        this.item = item;
        this.count = count;
    }

    public ItemStack(ItemDefinition item, int count, int durability)
    {
        this.item = item;
        this.count = count;
        this.durability = durability;
    }
    public ItemStack(ItemDefinition item, int count, int durability, int slot_index)
    {
        this.item = item;
        this.count = count;
        this.durability = durability;
        this.slot_index = slot_index;
    }

    // public ItemStack Clone()
    // {
    //     return new ItemStack(item, count, durability);
    // }
    public ItemStack Clone()
    {
        return new ItemStack(item, count, durability, slot_index);
    }

}