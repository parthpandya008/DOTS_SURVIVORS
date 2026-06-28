
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.EventSystems;


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
            var playerMoveJob = new PlayerMoveJob();
            systemState.Dependency = playerMoveJob.ScheduleParallel(systemState.Dependency);

            var enemyMoveJob = new CharacterMoveJob();
            systemState.Dependency = enemyMoveJob.ScheduleParallel(systemState.Dependency);
        }
    }

    [WithAll(typeof(PlayerTag))]
    [BurstCompile]
    public partial struct PlayerMoveJob : IJobEntity
    {
        public void Execute(RefRW<PhysicsVelocity> physicsVelocity, 
                            RefRW<FacingDirectionOverride> facingDirection,
                            RefRW<AnimationIndexOverride> animationOverride,
                            in CharacterMoveDirection moveDirection, 
                            in CharacterMoveSpeed moveSpeed)
        {
            var helper = new CharacterMoveHelper();
            var moveStep2d = helper.Move(moveDirection, moveSpeed, physicsVelocity, facingDirection);

            // Determine the animation type based on whether the Player is moving or idle
            var animationType = math.lengthsq(moveStep2d) > float.Epsilon ?
                                PlayerAnimationIndex.Move : 
                                PlayerAnimationIndex.Idle;
            animationOverride.ValueRW.Value = (float)animationType;
        }
    }
    


    [WithNone(typeof(PlayerTag))]
    [BurstCompile]
    public partial struct CharacterMoveJob : IJobEntity
    {
        public void Execute(RefRW<PhysicsVelocity> physicsVelocity, 
                            RefRW<FacingDirectionOverride> facingDirection,
                            in CharacterMoveDirection moveDirection, 
                            in CharacterMoveSpeed moveSpeed)
        {
            var helper = new CharacterMoveHelper();
            var moveStep2d = helper.Move(moveDirection, moveSpeed, physicsVelocity, facingDirection);
        }
    }  
}
