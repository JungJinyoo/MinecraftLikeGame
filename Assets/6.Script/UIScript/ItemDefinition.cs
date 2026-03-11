using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemDefinition")]
public class ItemDefinition : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 64;
    public ItemType itemType;
    public float hungerRestoreAmount;
}