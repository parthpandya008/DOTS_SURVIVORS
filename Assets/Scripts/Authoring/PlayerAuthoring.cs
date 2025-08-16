using Unity.Entities;
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