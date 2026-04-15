using System.ComponentModel;
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
            };
            state.Dependency = fadeOutJob.ScheduleParallel(state.Dependency);            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }

    [WithAll(typeof(FadeOutData))]
    /*TODO: 1. Currently this job won't support the fade out for the entity which doens't has DestroyEntityFlag
            2. Need to create and saperate job for the [WithNone(typeof(DestroyEntityFlag))]
            3. To avoid duplicating the alpha logic, extract it into a static helper*/
    [WithPresent(typeof(DestroyEntityFlag))]
    public partial struct FadeOutJob: IJobEntity
    {
        public float DeltaTime;        

        public void Execute(Entity entity,
                            RefRW<FadeOutData> fadeOutData,
                            RefRW<URPMaterialPropertyBaseColor> baseColor,
                            EnabledRefRW<FadeOutData> fadeOutDataEnabled,
                            EnabledRefRW<DestroyEntityFlag> destroyFlagEnabled)
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

                if(fadeOutData.ValueRO.DestroyOnComplete)
                {
                    destroyFlagEnabled.ValueRW = true;
                }
            }
        }
    }
}
