using UnityEngine;
using Unity.Mathematics;

namespace EvoAgents.World {
    [System.Serializable]
    [CreateAssetMenu(fileName = "World Settings", menuName = "EvoAgents/World Settings")]
    public class WorldSettings : ScriptableObject {
        public const int NrOctaves = 8;
        [SerializeField] private World world;

        public World World => world;
    }

    [System.Serializable]
    public struct World {
        public uint seed;
        public float persistance;
        public float lacunarity;
        public float scale;
        public float2 offset;
    }
}
