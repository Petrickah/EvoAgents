using System.Collections.Generic;
using UnityEngine;
using EvoAgents.World.Utils;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Cysharp.Threading.Tasks;

namespace EvoAgents.World {
    [AddComponentMenu("EvoAgents/World/Tree Generator")]
    public class TreeGenerator : MonoBehaviour {
        [Header("Configuration")]
        [Range(1f, 50f)]  public float Radius = 3f;
        public int Density = 300;
        [Range(30f, 50f)]  public int PointsBeforeRejection = 30;

        [Header("Area Settings")]
        public GameObject parentTransform;
        public WorldArea spawnableAreas;
        public List<GameObject> TreePrefabs;

        public async UniTask Generate(WorldChunk world, uint seed = 0) {
            List<VoxelData> voxels = world.VoxelTypes;
            if (seed != 0) UnityEngine.Random.InitState((int)seed);

            Destroy(parentTransform);
            var points = await PointSampling.GeneratePoints(seed, Radius, Density, PointsBeforeRejection);
            float2[] finalPoints = new float2[Density];
            points.CopyTo(finalPoints);
            points.Dispose();

            parentTransform = new GameObject("Trees") { layer = LayerMask.NameToLayer("Obstacles") };
            parentTransform.transform.parent = transform;

            foreach (var point in finalPoints) {
                Vector3 treePoint = CheckPoint(new Vector2(point.x, point.y), spawnableAreas, voxels);
                if (!float.IsInfinity(treePoint.sqrMagnitude)) {
                    var tree = Instantiate(TreePrefabs[UnityEngine.Random.Range(0, TreePrefabs.Count)], treePoint, Quaternion.identity, parentTransform.transform);
                    tree.layer = LayerMask.NameToLayer("Obstacles");
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
