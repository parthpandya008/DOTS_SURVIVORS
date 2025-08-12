using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public partial struct CharacterMoveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState systemState)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        foreach(var (physicsVelocity, moveDirection, moveSpeed) in 
            SystemAPI.Query<RefRW <PhysicsVelocity>, CharacterMoveDirection, CharacterMoveSpeed>())
        {
            var moveStep2d = moveDirection.Value * moveSpeed.Value;

            //localTransform.ValueRW.Position += new float3(moveStep2d, 0);
            physicsVelocity.ValueRW.Linear = new float3(moveStep2d, 0);
        }
    }
}
