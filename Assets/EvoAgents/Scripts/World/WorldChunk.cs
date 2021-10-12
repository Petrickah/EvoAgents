using System.Collections.Generic;
using UnityEngine;
using EvoAgents.World.Utils;
using UnityEngine.AI;

using Cysharp.Threading.Tasks;

namespace EvoAgents.World
{
    [AddComponentMenu("EvoAgents/World/World Chunk")]
    public class WorldChunk: MonoBehaviour {
        [Header("Terrain Settings")]
        public int ChunkSize = 0;
        public Mesh BaseMesh;

        public List<VoxelData> VoxelTypes;
        private List<CombineInstance>[] combines;

        public void StartGeneration(float[,] heightMap, Transform transform) {
            combines = new List<CombineInstance>[VoxelTypes.Count];
            for (int voxelIndex = 0; voxelIndex < VoxelTypes.Count; voxelIndex++)
                combines[voxelIndex] = new List<CombineInstance>();

            Generate(heightMap, transform);
        }

        private void Generate(float[,] heightMap, Transform parent) {
            for (float yk = 0; yk <= ChunkSize; yk++) {
                for (float xk = 0; xk <= ChunkSize; xk++) {
                    var voxel = GenerateVoxel(xk, yk, heightMap);
                    int biomeIndex = VoxelTypes.FindIndex((voxelData) => voxel.name == voxelData.name);
                    combines[biomeIndex].Add(new CombineInstance {
                        mesh = voxel.mesh,
                        transform = voxel.localToWorldMatrix,
                    });
                }
            }

            List<VoxelData> voxels = new List<VoxelData>();
            for (int voxelIndex = 0; voxelIndex < VoxelTypes.Count; voxelIndex++)
            {
                VoxelData voxel = VoxelTypes[voxelIndex];
                voxel.mesh = new Mesh();
                voxel.mesh.CombineMeshes(combines[voxelIndex].ToArray(), true, true);
                if (voxel.mesh.triangles.Length > 0)
                {
                    var voxelObject = new GameObject(voxel.name, typeof(MeshRenderer), typeof(MeshFilter));
                    voxelObject.transform.position = new Vector3(-1f, 0, -1f) * ChunkSize;
                    voxelObject.transform.parent = parent;
                    MeshRenderer meshRenderer = voxelObject.GetComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = voxel.biome.material;
                    voxelObject.GetComponent<MeshFilter>().sharedMesh = voxel.mesh;
                    if (voxel.name != "Water")
                    {
                        var modifier = voxelObject.AddComponent<NavMeshModifier>();
                        modifier.overrideArea = true;
                        modifier.area = NavMesh.GetAreaFromName(voxel.area.ToString());
                        voxelObject.layer = LayerMask.NameToLayer("Terrain");
                        voxelObject.AddComponent<MeshCollider>().sharedMesh = voxel.mesh;
                        voxelObject.transform.localScale -= Vector3.up * 0.33f;
                    }
                    else voxelObject.layer = LayerMask.NameToLayer("Water");
                    voxels.Add(voxel);
                }
            }
        }

        VoxelData GenerateVoxel(float x, float y, float[,] heightMap) {
            Vector3 center   = transform.position + new Vector3(ChunkSize / 2f, 0, ChunkSize / 2f);
            Vector3 voxelPos = center + new Vector3(x, 0f, y);
            Vector3 mapped   = voxelPos - center;

            float height = heightMap[(int)mapped.x, (int)mapped.z];
            VoxelData voxel = BiomeInfo(height, VoxelTypes.ToArray());

            Matrix4x4 localToWorldMatrix = Matrix4x4.TRS(voxelPos, Quaternion.identity, Vector3.one);
            if (voxel.name == "Water")
                localToWorldMatrix = Matrix4x4.TRS(voxelPos - 0.3f * Vector3.up, Quaternion.identity, Vector3.one);

            Direction dirMask = Direction.Up;
            if (voxel.name != "Water")
            {
                dirMask |= Direction.Down;
                for (int j = -1; j <= 1; j ++) {
                    if ((-1 < mapped.x - 1 && mapped.x + 1 < heightMap.GetLength(0)))
                    {
                        float neighbourX = heightMap[(int)mapped.x + j, (int)mapped.z + 0];
                        VoxelData blockTypeX = BiomeInfo(neighbourX, VoxelTypes.ToArray());
                        if (blockTypeX.name == "Water")
                        {
                            if (j == -1) dirMask |= Direction.West;
                            if (j == 1)  dirMask |= Direction.East;
                        }
                    }
                    if ((-1 < mapped.z - 1 && mapped.z + 1 < heightMap.GetLength(1)))
                    {
                        float neighbourZ = heightMap[(int)mapped.x + 0, (int)mapped.z + j];
                        VoxelData blockTypeZ = BiomeInfo(neighbourZ, VoxelTypes.ToArray());
                        if (blockTypeZ.name == "Water")
                        {
                            if (j == -1) dirMask |= Direction.South;
                            if (j == 1)  dirMask |= Direction.North;
                        }
                    }
                }
            }

            return new VoxelData {
                name = voxel.name,
                mesh = GenerateMesh(BaseMesh, (int)dirMask),
                biome = voxel.biome,
                localToWorldMatrix = localToWorldMatrix
            };
        }

        Mesh GenerateMesh(Mesh startingMesh, int mask) {

            List<int> newTriangles = new List<int>();

            for (int faceIndex = 0; faceIndex<6; faceIndex ++) {
                Direction direction = (Direction)(1<<faceIndex);
                if ((mask & (int)direction) != 0) {
                    newTriangles.Add(startingMesh.triangles[faceIndex * 6 + 0]);
                    newTriangles.Add(startingMesh.triangles[faceIndex * 6 + 1]);
                    newTriangles.Add(startingMesh.triangles[faceIndex * 6 + 2]);
                    newTriangles.Add(startingMesh.triangles[faceIndex * 6 + 3]);
                    newTriangles.Add(startingMesh.triangles[faceIndex * 6 + 4]);
                    newTriangles.Add(startingMesh.triangles[faceIndex * 6 + 5]);
                }
            }

            Mesh copyMesh = new Mesh {
                vertices = startingMesh.vertices,
                uv = startingMesh.uv,
                triangles = newTriangles.ToArray()
            };
            copyMesh.Optimize();
            copyMesh.RecalculateNormals();
            copyMesh.RecalculateBounds();
            return copyMesh;
        }

        VoxelData BiomeInfo(float height, VoxelData[] biomes) {
            int biomeIndex = 0;
            for (int i = 0; i < biomes.Length; i++) {
                if (height <= biomes[i].biome.height) {
                    biomeIndex = i;
                    break;
                }
            }

            return biomes[biomeIndex];
        }
    }
}
