using Unity.Burst;
using Unity.Entities;

//Apply all "damage " collected the player (by all collided enemies) this frame,
//then clear the buffer so it doesn't persist into the next frame.
partial struct ProcessDamageThisFrameSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var (characterCurrentHitPoints,damageThisFrame) in 
            SystemAPI.Query<RefRW<CharacterCurrentHitPoints>, DynamicBuffer<DamageThisFrame>>())
        {
            // Skip entities that received no damage this frame
            if (damageThisFrame.IsEmpty == true) continue;

            // Apply each damage event stored in the buffer
            foreach (var damage in damageThisFrame)
            {
                characterCurrentHitPoints.ValueRW.Value -= damage.Value;
            }

            damageThisFrame.Clear();
        }
    }
}
