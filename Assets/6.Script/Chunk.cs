using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 블록(복셀)의 종류를 정의하는 열거형 (공기, 풀, 흙, 돌, 물, 광석, 제작대, 화로)
public enum VoxelType { Air, GRASS, DIRT, STONE, Water, COAL_BLOCK, PLANK, CRAFTTABLE, FURANCE, WOOD, IRON_BLOCK , COBBLESTONE,Wool,Chast, LEAF, RAW_COWMEAT }

public class Voxel
{
    public VoxelType type; //블럭의 타입
    public float hp; //블럭의 체력
    public float hardness; // 블럭의 경도
    public Tooltag currrenttool; // 블럭에 맞는 도구

    public Voxel(VoxelType type,Tooltag currenttool,float hp,float hardness)
    {
        this.type = type;
        this.currrenttool = currenttool;
        this.hp = hp;
        this.hardness = hardness;
        
    }
}

// 하나의 청크(Chunk)를 표현하는 클래스
public class Chunk
{
    public static int chunkSize = 16;   // 청크의 가로/세로 크기 (x, z 방향)
    public static int chunkHeight = 255; // 청크의 높이 (y 방향)
    public Voxel[,,] voxels;
    public Vector3 origin;

    [System.NonSerialized] public ChunkRenderer chunkrenderer;

    // 생성자: 청크 크기만큼 블록 배열 초기화
    public Chunk(Vector3 originPosition)
    {
        origin = originPosition;
        voxels = new Voxel[chunkSize, chunkHeight, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    voxels[x, y, z] = new Voxel(VoxelType.Air,Tooltag.NULL, 0, 0);
                }
            }
        }

    }
}

public static class VoxelStats
{
    public static Dictionary<VoxelType, (Tooltag currenttool,float hp, float hardness)> stats =
        new Dictionary<VoxelType, (Tooltag, float, float)>()
    {
        { VoxelType.GRASS, (Tooltag.SHOVEL,10, 0.6f) },
        { VoxelType.DIRT,  (Tooltag.SHOVEL,10, 0.5f) },
        { VoxelType.STONE, (Tooltag.PICKAXE,10, 3) },
        { VoxelType.Water, (Tooltag.NULL,10,  0) },
        { VoxelType.COAL_BLOCK,   (Tooltag.PICKAXE,10, 3) },
        { VoxelType.PLANK, (Tooltag.AXE,10, 2) },
        { VoxelType.CRAFTTABLE, (Tooltag.AXE,10, 2.5f) },
        { VoxelType.FURANCE,  (Tooltag.PICKAXE,10, 3.5f) },
        { VoxelType.WOOD,  (Tooltag.AXE,10, 2) },
        { VoxelType.IRON_BLOCK,  (Tooltag.PICKAXE,10, 3) },
        { VoxelType.COBBLESTONE, (Tooltag.PICKAXE,10, 3.5f) },
        { VoxelType.Wool, (Tooltag.NULL,10, 0.8f) },
        { VoxelType.Chast, (Tooltag.AXE,10, 2.5f) },
        {VoxelType.LEAF,(Tooltag.NULL,10,0.2f)}
    };
}

// 블록의 텍스처 좌표(타일맵에서의 위치)를 저장하는 클래스
public class BlockUV
{
    public Vector2Int top;     // 윗면 타일 좌표
    public Vector2Int side;    // 사이드 타일 좌표
    public Vector2Int left;    // 왼쪽면 타일 좌표
    public Vector2Int bottom;  // 밑면 타일 좌표
    public Vector2Int front; //정면 타일 좌표


    // 생성자: 위, 옆, 아래 좌표를 지정
    public BlockUV(Vector2Int t, Vector2Int s, Vector2Int b, Vector2Int f)
    {
        top = t; side = s; bottom = b; front = f;
    }
}

// 블록별 텍스처 좌표 모음 (타일맵 UV)
public static class BlockUVs
{
    public static int atlasSizeX = 32; // 텍스처 아틀라스의 가로 타일 개수
    public static int atlasSizeY = 16; // 텍스처 아틀라스의 세로 타일 개수

