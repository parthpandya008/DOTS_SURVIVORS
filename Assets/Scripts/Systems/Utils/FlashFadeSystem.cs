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

    [BurstCompile]
    public partial struct FlashFadeJob: IJobEntity
    {
        public float DeltaTime;

        // Modifies the flash amount while reading the specific entity's speed 
        public void Execute(ref FlashAmount flashAmount, 
                            ref FlashSpeedData flashSpeedData,//Due to DOTS Job Safety System, this can't be Read-Only as we have EnabledRefRW<FlashSpeedData>, and same component can't have read and write access together
                            EnabledRefRW<FlashSpeedData> enabledFlashSpeedData) // EnabledRefRW allows direct, thread-safe access to the enable bitset!
        {
            // Only process the math if the entity is currently visibly flashing
            if (flashAmount.Value > 0)
            {
                flashAmount.Value -= DeltaTime * flashSpeedData.Value;
                if(flashAmount.Value <= 0)
                {
                    flashAmount.Value = 0;

                    // Turn this component off! The job will completely ignore this entity next frame.
                    enabledFlashSpeedData.ValueRW = false;
                }
            }
        }
    }
}

