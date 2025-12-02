using Survivors.UI;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Survivors.Game
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
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
            var endECBSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var endECB = endECBSystem.CreateCommandBuffer(state.WorldUnmanaged);
            
            //Begin ECB for gem spawing
            var beginECBSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var beginECB = beginECBSystem.CreateCommandBuffer(state.WorldUnmanaged);
           
            foreach (var (_, entity) in SystemAPI.Query<DestroyEntityFlag>().WithEntityAccess())
            {
                if (SystemAPI.HasComponent<PlayerTag>(entity))
                {
                    GameUIController.Instance.ShowGameOverUI();
                }
                
                if(SystemAPI.HasComponent<GemPrefab>(entity))
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

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