    // 블록 종류별로 UV 좌표 매핑
    public static Dictionary<VoxelType, BlockUV> uvs = new Dictionary<VoxelType, BlockUV>()
    {
        //순서대로 위,옆,아래,정면
        { VoxelType.GRASS, new BlockUV(new Vector2Int(2, 16), new Vector2Int(10, 16), new Vector2Int(18, 17),new Vector2Int(10, 16)) },

        // Dirt: 전체 동일 텍스처
        { VoxelType.DIRT,  new BlockUV(new Vector2Int(18, 17), new Vector2Int(18, 17), new Vector2Int(18, 17),new Vector2Int(18, 17)) },

        // Stone: 전체 동일 텍스처
        { VoxelType.STONE, new BlockUV(new Vector2Int(19, 16), new Vector2Int(19, 16), new Vector2Int(19, 16),new Vector2Int(19, 16)) },

        // Water: 전체 동일 텍스처
        { VoxelType.Water, new BlockUV(new Vector2Int(0, 29), new Vector2Int(0, 29), new Vector2Int(0, 29),new Vector2Int(0, 29)) },

        // Ore: 석탄 광석
        { VoxelType.COAL_BLOCK, new BlockUV(new Vector2Int(1, 20), new Vector2Int(1, 20), new Vector2Int(1, 20),new Vector2Int(1, 20)) },

        // Crafting Table: 위/옆/밑 각각 다른 텍스처
        { VoxelType.CRAFTTABLE, new BlockUV(new Vector2Int(12, 20), new Vector2Int(13, 20), new Vector2Int(21, 17),new Vector2Int(13, 20)) },

        //Plank: 전체동일 판자
        { VoxelType.PLANK, new BlockUV(new Vector2Int(21, 17), new Vector2Int(21, 17), new Vector2Int(21, 17),new Vector2Int(21, 17)) },

        // Furnace: 위/옆/밑 각각 다른 텍스처
        { VoxelType.FURANCE, new BlockUV(new Vector2Int(18, 20), new Vector2Int(17, 20), new Vector2Int(18, 20),new Vector2Int(15, 20)) },

        { VoxelType.WOOD, new BlockUV(new Vector2Int(4, 19), new Vector2Int(3, 19), new Vector2Int(4, 19),new Vector2Int(3, 19)) },

        { VoxelType.IRON_BLOCK, new BlockUV(new Vector2Int(0, 20), new Vector2Int(0, 20), new Vector2Int(0, 20),new Vector2Int(0, 20)) },

        { VoxelType.COBBLESTONE, new BlockUV(new Vector2Int(26, 16), new Vector2Int(26, 16), new Vector2Int(26, 16),new Vector2Int(26, 16)) },

        { VoxelType.Wool, new BlockUV(new Vector2Int(10, 26), new Vector2Int(10, 26), new Vector2Int(10, 26),new Vector2Int(10, 26)) },

        { VoxelType.Chast, new BlockUV(new Vector2Int(6, 26), new Vector2Int(7, 26), new Vector2Int(6, 26),new Vector2Int(8, 26)) },
         { VoxelType.LEAF, new BlockUV(new Vector2Int(29, 20), new Vector2Int(29, 20), new Vector2Int(29, 20),new Vector2Int(29, 20)) },
    };

    // 주어진 타일 좌표를 실제 UV 좌표 배열(사각형 4점)로 변환하는 함수
    public static Vector2[] GetUVs(Vector2Int tilePos)
    {
        float tileSizeX = 1f / atlasSizeX; // 하나의 타일이 차지하는 가로 비율
        float tileSizeY = 1f / atlasSizeY; // 하나의 타일이 차지하는 세로 비율

        float x = tilePos.x * tileSizeX;
        float y = 1f - (tilePos.y + 1) * tileSizeY; // Y축 반전 보정 (Unity UV 좌표계 차이 때문)
        //이건 gpt가 하레 그냥 uv좌표가 반대로 나와서

        // 사각형을 이루는 네 개 꼭짓점의 UV 좌표 반환 (시계/반시계 방향)
        return new Vector2[]
        {
            new Vector2(x, y),                          // 왼쪽 아래
            new Vector2(x + tileSizeX, y),              // 오른쪽 아래
            new Vector2(x + tileSizeX, y + tileSizeY),  // 오른쪽 위
            new Vector2(x, y + tileSizeY)               // 왼쪽 위
        };
    }
}
