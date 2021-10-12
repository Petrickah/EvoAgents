using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Cysharp.Threading.Tasks;

namespace EvoAgents.World
{
    public static class PointSampling {
        [BurstCompile]
        struct SamplingPoints : IJobParallelFor {
            public int kPoints;
            public float2 x0;
            public float radius;
            public Random random;

            [NativeDisableParallelForRestriction]
            public NativeArray<float2> activeList;

            public void Execute(int index) {
                float2 x = RandomPointAround(x0);
                float2 lastPoint = x;
                int pIndex;
                bool rejected = false;
                for (pIndex = 0; pIndex < kPoints; pIndex++) {
                    var xi = RandomPointAround(x);
                    rejected = math.distance(xi, x) < radius;
                    if (!rejected) lastPoint = xi;
                    else break;
                }
                if (rejected) activeList[index] = lastPoint;
                else activeList[index] = x;
            }
            float2 RandomPointAround(float2 xi) => xi + 2f * radius * (2f * random.NextFloat2() - 1f);
        }
        public static async UniTask<NativeArray<float2>> GeneratePoints(uint seed, float radius, int density, int kPoints) {
            Random random = (seed != 0) ? new Random(seed): new Random((uint)System.DateTime.Now.Millisecond);
            var activeList = new NativeArray<float2>(density, Allocator.TempJob);

            var samplingPoints = new SamplingPoints {
                x0 = float2.zero,
                radius = radius,
                random = random,
                activeList = activeList,
                kPoints = kPoints,
            };

            await samplingPoints.Schedule(density, 1);
                
            return activeList;
        }
    }
}
