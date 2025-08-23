using System.Linq.Expressions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
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
            AddComponent<InitCharacterFlag>(entity);
            AddComponent(entity, new CharacterMoveSpeed
            {
                Value = authoring.MoveSpeed,
            });
            AddComponent(entity, new FacingDirectionOverride
            {
                Value = 1
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

public struct InitCharacterFlag : IComponentData, IEnableableComponent { }

[MaterialProperty("_FacingDirection")]
public struct FacingDirectionOverride: IComponentData
{
    public float Value;
}