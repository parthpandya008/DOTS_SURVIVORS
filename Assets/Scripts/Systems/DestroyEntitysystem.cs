using Survivors.UI;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Game
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    partial struct DestroyEntitysystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var endECBSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var endECB = endECBSystem.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (_, entity) in SystemAPI.Query<DestroyEntityFlag>().WithEntityAccess())
            {
                if (SystemAPI.HasComponent<PlayerTag>(entity))
                {
                    GameUIController.Instance.ShowGameOverUI();
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
