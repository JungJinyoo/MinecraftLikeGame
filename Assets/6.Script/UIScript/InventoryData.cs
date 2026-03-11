using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Data/InventoryData")]
public class InventoryData : ScriptableObject
{
    [SerializeField] private List<ItemStack> slots = new List<ItemStack>();
    [SerializeField] private List<ItemStack> Craftingslots = new List<ItemStack>();

    [SerializeField] private List<ItemStack> Stoveslots = new List<ItemStack>();

    public int SlotCount => slots.Count;
    public int CslotCount => Craftingslots.Count;
    public int SslotCount => Stoveslots.Count;

    public void Init(int slotCount)
    { 
        if (slots.Count >= slotCount) return;
        slots.Clear();
        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new ItemStack());
        }

    }

    // public ItemStack GetSlot(int index) => slots[index];
    // public void SetSlot(int index, ItemStack stack) => slots[index] = stack;
    public ItemStack GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return new ItemStack();
        }

        return slots[index];
    }
    

public void SetSlot(int index, ItemStack stack)
{
    if (index < 0 || index >= slots.Count)
    {
        Debug.LogError($"[InventoryData.SetSlot] ❌ 잘못된 index 접근: index={index}, slots.Count={slots.Count}");
        return;
    }

    slots[index] = stack;
}
    public void Swap(int a, int b)
    {
        var temp = slots[a];
        slots[a] = slots[b];
        slots[b] = temp;
    }
    // public void Minus(int slotCount)
    // {
    //     if (slots[slotCount].IsEmpty) return;

    //     slots[slotCount].count--;

    //     if (slots[slotCount].count <= 0)
    //     {
    //         slots[slotCount] = new ItemStack(); // 빈칸 처리
    //     }
    // }
    public void Minus(int slotIndex)
{
    if (slotIndex < 0 || slotIndex >= slots.Count)
    {
        Debug.LogError($"[InventoryData.Minus] ❌ 잘못된 index: index={slotIndex}, slots.Count={slots.Count}");
        return;
    }

    Debug.Log($"[InventoryData.Minus] slotIndex={slotIndex}, 현재 수량={slots[slotIndex].count}");

    if (slots[slotIndex].IsEmpty) return;

    slots[slotIndex].count--;

    if (slots[slotIndex].count <= 0)
    {
        slots[slotIndex] = new ItemStack();
        Debug.Log($"[InventoryData.Minus] slotIndex={slotIndex} → 빈칸 처리됨");
    }
}

    //크래프팅용
    public void CInit(int slotCount)
    {
        if (Craftingslots.Count >= slotCount) return;
        Craftingslots.Clear();
        for (int i = 0; i < slotCount; i++)
            Craftingslots.Add(new ItemStack());
    }
    public ItemStack CGetSlot(int index)
    {
        if (index < 0 || index >= Craftingslots.Count)
        {
            return new ItemStack();
        }

        return Craftingslots[index];
    }
    public void CSetSlot(int index, ItemStack stack) => Craftingslots[index] = stack;

    public void CMinus(int slotCount)
    {
        if (Craftingslots[slotCount].IsEmpty) return;

        Craftingslots[slotCount].count--;

        if (Craftingslots[slotCount].count <= 0)
        {
           Craftingslots[slotCount] = new ItemStack(); // 빈칸 처리
        }
    }

    //화로용
    public void SInit(int slotCount)
    {
        if (Stoveslots.Count >= slotCount) return;
        Stoveslots.Clear();
        for (int i = 0; i < slotCount; i++)
            Stoveslots.Add(new ItemStack());
    }
    public void SMinus(int slotCount)
    {
        if (Stoveslots[slotCount].IsEmpty) return;

        Stoveslots[slotCount].count--;

        if (Stoveslots[slotCount].count <= 0)
        {
            Stoveslots[slotCount] = new ItemStack(); // 빈칸 처리
        }
    }

    public ItemStack SGetSlot(int index) => Stoveslots[index];
    public void SSetSlot(int index, ItemStack stack) => Stoveslots[index] = stack;

}