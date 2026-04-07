using Survivors.Game;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using static Survivors.Game.FadeOutAuthoring;

namespace Survivors.Game
{
    // Handles fade-out effect over time and optionally destroys entity when complete
    partial struct FadeOutSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FadeOutData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            // ECB to safely apply structural changes (enable destroy flag)
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            var fadeOutJob = new FadeOutJob
            {
                DeltaTime = deltaTime,
                ECB = ecb.AsParallelWriter()
            };
            state.Dependency = fadeOutJob.ScheduleParallel(state.Dependency);

            /*foreach (var (fadeOutData, baseColor, fadeOutDataEnabled, entity)
                in SystemAPI.Query<RefRW<FadeOutData>,
                    RefRW<URPMaterialPropertyBaseColor>,
                    EnabledRefRW<FadeOutData>
                    >().WithEntityAccess())
            {
                fadeOutData.ValueRW.Elapsed += deltaTime;
                var alpha = 1 - math.saturate(fadeOutData.ValueRO.Elapsed / fadeOutData.ValueRO.Duration);
                baseColor.ValueRW.Value = new float4(1f, 1f, 1f, alpha);

                // When fully faded out
                if (alpha <= 0)
                {
                    // Reset timer and disable fade component
                    fadeOutData.ValueRW.Elapsed = 0;
                    fadeOutDataEnabled.ValueRW = false;

                    if (fadeOutData.ValueRO.DestroyOnComplete)
                    {
                        //mark entity for destruction
                        ecb.SetComponentEnabled<DestroyEntityFlag>(entity, true);
                    }
                }
            }*/
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }

    public partial struct FadeOutJob: IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute([ChunkIndexInQuery] int chunkIndex,
                            Entity entity,
                            RefRW<FadeOutData> fadeOutData,
                            RefRW<URPMaterialPropertyBaseColor> baseColor,
                            EnabledRefRW<FadeOutData> fadeOutDataEnabled)
        {
            fadeOutData.ValueRW.Elapsed += DeltaTime;
            var alpha = 1 - math.saturate(fadeOutData.ValueRO.Elapsed / fadeOutData.ValueRO.Duration);
            baseColor.ValueRW.Value = new float4(1f, 1f, 1f, alpha);

            // When fully faded out
            if (alpha <= 0)
            {
                // Reset timer and disable fade component
                fadeOutData.ValueRW.Elapsed = 0;
                fadeOutDataEnabled.ValueRW = false;

                if (fadeOutData.ValueRO.DestroyOnComplete)
                {
                    //mark entity for destruction
                    ECB.SetComponentEnabled<DestroyEntityFlag>(chunkIndex, entity, true);
                }
            }
        }
    }
}
