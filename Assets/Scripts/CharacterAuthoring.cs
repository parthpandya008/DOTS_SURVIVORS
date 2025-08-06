using System.Linq.Expressions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CharacterAuthoring : MonoBehaviour
{
    public float MoveSpeed = 3f;
    private class Baker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            //Take the entity ref that is being baked,
           var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<CharacterMoveDirection>(entity);
            AddComponent(entity, new CharacterMoveSpeed
            {
                Value = authoring.MoveSpeed,
            });
        }
    }
}

public struct CharacterMoveDirection: IComponentData
{
    public float2 Value;
}

public struct CharacterMoveSpeed:IComponentData
{
    public float Value;
}