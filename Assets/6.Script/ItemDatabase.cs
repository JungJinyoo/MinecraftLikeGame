using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<ItemType, ItemDefinition> dict;
    private static Dictionary<string, ItemDefinition> dictByName;

    static ItemDatabase()
    {
        dict = new Dictionary<ItemType, ItemDefinition>();
        dictByName = new Dictionary<string, ItemDefinition>();
        LoadAllItems();
    }

    private static void LoadAllItems()
    {
        // "Resources/Item" 폴더에 있는 ItemDefinition들을 전부 자동으로 로드
        var items = Resources.LoadAll<ItemDefinition>("AllItem");
        foreach (var item in items)
        {
            if (!dict.ContainsKey(item.itemType))
                dict[item.itemType] = item;
            if (!dictByName.ContainsKey(item.itemName))
                dictByName[item.itemName] = item;
        }

        Debug.Log($"[ItemDatabase] Loaded {dict.Count} items.");
    }

    public static ItemDefinition GetDefinition(ItemType type)
    {
        return dict.TryGetValue(type, out var def) ? def : null;
    }
    public static ItemDefinition GetItemByName(string name)
    {
        return dictByName.TryGetValue(name, out var def) ? def : null;
    }
}
