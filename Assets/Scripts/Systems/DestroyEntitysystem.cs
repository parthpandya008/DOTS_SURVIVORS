using Survivors.UI;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Rendering.VirtualTexturing;

namespace Survivors.Game
{
    //Run this system at late inside the SimulationSystemGroup. We need to check,
    //if entity needs to be destroyed, spawing (gems) or showing Game over UI
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    
    //The EndSimulationEntityCommandBufferSystem is the moment when all buffered structural changes for this frame are finally executed.
    //As we are destroying the entites, the system should run beofre ECB playback.
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    partial struct DestroyEntitysystem : ISystem
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //A system that buffers structural changes (ex: destroy) and plays them back at the end
            //It batches all entity changes and applies them only once per frame.
            var endECBSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var endECB = endECBSystem.CreateCommandBuffer(state.WorldUnmanaged);

            //A system that plays back its ECB at the very start of the next frame
            //If you use EndSimulation ECB to destroy an entity and want to spawn something immediately at the start of the next frame
            //Begin ECB for gem spawing
            var beginECBSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var beginECB = beginECBSystem.CreateCommandBuffer(state.WorldUnmanaged);
           
            foreach (var (_, entity) in SystemAPI.Query<DestroyEntityFlag>().WithEntityAccess())
            {
                //Show Game Over UI on player destroyed
                if (SystemAPI.HasComponent<PlayerTag>(entity))
                {
                    GameUIController.Instance.ShowGameOverUI();
                }

                //Spawn gem on enemy destroyed
                if (SystemAPI.HasComponent<GemPrefab>(entity))
                {
                    //Instantiate GemPrefab
                    var gemPrefeb = SystemAPI.GetComponent<GemPrefab>(entity).Value;
                    var newGem = beginECB.Instantiate(gemPrefeb);

                    //Set enemy position to newly spawnGem
                    var spawnPosition = SystemAPI.GetComponent<LocalTransform>(entity).Position;
                    beginECB.SetComponent(newGem, LocalTransform.FromPosition(spawnPosition));
                }

                endECB.DestroyEntity(entity);
            }
        }

    }
}
