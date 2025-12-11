using System.ComponentModel;
using Survivors.Game;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

/// <summary>
/// System responsible for handling the collection of gems by players.
/// This system listens for trigger events between Player and Gems and processes  collection logic.
/// </summary>
partial struct CollectGemSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Create and configure the GemCollectJob with necessary component lookups.
        var gemCollectJob = new GemCollectJob
        {
            GemLookup = SystemAPI.GetComponentLookup<GemTag>(true),
            GemsCollectedCountLookUp = SystemAPI.GetComponentLookup<GemsCollectedCount>(),
            DestroyEntityLookUp = SystemAPI.GetComponentLookup<DestroyEntityFlag>(),
            UpdateGemUIFlagLookUp = SystemAPI.GetComponentLookup<UpdateGemUIFlag>()
        };

        // Get the SimulationSingleton instance to access the physics world.
        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        // Schedule the GemCollectJob to process trigger events in the physics simulation.
        state.Dependency = gemCollectJob.Schedule(simulationSingleton, state.Dependency);
    }
}

[BurstCompile]
public struct GemCollectJob : ITriggerEventsJob
{
    [Unity.Collections.ReadOnly]
    public ComponentLookup<GemTag> GemLookup;
    public ComponentLookup<DestroyEntityFlag> DestroyEntityLookUp;
    public ComponentLookup<GemsCollectedCount> GemsCollectedCountLookUp;
    public ComponentLookup<UpdateGemUIFlag> UpdateGemUIFlagLookUp;

    /// <summary>
    /// When a player collides with a gem, the gem is collected, the player's gem count is updated,
    /// the gem is marked for destruction, and the UI update flag is set.
    /// </summary>
    /// <param name="triggerEvent">The trigger event containing the entities involved.</param>
    public void Execute(TriggerEvent triggerEvent)
    {
        Entity gemEntity;
        Entity playerEntity;

        // Determine which entity is the gem and which is the player
        if (GemLookup.HasComponent(triggerEvent.EntityA) && GemsCollectedCountLookUp.HasComponent(triggerEvent.EntityB))
        {
            gemEntity = triggerEvent.EntityA;
            playerEntity = triggerEvent.EntityB;
        }
        else if (GemLookup.HasComponent(triggerEvent.EntityB) && GemsCollectedCountLookUp.HasComponent(triggerEvent.EntityA))
        {
            gemEntity = triggerEvent.EntityB;
            playerEntity = triggerEvent.EntityA;
        }
        else
        {
            return;
        }

        // Increment the player's gem collection count.
        var gemsCollected = GemsCollectedCountLookUp[playerEntity];
        gemsCollected.Value += 1;
        GemsCollectedCountLookUp[playerEntity] = gemsCollected;

        // Enable the flag to update the gem UI for the player.
        UpdateGemUIFlagLookUp.SetComponentEnabled(playerEntity, true);

        // Mark the gem entity for destruction.
        DestroyEntityLookUp.SetComponentEnabled(gemEntity, true); 
    }
}
