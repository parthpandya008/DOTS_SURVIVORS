using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;


namespace Survivors.Game
{
    /// <summary>
    /// CharacterMoveSystem is responsible for updating the movement of characters in the game.
    /// It uses SystemAPI to query entities with components related to movement and applies movement logic
    /// based on their current direction and speed. Additionally, it updates the facing direction and animation
    /// state of the Player if applicable.
    /// </summary>
    public partial struct CharacterMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState systemState)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (physicsVelocity, facingDirection, moveDirection, moveSpeed, entity) in
                SystemAPI.Query<RefRW<PhysicsVelocity>, RefRW<FacingDirectionOverride>,
                CharacterMoveDirection, CharacterMoveSpeed>().WithEntityAccess())
            {
                var moveStep2d = moveDirection.Value * moveSpeed.Value;

                //localTransform.ValueRW.Position += new float3(moveStep2d, 0);
                physicsVelocity.ValueRW.Linear = new float3(moveStep2d, 0);

                // Update the facing direction of the entity based on the X component of the movement
                if (math.abs(moveStep2d.x) > 0.1f)
                {
                    facingDirection.ValueRW.Value = math.sign(moveStep2d.x);
                }

                //  handling for Player entities
                if (SystemAPI.HasComponent<PlayerTag>(entity))
                {
                    var animationOverride = SystemAPI.GetComponentRW<AnimationIndexOverride>(entity);

                    // Determine the animation type based on whether the Player is moving or idle
                    var animationType = math.lengthsq(moveStep2d) > float.Epsilon ?
                                            PlayerAnimationIndex.Move : PlayerAnimationIndex.Idle;

                    animationOverride.ValueRW.Value = (float)animationType;
                }
            }
        }
    }
}
