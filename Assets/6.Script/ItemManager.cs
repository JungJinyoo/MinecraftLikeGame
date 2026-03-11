using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//확인용 enum
//public enum VoxelType { Air, Grass, Dirt, Stone, Water, Ore, Plank, CraftBox, Furnace }
//농사는 제외할것.
public enum ItemType
{
    NONE,
    /*Block*/
    WOOD, PLANK, STONE, IRON_BLOCK, COBBLESTONE, CRAFTTABLE, FURANCE, COAL_BLOCK,
    GRASS,DIRT,
    /*ITEM*/
    STICK, IRON, COAL,
    /*TOOL*/
    IRON_PICKAXE,
    STONE_SHOVEL, STONE_SWORD, STONE_PICKAXE, STONE_AXE,
    WOOD_PICKAXE, WOOD_AXE, WOOD_SHOVEL, WOOD_SWROD, HAND,
    /*MonsterDropItem*/
    ZOMBIE_MEAT, SKELETON_BORN, CREEPER_POWDER, SPIDER_WEB,
    /*FOOD*/
    RAW_COWMEAT, BAKED_COWMEAT, RAW_PIGMEAT, BAKED_PIGMEAT,
    /*ARMOR*/
    IRON_HELMAT, IRON_CHAST, IRON_REGGINGS, IRON_BOOTS
};

public enum Tooltag
{
    PICKAXE,AXE,SHOVEL,SWORD,NULL
}
public class Tool
{
    public ItemType type;
    public Tooltag tag;
    public float damage;
    public float durability;

    public Tool(ItemType type, Tooltag tag, float damage, float durability)
    {
        this.type = type;
        this.tag = tag;
        this.damage = damage;
        this.durability = durability;
    }
}

public class armor
{
    public ItemType type;
    public float defens;
    public float durability;

    public armor(ItemType type, float defens, float durability)
    {
        this.type = type;
        this.defens = defens;
        this.durability = durability;
    }
}

public static class armorstat
{
    public static Dictionary<ItemType, (float defens, float durability)> armorstats =
    new Dictionary<ItemType, (float, float)>()
    {
        /*IRON_ARMOR*/ //가죽이 따로 없으니 철방어구만
        //순서대로 방어력,내구도
        {ItemType.IRON_HELMAT,(2,165)},
        {ItemType.IRON_CHAST,(6,165)},
        {ItemType.IRON_REGGINGS,(5,165)},
        {ItemType.IRON_BOOTS,(2,165)},
    };
}
public class Item
{
    public ItemType type;
    public bool eatable; //먹기가 가능한지 여부
    public int maxnum; //최대 홀드 갯수
    public float full; //허기채워지는 수치
    public Item(ItemType type, bool eatable, int maxnum, float full)
    {
        this.type = type;
        this.eatable = eatable;
        this.maxnum = maxnum;
        this.full = full;
    }
}
public static class itemstat
{
    public static Dictionary<ItemType, (bool eatable, int maxnum, float full)> itemstats =
    new Dictionary<ItemType, (bool, int, float)>()
    {
        //순서대로 먹는것이 가능한지,겹쳐질수 있는 갯수, 허기채워지는 수치
        /*Item*/
        {ItemType.STICK,(false,64,0)},
        {ItemType.COAL,(false,64,0)},
        {ItemType.IRON,(false,64,0)},
        /*BLOCK*/
        {ItemType.PLANK,(false,64,0)},
        {ItemType.STONE,(false,64,0)},
        {ItemType.COBBLESTONE,(false,64,0)},
        {ItemType.WOOD,(false,64,0)},
        {ItemType.CRAFTTABLE,(false,64,0)},
        {ItemType.FURANCE,(false,64,0)},
        {ItemType.IRON_BLOCK,(false,64,0)},
        {ItemType.COAL_BLOCK,(false,64,0)},
        {ItemType.GRASS,(false,64,0)},
        {ItemType.DIRT,(false,64,0)},
        /*FOOD*/
        {ItemType.RAW_COWMEAT,(true,64,3)},
        {ItemType.BAKED_COWMEAT,(true,64,35)},
        {ItemType.RAW_PIGMEAT,(true,64,3)},
        {ItemType.BAKED_PIGMEAT,(true,64,30)},
        /*MonsterDropItem*/
        {ItemType.SPIDER_WEB,(false,64,0)},
        {ItemType.ZOMBIE_MEAT,(true,64,1)},
        {ItemType.SKELETON_BORN,(false,64,0)},
        {ItemType.CREEPER_POWDER,(false,64,0)},
    };
}

public static class ToolStat
{
    public static Dictionary<ItemType, (Tooltag tag, float damage, float durability)> toolstats =
    new Dictionary<ItemType, (Tooltag, float, float)>()
    {   
        //순서대로 용도, 데미지,내구도
        /*Defalut*/
        {ItemType.HAND,(Tooltag.NULL,0.5f,-1)},
        /*PICKAXE*/
        {ItemType.WOOD_PICKAXE,(Tooltag.PICKAXE,2,59)},
        {ItemType.STONE_PICKAXE,(Tooltag.PICKAXE,3,131)},
        {ItemType.IRON_PICKAXE,(Tooltag.PICKAXE,4,250)},
        /*AXE*/
        {ItemType.STONE_AXE,(Tooltag.AXE,3,131)},
        {ItemType.WOOD_AXE,(Tooltag.AXE,2,59)},
        /*SHOVEL*/
        {ItemType.STONE_SHOVEL,(Tooltag.SHOVEL,2,131)},
        {ItemType.WOOD_SHOVEL,(Tooltag.SHOVEL,2,59)},
        /*SWORD*/
        {ItemType.STONE_SWORD,(Tooltag.SWORD,5,131)},
        {ItemType.WOOD_SWROD,(Tooltag.SWORD,4,59)},
        //나무도구 차후 추가
    };
}

public class Recipe
{

}
