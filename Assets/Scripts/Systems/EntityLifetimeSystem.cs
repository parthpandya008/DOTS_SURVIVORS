using Survivors.Game;
using Unity.Burst;
using Unity.Entities;

/// <summary>
/// System that checks EntityLifetime components and marks expired entities for destruction.
/// Runs in SimulationSystemGroup to check lifetimes each frame before destruction system runs.
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(DestroyEntitysystem))]// Must run before the destruction system
partial struct EntityLifetimeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntityLifetime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var currentTime = SystemAPI.Time.ElapsedTime;
        var destroyLookUp = SystemAPI.GetComponentLookup<DestroyEntityFlag>();
        
        foreach(var (lifeTime, entity) in 
            SystemAPI.Query<RefRO<EntityLifetime>>()
            .WithDisabled<DestroyEntityFlag>()
            .WithEntityAccess())
        {
            // Check if the entity's lifetime has expired
            if (currentTime >= lifeTime.ValueRO.DestroyAtTime)
            {
                // Mark the entity for destruction by enabling the DestroyEntityFlag
                // This will be picked up by DestroyEntitysystem
                destroyLookUp.SetComponentEnabled(entity, true);
            }
        }

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
