using UnityEngine;

public static class ParticleTileSetter
{
    public static int ParticleSetNum(VoxelType type)
    {
        if (type == VoxelType.GRASS || type == VoxelType.DIRT) return 50;
        if (type == VoxelType.STONE) return 19;
        if (type == VoxelType.COBBLESTONE) return 26;
        if (type == VoxelType.FURANCE) return 145;
        if (type == VoxelType.CRAFTTABLE) return 141;
        if (type == VoxelType.PLANK) return 53;
        if (type == VoxelType.WOOD) return 99;
        if (type == VoxelType.COAL_BLOCK) return 129;
        if (type == VoxelType.IRON_BLOCK) return 128;
        if (type == VoxelType.LEAF) return 157;
        
        return 0;
    }
}
