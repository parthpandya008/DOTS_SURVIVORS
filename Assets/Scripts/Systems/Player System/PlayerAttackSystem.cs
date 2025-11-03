using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Survivors.Game
{
    partial struct PlayerAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var elapasedTime = SystemAPI.Time.ElapsedTime;
            var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();


            foreach (var (expirationTimeStamp, attackData, transform)
                        in SystemAPI.Query<RefRW<PlayerCoolDownExpirationTimestamp>,
                                            PlayerAttackData, LocalTransform>())
            {
                // Skip this entity if the cooldown time has not yet expired
                if (expirationTimeStamp.ValueRO.Value > elapasedTime) continue;

                var spawnPosition = transform.Position;

                // Calculate the bounding box (AABB) for detecting nearby enemies
                var minPosition = spawnPosition - attackData.DetectationSize;
                var maxPosition = spawnPosition + attackData.DetectationSize;

                Aabb aabb = new Aabb
                {
                    Max = maxPosition,
                    Min = minPosition
                };

                // Prepare input for the OverlapAABB physics query
                var aabbInput = new OverlapAabbInput
                {
                    Aabb = aabb,
                    Filter = attackData.CollisionFilter
                };

                var overlapHits = new NativeList<int>(state.WorldUpdateAllocator);

                // overlap query to find entities within the attack range
                if (!physicsWorldSingleton.OverlapAabb(aabbInput, ref overlapHits))
                {
                    continue;
                }

                var minDistSq = float.MaxValue;
                var closestEnemyPosition = float3.zero;

                // Iterate over the overlap hits to find the closest enemy
                foreach (var hit in overlapHits)
                {
                    //Get the collided enemy position from the Physics World
                    var enemyPosition = physicsWorldSingleton.Bodies[hit].WorldFromBody.pos;
                    var distSq = math.distancesq(spawnPosition.xy, enemyPosition.xy);
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        closestEnemyPosition = enemyPosition;
                    }
                }

                var dir = closestEnemyPosition - spawnPosition;
                var angle = math.atan2(dir.y, dir.x);
                var spawnRotation = quaternion.Euler(0, 0, angle);

                // Instantiate the attack prefab and set its position and rotation
                var newAttack = ecb.Instantiate(attackData.AttackPrefab);
                ecb.SetComponent(newAttack, LocalTransform.FromPositionRotation(spawnPosition, spawnRotation));

                // Update the cooldown expiration timestamp for the entity
                expirationTimeStamp.ValueRW.Value = elapasedTime + attackData.CoolDownTime;
            }
        }
    }
}
