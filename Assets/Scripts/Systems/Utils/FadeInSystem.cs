using Survivors.Game;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Survivors.Game
{
    // Handles fade-in effect over time by gradually increasing alpha
    partial struct FadeInSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FadeInData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            var fadeInJob = new FadeInJob
            {
                DeltaTime = deltaTime
            };
            state.Dependency = fadeInJob.ScheduleParallel(state.Dependency);            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }

    [WithAll(typeof(FadeInData))]
    [BurstCompile]
    public partial struct FadeInJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(RefRW<FadeInData> fadeInData,
                            RefRW<URPMaterialPropertyBaseColor> baseColor,
                            EnabledRefRW<FadeInData> fadeInDataEnable)
        {
            fadeInData.ValueRW.Elapsed += DeltaTime;

            var alpha = math.saturate(fadeInData.ValueRO.Elapsed / fadeInData.ValueRO.Duration);

            //baseColor.ValueRW.Value = new float4(1, 1, 1, alpha);
            var color = baseColor.ValueRW.Value;
            color.w = alpha; // only modify alpha
            baseColor.ValueRW.Value = color;

            // When fully visible, Disable fade-in and reset timer
            if (alpha >= 1)
            {
                fadeInDataEnable.ValueRW = false;
                fadeInData.ValueRW.Elapsed = 0;
            }
        }
    }
}
