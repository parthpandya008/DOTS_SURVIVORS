using Unity.Entities;
using UnityEngine;

namespace Survivors.Game
{
    [RequireComponent(typeof(CharacterAuthoring))]
    public class EnemyAuthoring : MonoBehaviour
    {
        public float AttackDamage;
        public float CoolDownTime;

        public class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<EnemyTag>(entity);
                AddComponent(entity, new EnemyAttackData
                {
                    HitPoints = authoring.AttackDamage,
                    CoolDownTime = authoring.CoolDownTime

                });
                AddComponent<EnemyCoolDownExpirationTimeStamp>(entity);
                SetComponentEnabled<EnemyCoolDownExpirationTimeStamp>(entity, false);
            }
        }
    }


    #region ComponentData
    public struct EnemyTag : IComponentData { }

    public struct EnemyAttackData : IComponentData
    {
        public float HitPoints;
        public double CoolDownTime;
    }

    public struct EnemyCoolDownExpirationTimeStamp : IComponentData, IEnableableComponent
    {
        public double Value;
    }
    #endregion
}