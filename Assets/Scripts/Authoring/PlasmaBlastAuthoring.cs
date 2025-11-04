using Unity.Entities;
using UnityEngine;

namespace Survivors.Game
{
    public class PlasmaBlastAuthoring : MonoBehaviour
    {
        public float MoveSpeed;
        public int AttackDamage;
        public float LifeSpanDuration;

        public class Baker : Baker<PlasmaBlastAuthoring>
        {
            public override void Bake(PlasmaBlastAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlasmaBlastData
                {
                    AttackDamage = authoring.AttackDamage,
                    MoveSpeed = authoring.MoveSpeed,
                    LifeSpanDuration = authoring.LifeSpanDuration,
                });
                AddComponent<DestroyEntityFlag>(entity);
                SetComponentEnabled<DestroyEntityFlag>(entity, false);
                AddComponent<EntityLifetime>(entity);
            }
        }
    }

     #region ComponentData
    public struct PlasmaBlastData : IComponentData
    {
        public float MoveSpeed;
        public int AttackDamage;
        public float LifeSpanDuration;
    }
    #endregion
}
