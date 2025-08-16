using Unity.Burst;
using Unity.Entities;

// System runs in InitializationSystemGroup to ensure camera setup happens 
// before other systems that might depend on camera data
[UpdateInGroup(typeof(InitializationSystemGroup))]
partial struct CameraInitSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Only run this system when entities with InitCameraTargetTag exist
        state.RequireForUpdate<InitCameraTargetTag>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        if (CameraTargetSingleton.Instance == null) return;

        // Cache the camera target transform from the managed singleton
       var camTargetTransform = CameraTargetSingleton.Instance.transform;

        // Create EntityCommandBuffer using WorldUpdateAllocator for automatic cleanup
        // WorldUpdateAllocator provides fast, frame-scoped memory allocation
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach(var (camTarget, entity) in 
            SystemAPI.Query<RefRW <CameraTarget>>().WithAll<InitCameraTargetTag, PlayerTag>().WithEntityAccess())
        {
            // Set the camera transform reference in the CameraTarget component,
                // so CameraMoveSystem can update position accroidng to the Player movement
            camTarget.ValueRW.CameraTransform = camTargetTransform;
            ecb.RemoveComponent<InitCameraTargetTag>(entity);
        }
        ecb.Playback(state.EntityManager);
    }
}
