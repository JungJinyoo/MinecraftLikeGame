using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//여기는 그냥 데이터구조
public class Crafting : MonoBehaviour
{

    public craftingmanager manager = new craftingmanager();

    void Start()
    {
        // 스틱 (Plank 2개 세로)
        manager.addrecipe(new craftrecipe
        {
            width = 1,
            height = 2,
            pattern = new ItemType[2, 1] {
            { ItemType.PLANK },
            { ItemType.PLANK }
        },
            result = ItemType.STICK,
            resultcount = 4
        });

        // 나무판자 (나무1개 1x1)
        manager.addrecipe(new craftrecipe
        {
            width = 1,
            height = 1,
            pattern = new ItemType[1, 1] {
            { ItemType.WOOD },
        },
            result = ItemType.PLANK,
            resultcount = 4
        });

        // 제작대
        manager.addrecipe(new craftrecipe
        {
            width = 2,
            height = 2,
            pattern = new ItemType[2, 2] {
            { ItemType.PLANK,ItemType.PLANK },
            { ItemType.PLANK,ItemType.PLANK }
        },
            result = ItemType.CRAFTTABLE,
            resultcount = 1
        });

        //화로
        manager.addrecipe(new craftrecipe
        {
            width = 3,
            height = 3,
            pattern = new ItemType[3, 3] {
            { ItemType.COBBLESTONE, ItemType.COBBLESTONE, ItemType.COBBLESTONE },
            { ItemType.COBBLESTONE,  ItemType.NONE, ItemType.COBBLESTONE },
            { ItemType.COBBLESTONE,  ItemType.COBBLESTONE, ItemType.COBBLESTONE }
        },
            result = ItemType.FURANCE,
            resultcount = 1
        });
        //돌 도끼
        manager.addrecipe(new craftrecipe
        {
            width = 3,
            height = 3,
            pattern = new ItemType[3, 3] {
            { ItemType.COBBLESTONE, ItemType.COBBLESTONE, ItemType.NONE },
            { ItemType.COBBLESTONE,  ItemType.STICK, ItemType.NONE },
            { ItemType.NONE,  ItemType.STICK, ItemType.NONE }
        },
            result = ItemType.STONE_AXE,
            resultcount = 1
        });
        //돌 곡괭이
        manager.addrecipe(new craftrecipe
        {
            width = 3,
            height = 3,
            pattern = new ItemType[3, 3] {
            { ItemType.COBBLESTONE, ItemType.COBBLESTONE, ItemType.COBBLESTONE },
            { ItemType.NONE,  ItemType.STICK, ItemType.NONE },
            { ItemType.NONE,  ItemType.STICK, ItemType.NONE }
        },
            result = ItemType.STONE_PICKAXE,
            resultcount = 1
        });
        //돌 삽
        manager.addrecipe(new craftrecipe
        {
            width = 1,
            height = 3,
            pattern = new ItemType[3, 1] {
            {ItemType.COBBLESTONE},
            {ItemType.STICK},
            {ItemType.STICK}
        },
            result = ItemType.STONE_SHOVEL,
            resultcount = 1
        });

        //돌 검
        manager.addrecipe(new craftrecipe
        {
            width = 1,
            height = 3,
            pattern = new ItemType[3, 1] {
            { ItemType.COBBLESTONE},
            { ItemType.COBBLESTONE},
            { ItemType.STICK}
        },
            result = ItemType.STONE_SWORD,
            resultcount = 1
        });

        // 나무 곡괭이 (Plank 3개 + Stick 2개)
        manager.addrecipe(new craftrecipe
        {
            width = 3,
            height = 3,
            pattern = new ItemType[3, 3] {
            { ItemType.PLANK, ItemType.PLANK, ItemType.PLANK },
            { ItemType.NONE,  ItemType.STICK, ItemType.NONE },
            { ItemType.NONE,  ItemType.STICK, ItemType.NONE }
        },
            result = ItemType.WOOD_PICKAXE,
            resultcount = 1
        });

        //나무 도끼
        manager.addrecipe(new craftrecipe
        {
            width = 3,
            height = 3,
            pattern = new ItemType[3, 3] {
            { ItemType.PLANK, ItemType.PLANK, ItemType.NONE },
            { ItemType.PLANK,  ItemType.STICK, ItemType.NONE },
            { ItemType.NONE,  ItemType.STICK, ItemType.NONE }
        },
            result = ItemType.WOOD_AXE,
            resultcount = 1
        });

        //나무 삽
        manager.addrecipe(new craftrecipe
        {
            width = 1,
            height = 3,
            pattern = new ItemType[3, 1] {
            { ItemType.PLANK },
            { ItemType.STICK },
            {  ItemType.STICK }
        },
            result = ItemType.WOOD_SHOVEL,
            resultcount = 1
        });

        //나무 검
        manager.addrecipe(new craftrecipe
        {
            width = 1,
            height = 3,
            pattern = new ItemType[3, 1] {
            { ItemType.PLANK },
            { ItemType.PLANK },
            {  ItemType.STICK }
        },
            result = ItemType.WOOD_SWROD,
            resultcount = 1
        });
    }
}
public class craftrecipe //레시피 저장공간
{
    public int width, height;
    public ItemType[,] pattern;
    //레시피의 핵심 패턴
    public ItemType result;
    //결과물
    public int resultcount;
    //갯수
}
public class Itemslot
{
    public ItemType type;
    //슬롯에서 가져올건 타입이면 충분 들어온 아이템의 타입 검사. 밑 레시피와 일치 검사
    public int count;

}
public class craftingmanager
{
    public List<craftrecipe> recipes = new List<craftrecipe>(); //레시피 저장.
    public void addrecipe(craftrecipe recipe)
    {
        recipes.Add(recipe); //레시피 주가 로직 나중에 start부분에서 레시피를 등록할수있게 할 것.
    }
    public Itemslot trycraft(ItemType[,] slot)
    {
        ItemType[,] normalized = normalize(slot);
        foreach (var recipe in recipes)
        {
            if (Match(normalized, recipe))
            {
                return new Itemslot { type = recipe.result, count = recipe.resultcount }; //해당 결과물을 result 공간에 보내줘야함.
            }
        }
        return null;
    }

private bool Match(ItemType[,] input, craftrecipe recipe)
{
    ItemType[,] pattern = recipe.pattern; 

    int inputHeight = input.GetLength(0);
    int inputWidth = input.GetLength(1);
    int patternHeight = pattern.GetLength(0);
    int patternWidth = pattern.GetLength(1);

    // input이 pattern보다 작으면 무조건 불일치
    if (inputHeight < patternHeight || inputWidth < patternWidth)
        return false;

    for (int y = 0; y < patternHeight; y++)
    {
        for (int x = 0; x < patternWidth; x++)
        {
            ItemType inputType = input[y, x];
            ItemType patternType = pattern[y, x];

            // 패턴이 NONE이면 input이 뭐든 상관없음
            if (patternType == ItemType.NONE)
                continue;

            if (inputType != patternType)
            {
                return false;
            }
        }
    }

    // 추가: pattern이 작고 input에 남는 칸이 있으면 NONE인지 확인
    for (int y = 0; y < inputHeight; y++)
    {
        for (int x = 0; x < inputWidth; x++)
        {
            if (y < patternHeight && x < patternWidth) continue; // 이미 검사됨
            if (input[y, x] != ItemType.NONE)
            {
                Debug.Log($"Extra item in input at ({y},{x}): {input[y,x]}");
                return false;
            }
        }
    }

    return true;
}

    //왼쪽위로 이동 로직.
    ItemType[,] normalize(ItemType[,] grid)
    {
        int height = grid.GetLength(0);
        int width = grid.GetLength(1);

        int minX = width, minY = height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[y, x] != ItemType.NONE)
                {
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                }
            }
        }

        // 결과 배열은 원래 크기 그대로 생성
        ItemType[,] normalized = new ItemType[height, width];

        for (int y = minY; y < height; y++)
        {
            for (int x = minX; x < width; x++)
            {
                normalized[y - minY, x - minX] = grid[y, x];
            }
        }

        return normalized;
    }


}


