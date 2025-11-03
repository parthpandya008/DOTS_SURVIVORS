using System.Numerics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct EnemyMoveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position.xy;
        var enemyMoveJob = new EnemyMoveJob
        {
            PlayerPosition = playerPosition
        };

        state.Dependency = enemyMoveJob.ScheduleParallel(state.Dependency);
    }
}

[WithAll(typeof(EnemyTag))]
[BurstCompile]
public partial struct EnemyMoveJob: IJobEntity
{
    public float2 PlayerPosition;
    
    public void Execute(ref CharacterMoveDirection moveDirection, in LocalTransform localTransform)
    {
        var playerDirection = PlayerPosition - localTransform.Position.xy;
        moveDirection.Value = math.normalize (playerDirection);
    }
}
