using UnityEngine;
using UnityEngine.AI;
using EvoAgents.World.Utils;
using EvoAgents.Agents;
using Cysharp.Threading.Tasks;
using EvoAgents.Player;

namespace EvoAgents.World {
    [AddComponentMenu("EvoAgents/World/World Generator")]
    [RequireComponent(typeof(WorldChunk))]
    [RequireComponent(typeof(TreeGenerator))]
    [RequireComponent(typeof(VegetationManager))]
    [RequireComponent(typeof(NavMeshSurface))]
    public class WorldGenerator : MonoBehaviour {
        [Header("World Settings")]
        public string stringSeed = "";
        public bool AutoGeneration = true;
        public GameObject worldObject;
        public WorldSettings world;

        [Header("Helper Objects")]
        public HealthSystem player;
        public AgentManager agentManager;
        [SerializeField] NavMeshSurface navMeshSurface;
        [SerializeField] WorldChunk worldChunk;
        [SerializeField] TreeGenerator treeGenerator;
        [SerializeField] VegetationManager vegetationManager;

        public void ExitGame() => Application.Quit();
        uint StringToNumberSeed(string stringSeed) {
            int seed = 0;
            if (stringSeed.Length < 8) return 0;
            for (int charIndex = 3; charIndex < stringSeed.Length; charIndex++)
                seed += stringSeed[charIndex - 3] | (stringSeed[charIndex - 2] << 8) | (stringSeed[charIndex - 1] << 16) | (stringSeed[charIndex] << 32);
            return (uint)Mathf.RoundToInt(seed / (stringSeed.Length/4f));
        }

        private void Awake() {
            worldChunk = worldChunk != null ? worldChunk : GetComponent<WorldChunk>();
            treeGenerator = treeGenerator != null ? treeGenerator : GetComponent<TreeGenerator>();
            vegetationManager = vegetationManager != null ? vegetationManager : GetComponent<VegetationManager>();
            navMeshSurface = navMeshSurface != null ? navMeshSurface : GetComponent<NavMeshSurface>();
        }

        void Start() {
            if (AutoGeneration) {
                player.OnPlayerDead += Generate;
                Generate();
            }
        }

        public async void Generate() {
            var seed = (stringSeed != "") ? StringToNumberSeed(stringSeed) : 0;
            Destroy(worldObject);
            worldObject = new GameObject("World Object") { isStatic = true };
            worldObject.transform.parent = transform;

            float[,] heightMap = NoiseGenerator.Heightmap(world.World, worldChunk.ChunkSize + 1, seed);
            worldChunk.StartGeneration(heightMap, worldObject.transform);
            await treeGenerator.Generate(worldChunk, seed);
            navMeshSurface.BuildNavMesh();
            agentManager.SpawnBatch(seed);
            await vegetationManager.Generate(worldChunk, seed);
        }
    }
}
