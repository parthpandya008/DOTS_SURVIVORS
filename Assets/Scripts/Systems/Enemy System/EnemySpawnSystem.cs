using Survivors.Game;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct EnemySpawnSystem : ISystem
{
    // EnemySpawnSystem is an ECS system responsible for spawning enemies at regular intervals
    // around the player's position.
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        // Get the Entity Command Buffer system 
        var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        // Get the player entity and its position.
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

        foreach(var (spawnState, spawnData) in 
            SystemAPI.Query<RefRW <EnemySpawnState>, EnemySpawnData>())
        {
            spawnState.ValueRW.SpawnTimer -= deltaTime;
            if(spawnState.ValueRO.SpawnTimer > 0 ) continue;
            spawnState.ValueRW.SpawnTimer = spawnData.SpawnInterval;

            // Spawn a new enemy entity
            var newEnemy = ecb.Instantiate(spawnData.EnemyPrefab);
            
            // Calculate a random spawn position based on a random angle and spawn distance.
            var spawnAngle = spawnState.ValueRW.Random.NextFloat(0, math.TAU); //math.TAU = 2Pi = a full circle
            var spawnPos = new float3
            {
                x = math.cos(spawnAngle),
                y = math.sin(spawnAngle),
                z = 0
            };
            spawnPos *= spawnData.SpawnDistance;
            spawnPos += playerPosition;

            // Set the spawned enemy's position using its LocalTransform component.
            ecb.SetComponent(newEnemy, LocalTransform.FromPosition(spawnPos));
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
