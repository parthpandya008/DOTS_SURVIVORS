using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerTag>(entity);
            AddComponent<InitCameraTargetTag>(entity);
            AddComponent<CameraTarget>(entity);
            AddComponent<AnimationIndexOverride>(entity);
        }
    }
}

public struct PlayerTag: IComponentData { }

public struct CameraTarget: IComponentData 
{
    public UnityObjectRef<Transform> CameraTransform;
}

//This component is to set CameraTarget.CameraTransform once, then remove it
public struct InitCameraTargetTag: IComponentData { }

[MaterialProperty("_AnimationIndex")]
public struct AnimationIndexOverride: IComponentData
{
    public float Value;
}

public enum PlayerAnimationIndex: byte
{
    //Animation indexes starts form the bottom, so the  movemnet is the first then the idle
    Move = 0,
    Idle = 1,

    None = byte.MaxValue
}