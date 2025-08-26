using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(CharacterAuthoring))]
public class EnemyAuthoring : MonoBehaviour
{
    public class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
          var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<EnemyTag>(entity); 
        }
    }
}

public struct EnemyTag : IComponentData { }