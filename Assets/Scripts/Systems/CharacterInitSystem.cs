using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[UpdateInGroup(typeof(InitializationSystemGroup))]
partial struct CharacterInitSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var (physicsMass, initFlag) in 
            SystemAPI.Query<RefRW <PhysicsMass>, EnabledRefRW <InitCharacterFlag>>())
        {
            physicsMass.ValueRW.InverseInertia = float3.zero;
            initFlag.ValueRW = false;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
