using Survivors.Game;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BossSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var elapsedTime = (float)SystemAPI.Time.ElapsedTime;
        var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

        // WithAll<BossSpawnState> means the query only matches while BossSpawnState is *enabled*.
        // Once we disable it below, this entity never enters the loop again — free self-disabling.
        foreach (var (bossSpawnData, spawnState , entity) in
                        SystemAPI.Query<BossSpawnData, RefRW<BossSpawnState>>()
                        .WithEntityAccess())
        {
            if (elapsedTime < bossSpawnData.SpawnDelay)
                continue;

            // Spawn the boss
            var boss = ecb.Instantiate(bossSpawnData.BossPrefab);

            // Calculate a random spawn position based on a random angle and spawn distance.
            var spawnAngle = spawnState.ValueRW.Random.NextFloat(0, math.TAU); //math.TAU = 2Pi = a full circle
            var spawnPosition = new float3
            {
                x = math.cos(spawnAngle),
                y = math.sin(spawnAngle),
                z = 0
            };
            spawnPosition *= bossSpawnData.SpawnDistance;
            spawnPosition += playerPosition;
            ecb.SetComponent(boss, LocalTransform.FromPosition(spawnPosition));

            // Disable BossSpawnState so this system never processes this entity again
            ecb.SetComponentEnabled<BossSpawnState>(entity, false);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
