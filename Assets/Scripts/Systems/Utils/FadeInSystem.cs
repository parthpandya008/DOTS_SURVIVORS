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

            foreach (var (fadeInData, baseColor, fadeInDataEnable) in
                SystemAPI.Query<RefRW<FadeInData>,
                RefRW<URPMaterialPropertyBaseColor>,
                EnabledRefRW<FadeInData>>())
            {
                fadeInData.ValueRW.Elapsed += deltaTime;

                var alpha = math.saturate(fadeInData.ValueRO.Elapsed / fadeInData.ValueRO.Duration);
                baseColor.ValueRW.Value = new float4(1, 1, 1, alpha);
                
                // When fully visible, Disable fade-in and reset timer
                if (alpha >= 1)
                {
                    fadeInDataEnable.ValueRW = false;
                    fadeInData.ValueRW.Elapsed = 0;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
