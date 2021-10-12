using System;
using UnityEngine;

namespace EvoAgents.World.Utils {
    [Serializable]
    public struct BlockType {
        [Range(.0f, .5f)] public float height;
        public Material material;
    }

    [Flags]
    public enum Direction {
        Nothing = 0b000000,
        All     = 0b111111,
        North   = 0b000001,
        Up      = 0b000010,
        South   = 0b000100,
        Down    = 0b001000,
        West    = 0b010000,
        East    = 0b100000
    }

    [Flags]
    public enum WorldArea {
        Nothing = 0b0000,
        All     = 0b1111,
        Water   = 0b0001,
        Grass   = 0b0010, 
        Sand    = 0b0100
    }

    [Serializable]
    public struct VoxelData {
        public string name;
        public WorldArea area;
        public BlockType biome;

        [HideInInspector] public Matrix4x4 localToWorldMatrix;
        [HideInInspector] public Mesh mesh;

        public VoxelData(string name = "") {
            this.name = name;
            area = WorldArea.Nothing;
            biome = new BlockType();
            localToWorldMatrix = Matrix4x4.identity;
            mesh = new Mesh();
        }
    }
}
