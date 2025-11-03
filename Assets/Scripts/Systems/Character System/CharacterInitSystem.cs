using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[UpdateInGroup(typeof(InitializationSystemGroup))]
partial struct CharacterInitSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var (physicsMass, initFlag) in 
            SystemAPI.Query<RefRW <PhysicsMass>, EnabledRefRW <InitCharacterFlag>>())
        {
            //Set InverseInertia, so the player doen't rotate on collission from the 
            physicsMass.ValueRW.InverseInertia = float3.zero;
            
            //Set InitCharacterFlag component flase, so that this doesn't set every time
            initFlag.ValueRW = false;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
