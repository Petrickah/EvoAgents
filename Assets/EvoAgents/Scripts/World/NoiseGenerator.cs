using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Collections.Generic;

namespace EvoAgents.World.Utils {
    public static class NoiseGenerator {
        static Random random;
        static void InitRandomState(uint seed) {
            if (seed != 0)
                random = new Random(seed);
            else random = new Random((uint)System.DateTime.Now.Millisecond);
        }

        public static float[, ] Heightmap(World noiseSettings, int size, uint seed = 0) {
            var map = new float[size, size];
            InitRandomState(seed);

            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            var octaves = new NativeArray<float2>(WorldSettings.NrOctaves, Allocator.TempJob);
            var offsets = GenerateOctaves(ref octaves);
            octaves.Dispose();

            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    var sample = new SampleParams
                    {
                        frequency = noiseSettings.scale,
                        amplitude = 1f,
                        sample = new float2(x, y),
                        size = size,
                        world = noiseSettings
                    };

                    map[x, y] = CalculateHeight(sample, offsets);
                    minHeight = math.min(minHeight, map[x, y]);
                    maxHeight = math.max(maxHeight, map[x, y]);
                }
            }

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    map[x, y] = math.unlerp(minHeight, maxHeight, map[x, y]);
            return map;
        }

        static float2[] GenerateOctaves(ref NativeArray<float2> octaves) {
            CalculateOctavesJob octavesJob = new CalculateOctavesJob {
                one = new float2(1, 1),
                random = random,
                octaves = octaves
            };
            JobHandle jobHandle = octavesJob.Schedule(WorldSettings.NrOctaves, 1);
            jobHandle.Complete();

            float2[] offsets = new float2[WorldSettings.NrOctaves];
            octaves.CopyTo(offsets);
            return offsets;
        }

        static float CalculateHeight(SampleParams sample, float2[] octaves) {
            NativeArray<float> height = new NativeArray<float>(new float[] { 0 }, Allocator.TempJob);

            Stack<JobHandle> handles = new Stack<JobHandle>();
            for (int octaveIndex = 0; octaveIndex < WorldSettings.NrOctaves; octaveIndex++) {
                var calcHeightJob = new CalculateHeightJob {
                    offsetValue = octaves[octaveIndex],
                    height = height,
                    samples = sample
                };
                JobHandle handle;
                if (octaveIndex == 0)
                    handle = calcHeightJob.Schedule();
                else handle = calcHeightJob.Schedule(handles.Peek());
                handles.Push(handle);
            }
            var jobHandle = handles.Peek();
            jobHandle.Complete();

            float value = height[0];
            height.Dispose();
            return value;
        }
    }

    [BurstCompile]
    struct CalculateOctavesJob : IJobParallelFor
    {
        public float2 one;
        public Random random;
        public NativeArray<float2> octaves;
        public void Execute(int index) {
            octaves[index] = 1000 * (2 * random.NextFloat2Direction() - one);
        }
    }

    [BurstCompile]
    struct CalculateHeightJob : IJob
    {
        public float2 offsetValue;
        public SampleParams samples;
        public NativeArray<float> height;

        public void Execute()
        {
            var freq = samples.frequency * samples.world.lacunarity;
            var amp = samples.amplitude * samples.world.persistance;
            var sample = (samples.sample / samples.size) * samples.frequency + (offsetValue + samples.world.offset);
            samples = new SampleParams { sample = sample, amplitude = amp, frequency = freq };
            height[0] += noise.cnoise(sample) * amp;
        }
    }

    struct SampleParams {
        public float size;
        public float frequency;
        public float amplitude;
        public float2 sample;
        public World world;
    }
}
