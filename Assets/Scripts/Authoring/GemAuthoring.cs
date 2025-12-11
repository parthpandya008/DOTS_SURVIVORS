using Survivors.Game;
using Unity.Entities;
using UnityEngine;

public class GemAuthoring : MonoBehaviour
{
    private class Baker : Baker<GemAuthoring>
    {
        public override void Bake(GemAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<GemTag>(entity);
            AddComponent<DestroyEntityFlag>(entity);
            SetComponentEnabled<DestroyEntityFlag>(entity, false);
           
        }
    }

}

#region ComponentData
public struct GemTag : IComponentData { }

#endregion
