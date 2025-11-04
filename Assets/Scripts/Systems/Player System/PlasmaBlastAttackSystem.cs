using System.ComponentModel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Survivors.Game
{
    /// <summary>
    /// This system handles the logic for detecting collisions between PlasmaBlast entities and Enemy entities.
    /// It schedules a job to process collision events and apply damage to the enemies.
    /// </summary>

    [UpdateInGroup(typeof(PhysicsSystemGroup))]// ensures we run in the physics pipeline
    [UpdateAfter(typeof(PhysicsSimulationGroup))]// run *after* physics has advanced and generated collision events
    [UpdateBefore(typeof(AfterPhysicsSystemGroup))]//before post-physics cleanup/logic runs
    partial struct PlasmaBlastAttackSystem : ISystem
    {
        private NativeList<Entity> logEntities;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            logEntities = new NativeList<Entity>(Allocator.Persistent);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!logEntities.IsCreated) return;

            logEntities.Clear();

            var attackJob = new PlasmaBlastAttackJob
            {
                PlasmaBlastLookUp = SystemAPI.GetComponentLookup<PlasmaBlastData>(true), // Lookup for PlasmaBlast data
                EnemyLookUp = SystemAPI.GetComponentLookup<EnemyTag>(true),// Lookup for enemy tags
                DammageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>(), // Access dynamic buffer for handling damage
                DestroyEntityLookUp = SystemAPI.GetComponentLookup<DestroyEntityFlag>(), // Lookup for entities flagged for destruction
                LogEntities = logEntities,
            };

            // Retrieve the SimulationSingleton for scheduling the job
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            state.Dependency = attackJob.Schedule(simulationSingleton, state.Dependency);
            // For debugging: force complete immediately so we can log results this frame
            // (in production you usually don’t Complete here, to keep jobs async)
            state.Dependency.Complete();

            foreach (var entity in logEntities)
            {
                UnityEngine.Debug.Log($"PlasmaBlastAttackSystem Collision detected with Entity: {entity.Index}");
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (logEntities.IsCreated)
                logEntities.Dispose();
        }
    }


    [BurstCompile]
    public struct PlasmaBlastAttackJob : ITriggerEventsJob
    {
        [Unity.Collections.ReadOnly]
        public ComponentLookup<PlasmaBlastData> PlasmaBlastLookUp;
        [Unity.Collections.ReadOnly]
        public ComponentLookup<EnemyTag> EnemyLookUp;
        public ComponentLookup<DestroyEntityFlag> DestroyEntityLookUp;

        // Dynamic buffer for storing damage events to be applied to enemies
        public BufferLookup<DamageThisFrame> DammageBufferLookup;


        public NativeList<Entity> LogEntities;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity plasmaBlastEntity;
            Entity enemyEntity;

            LogEntities.Add(triggerEvent.EntityA);
            LogEntities.Add(triggerEvent.EntityB);

            // Determine which entity is the PlasmaBlast and which is the Enemy
            if (PlasmaBlastLookUp.HasComponent(triggerEvent.EntityA) && EnemyLookUp.HasComponent(triggerEvent.EntityB))
            {
                plasmaBlastEntity = triggerEvent.EntityA;
                enemyEntity = triggerEvent.EntityB;
            }
            else if (PlasmaBlastLookUp.HasComponent(triggerEvent.EntityB) && EnemyLookUp.HasComponent(triggerEvent.EntityA))
            {
                plasmaBlastEntity = triggerEvent.EntityB;
                enemyEntity = triggerEvent.EntityA;
            }
            else
            {
                return;
            }

            var attackDamage = PlasmaBlastLookUp[plasmaBlastEntity].AttackDamage;

            // Get the dynamic buffer belonging to this particular Enemy entity and add the damage
            var enemyDynamicDamageBuffer = DammageBufferLookup[enemyEntity];
            enemyDynamicDamageBuffer.Add(new DamageThisFrame { Value = attackDamage });

            // Mark the PlasmaBlast entity for destruction
           // DestroyEntityLookUp.SetComponentEnabled(plasmaBlastEntity, true);
        }
    }
}
