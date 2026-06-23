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
                DestroyEntityLookUp = SystemAPI.GetComponentLookup<DestroyEntityFlag>(), // Lookup for entities flagged for destruction
                
                FlashAmountLookUp = SystemAPI.GetComponentLookup<FlashAmount>(), // Lookup for FlashAmount 
                FlashSpeedLookUp = SystemAPI.GetComponentLookup<FlashSpeedData>(), // Lookup for FlashSpeedData 

                DammageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>(), // Access dynamic buffer for handling damage

                HitEnemyLookup = SystemAPI.GetBufferLookup<HitEnemy>(), // Access dynamic buffer for tracking hit enemies

                LogEntities = logEntities,
            };

            // Retrieve the SimulationSingleton for scheduling the job
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            state.Dependency = attackJob.Schedule(simulationSingleton, state.Dependency);
           
            /* Uncomment the following to check the logs
            // For debugging: force complete immediately so we can log results this frame
            // (in production you usually don’t Complete here, to keep jobs async)
            state.Dependency.Complete();

            foreach (var entity in logEntities)
            {
                UnityEngine.Debug.Log($"PlasmaBlastAttackSystem Collision detected with Entity: {entity.Index}");
            }
            */
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
        
        public ComponentLookup<FlashAmount> FlashAmountLookUp;
        public ComponentLookup<FlashSpeedData> FlashSpeedLookUp;

        // Dynamic buffer for storing damage events to be applied to enemies
        public BufferLookup<DamageThisFrame> DammageBufferLookup;

        public BufferLookup<HitEnemy> HitEnemyLookup;

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

            //Skip if this blast already hit this enemy ---
            var hitEnemyList = HitEnemyLookup[plasmaBlastEntity];
            for(int i = 0; i < hitEnemyList.Length; i++)
            {
                if (hitEnemyList[i].Value == enemyEntity)
                {
                    // This enemy has already been hit by this PlasmaBlast, skip it
                    return;
                }
            }

            // Record this enemy as hit before applying damage
            hitEnemyList.Add(new HitEnemy { Value = enemyEntity });

            var attackDamage = PlasmaBlastLookUp[plasmaBlastEntity].AttackDamage;

            
            // Get the dynamic buffer belonging to this particular Enemy entity and add the damage
            var enemyDynamicDamageBuffer = DammageBufferLookup[enemyEntity];
            enemyDynamicDamageBuffer.Add(new DamageThisFrame 
            { 
                Value = attackDamage 
            });
            
            if(FlashAmountLookUp.HasComponent(enemyEntity))
            {
                // We check if the enemy has the component to avoid errors, then set it to 1
                FlashAmountLookUp[enemyEntity] = new FlashAmount
                {
                    Value = 1.0f
                };

                // 2. Wake up the fade system for this specific enemy
                FlashSpeedLookUp.SetComponentEnabled(enemyEntity, true);
            }

            // Mark the PlasmaBlast entity for destruction (For now we don't want as we want to damage multiple enemies)
           // DestroyEntityLookUp.SetComponentEnabled(plasmaBlastEntity, true);
        }
    }
}
