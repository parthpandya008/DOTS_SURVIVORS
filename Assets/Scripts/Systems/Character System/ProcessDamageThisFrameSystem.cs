using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Survivors.Game
{
    //Apply all "damage " collected the player (by all collided enemies) this frame,
    //then clear the buffer so it doesn't persist into the next frame.
    partial struct ProcessDamageThisFrameSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            /* REASON:
             * Split into two jobs (fade vs no-fade) instead of one job using a ComponentLookup,
            // because writing to an IEnableableComponent (FadeOutData / DestroyEntityFlag) via
            // ComponentLookup requires write access, which is not ideal in a parallel job.
            // [WithPresent]/[WithNone] on FadeOutData lets each job write its target component
            // directly instead, which IS safe in ScheduleParallel.*/

            // Entities that have FadeOutData 
            var fadeJob = new ProcessDamageWithFadeJob();
            var fadeJobHandle  = fadeJob.ScheduleParallel(state.Dependency);

            // Entities without FadeOutData — destroy immediately as before
            var destroyJob = new ProcessDamageWithoutFadeJob();
            // Depend on fadeJobHandle instead of state.Dependency directly —
            // this tells Unity "wait for the fade job's writes to CharacterCurrentHitPoints
            // to finish before this job's writes to the same type begin"
            state.Dependency = destroyJob.ScheduleParallel(fadeJobHandle); 
        }

        [WithPresent(typeof(FadeOutData))]
        [BurstCompile]
        public partial struct ProcessDamageWithFadeJob : IJobEntity
        {
            public void Execute(RefRW<CharacterCurrentHitPoints> characterCurrentHitPoints,
                                DynamicBuffer<DamageThisFrame> damageThisFrame,
                                EnabledRefRW<FadeOutData> fadeOutEnabled)
            {
                if(DamageHelper.ApplyDamage(ref characterCurrentHitPoints, damageThisFrame))
                {
                    fadeOutEnabled.ValueRW = true;
                }
            }
        }


        [WithNone(typeof(FadeOutData))]
        [WithPresent(typeof(DestroyEntityFlag))]
        [BurstCompile]
        public partial struct ProcessDamageWithoutFadeJob: IJobEntity
        {
            public void Execute(RefRW<CharacterCurrentHitPoints> characterCurrentHitPoints,
                                DynamicBuffer<DamageThisFrame> damageThisFrame,
                                EnabledRefRW <DestroyEntityFlag> destroyFlagEnabled)
            {
                if(DamageHelper.ApplyDamage(ref characterCurrentHitPoints, damageThisFrame))
                {
                    destroyFlagEnabled.ValueRW = true;
                }
            }
        }
    }
}
