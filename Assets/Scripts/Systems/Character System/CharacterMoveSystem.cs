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

        foreach(var (physicsVelocity, facingDirection ,moveDirection, moveSpeed, entity) in 
            SystemAPI.Query<RefRW <PhysicsVelocity>, RefRW<FacingDirectionOverride>,
            CharacterMoveDirection, CharacterMoveSpeed>().WithEntityAccess())
        {
            var moveStep2d = moveDirection.Value * moveSpeed.Value;

            //localTransform.ValueRW.Position += new float3(moveStep2d, 0);
            physicsVelocity.ValueRW.Linear = new float3(moveStep2d, 0);

            if(math.abs(moveStep2d.x) > 0.1f)
            {
                facingDirection.ValueRW.Value = math.sign(moveStep2d.x);
            }

            if (SystemAPI.HasComponent<PlayerTag>(entity))
            {
                var animationOverride = SystemAPI.GetComponentRW<AnimationIndexOverride>(entity);

                var animationType = math.lengthsq(moveStep2d) > float.Epsilon ?
                                        PlayerAnimationIndex.Move : PlayerAnimationIndex.Idle;

                animationOverride.ValueRW.Value = (float)animationType;
            }
        }
    }
}
