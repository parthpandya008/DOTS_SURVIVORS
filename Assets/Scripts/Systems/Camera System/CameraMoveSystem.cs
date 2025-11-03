using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

// System runs after TransformSystemGroup to ensure all transform calculations
// are complete before updating camera positions
[UpdateAfter(typeof(TransformSystemGroup))]
partial struct CameraMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        //LocalToWorld is Unity DOTS' solution for providing fast,
        //cached access to final world-space transformations
        foreach (var (transform, camTarget) in 
            SystemAPI.Query<LocalToWorld, CameraTarget>().
            WithAll<PlayerTag>().WithNone<InitCameraTargetTag>())
        {
            // Update the managed Unity Camera Transform position to match the entity's world position
            // This creates a bridge between ECS entity position and traditional Unity camera
            camTarget.CameraTransform.Value.position = transform.Position;
        }
    }
}
