using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Game
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct FlashFadeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var flashFadeJob = new FlashFadeJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            };

            state.Dependency = flashFadeJob.ScheduleParallel(state.Dependency);
        }
    }

    public partial struct FlashFadeJob: IJobEntity
    {
        public float DeltaTime;

        // Modifies the flash amount while reading the specific entity's speed 
        public void Execute(ref FlashAmount flashAmount, 
                            in FlashSpeedData flashSpeedData)
        {
            // Only process the math if the entity is currently visibly flashing
            if (flashAmount.Value > 0)
            {
                flashAmount.Value -= DeltaTime * flashSpeedData.Value;
                if(flashAmount.Value < 0)
                {
                    flashAmount.Value = 0;
                }
            }
        }
    }
}

