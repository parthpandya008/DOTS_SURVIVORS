using System.Numerics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Survivors.Game
{
    // System runs after TransformSystemGroup to ensure all transform calculations
    // are complete before updating camera positions
    [UpdateAfter(typeof(TransformSystemGroup))]
    partial struct CameraMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            //LocalToWorld is Unity DOTS' solution for providing fast,
            //cached access to final world-space transformations
            foreach (var (transform, camTarget) in
                SystemAPI.Query<LocalToWorld, CameraTarget>().
                WithAll<PlayerTag>().WithNone<InitCameraTargetTag>())
            {
                // Update the managed Unity Camera Transform position to match the entity's world position
                // This creates a bridge between ECS entity position and traditional Unity camera

                float3 currentCamPos = camTarget.CameraTransform.Value.position;
                float3 playerPos = transform.Position;

                // Interpolate between current position and player position
                float3 lerpPos = math.lerp(currentCamPos, playerPos, camTarget.SmoothSpeed * Time.deltaTime);
                camTarget.CameraTransform.Value.position = lerpPos;
            }
        }
    }
}