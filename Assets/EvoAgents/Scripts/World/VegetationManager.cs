using System.Collections.Generic;
using UnityEngine;
using EvoAgents.World.Utils;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace EvoAgents.World
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [AddComponentMenu("EvoAgents/World/Vegetation Manager")]
    public class VegetationManager : MonoBehaviour {
        [Header("Configuration")]
        [Range(1f, 50f)] public int Radius = 25;
        public int Density = 1000;
        [Range(30, 40)] public int PointsBeforeRejection = 30;

        [Header("Area Settings")]
        public GameObject parentTransform;
        public WorldArea spawnableAreas;
        public List<GameObject> VegetationPrefabs;

        public async UniTask Generate(WorldChunk world, uint seed = 0) {
            List<VoxelData> voxels = world.VoxelTypes;
            if (seed != 0) UnityEngine.Random.InitState((int)seed);

            Destroy(parentTransform);
            var points = await PointSampling.GeneratePoints(seed, Radius, Density, PointsBeforeRejection);
            float2[] finalPoints = new float2[Density];
            points.CopyTo(finalPoints);
            points.Dispose();

            parentTransform = new GameObject("Vegetation");
            parentTransform.transform.parent = transform;

            
            foreach (var point in finalPoints) {
                var grassPoint = CheckPoint(new Vector2(point.x, point.y), spawnableAreas, voxels);
                if (!float.IsInfinity(grassPoint.sqrMagnitude)) {
                    Instantiate(VegetationPrefabs[UnityEngine.Random.Range(0, VegetationPrefabs.Count)], grassPoint, Quaternion.identity, parentTransform.transform);
                }
            }
        }

        Vector3 CheckPoint(Vector2 point, WorldArea spawnableAreas, List<VoxelData> voxels) {
            if (Physics.Raycast(new Vector3(point.x, 9f, point.y), Vector2.down, out RaycastHit hitInfo, 10f)) {
                if (hitInfo.collider != null) {
                    if (FindValidVoxel(hitInfo.collider.name, spawnableAreas, voxels) != -1)
                        return hitInfo.point;
                }
            }
            return Vector3.positiveInfinity;
        }

        int FindValidVoxel(string name, WorldArea spawnableAreas, List<VoxelData> voxels) {
            return voxels.FindIndex((target) => {
                return target.name == name && (target.area & spawnableAreas) != 0;
            });
        }
    }
}
