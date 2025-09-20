using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Physics.Systems;


[UpdateInGroup(typeof(PhysicsSystemGroup))]// ensures we run in the physics pipeline
[UpdateAfter(typeof(PhysicsSimulationGroup))]// run *after* physics has advanced and generated collision events
[UpdateBefore(typeof(AfterPhysicsSystemGroup))]//before post-physics cleanup/logic runs
partial struct EnemyAttackSystem : ISystem
{
    // A temporary list for debugging/logging collided entities
    private NativeList<Entity> logEntities;
   public void OnCreate(ref SystemState state)
    {
        // SimulationSingleton is the central object that represents a physics world simulation.
        // Without this, we cannot schedule ICollisionEventsJob.

        state.RequireForUpdate<SimulationSingleton>();
        logEntities = new NativeList<Entity>(Allocator.Persistent);
    }

    public  void OnDestroy()
    {
        // Dispose of the NativeList when the system is destroyed
       
        if(logEntities.IsCreated)
            logEntities.Dispose();
       
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!logEntities.IsCreated) return;

            logEntities.Clear();

        var elapsedTime = SystemAPI.Time.ElapsedTime;
        foreach(var (expirationTimeStamp, cooldownEnabled) 
            in SystemAPI.Query<EnemyCoolDownExpirationTimeStamp, EnabledRefRW<EnemyCoolDownExpirationTimeStamp>>())
        {
            // Disable cooldown if its expiration timestamp has passed
            // This ensures enemies can attack again after cooldown
            if (expirationTimeStamp.Value > elapsedTime) continue;

            cooldownEnabled.ValueRW = false;
        }

        // The job processes physics collision events and applies damage
        var attackJob = new EnemyAttackJob
        {
            //a ComponentLookup lets you directly check or fetch a component from any entity you KNOW.
            PlayerLookup = SystemAPI.GetComponentLookup<PlayerTag>(true),
            AttackDataLookup = SystemAPI.GetComponentLookup<EnemyAttackData>(true),
            CooldownExpirationLookup = SystemAPI.GetComponentLookup<EnemyCoolDownExpirationTimeStamp>(),
            DammageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>(),
            ElapsedTime = elapsedTime,
            LogEntities = logEntities,
        };

        // SimulationSingleton object holds the physics simulation world state.
        // Needed for scheduling jobs that work with physics events like ICollisionEventsJob and ITriggerEventsJob.
        // Without this singleton, there’s no physics simulation context.
        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = attackJob.Schedule(simulationSingleton, state.Dependency);

        // For debugging: force complete immediately so we can log results this frame
        // (in production you usually don’t Complete here, to keep jobs async)
        state.Dependency.Complete();

        foreach (var entity in logEntities)
        {
            UnityEngine.Debug.Log($"Collision detected with EntityA: {entity}");
        }
    }
}

[BurstCompile]
public struct EnemyAttackJob : ICollisionEventsJob
{
    // Lookups allow random access to components during job execution
    [Unity.Collections.ReadOnly]
    public ComponentLookup<PlayerTag> PlayerLookup;
    [Unity.Collections.ReadOnly]
    public ComponentLookup<EnemyAttackData> AttackDataLookup;

    public ComponentLookup<EnemyCoolDownExpirationTimeStamp> CooldownExpirationLookup;
    public BufferLookup<DamageThisFrame> DammageBufferLookup;

    public double ElapsedTime;

    public NativeList<Entity> LogEntities;

    public void Execute(CollisionEvent collisionEvent)
    {
        //UnityEngine.Debug.Log(" collisionEvent.EntityA " + collisionEvent.EntityA);
        Entity playerEntity;
        Entity enemyEntity;
        
        LogEntities.Add(collisionEvent.EntityA);
        LogEntities.Add(collisionEvent.EntityB);

        // Identify which entity is player and which is enemy
        //HasComponent Returns true if the passed Entity currently has the component type T

        if (PlayerLookup.HasComponent(collisionEvent.EntityA) && AttackDataLookup.HasComponent(collisionEvent.EntityB))
        {
            playerEntity = collisionEvent.EntityA;
            enemyEntity = collisionEvent.EntityB;
        }
        else if (PlayerLookup.HasComponent(collisionEvent.EntityB) && AttackDataLookup.HasComponent(collisionEvent.EntityA))
        {
            playerEntity = collisionEvent.EntityB;
            enemyEntity = collisionEvent.EntityA;
        }
        else
        {
            return;
        }

        // Skip if enemy is currently in cooldown
        if (CooldownExpirationLookup.IsComponentEnabled(enemyEntity))
        {
            return;
        }
        // Enable cooldown period
        CooldownExpirationLookup.SetComponentEnabled(enemyEntity, true);

        // Set next expiration time
        var attackData = AttackDataLookup[enemyEntity];
        CooldownExpirationLookup[enemyEntity] = new EnemyCoolDownExpirationTimeStamp
        {
            Value = attackData.CoolDownTime + ElapsedTime
        };

        // Apply damage to the player via dynamic buffer
        var playerDamageBuffer = DammageBufferLookup[playerEntity];
        playerDamageBuffer.Add(new DamageThisFrame
        {
            Value = attackData.HitPoints
        });

        
    }
}
