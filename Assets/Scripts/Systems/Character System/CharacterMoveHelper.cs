using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Survivors.Game
{
    public readonly struct CharacterMoveHelper
    {
        public float2 Move(in CharacterMoveDirection moveDirection, in CharacterMoveSpeed moveSpeed,
                            RefRW<PhysicsVelocity> physicsVelocity, RefRW<FacingDirectionOverride> facingDirection)
        {
            var moveStep2d = moveDirection.Value * moveSpeed.Value;

            physicsVelocity.ValueRW.Linear = new float3(moveStep2d, 0);

            // Update the facing direction of the entity based on the X component of the movement
            if (math.abs(moveStep2d.x) > 0.1f)
            {
                facingDirection.ValueRW.Value = math.sign(moveStep2d.x);
            }

            return moveStep2d;
        }
    }
}