using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;



public class PlayerAuthoring : MonoBehaviour
{
    public GameObject AttackPrefab;
    public float CoolDownTime;
    public float DetectationSize;
    public CollisionFilter CollisionFilter;

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerTag>(entity);
            AddComponent<InitCameraTargetTag>(entity);
            AddComponent<CameraTarget>(entity);
            AddComponent<AnimationIndexOverride>(entity);

            var enemyLayer = LayerMask.NameToLayer("Enemy");
            var enemyLayerMask = (uint)math.pow(2, enemyLayer);

            var atackCollisionFilter = new CollisionFilter();
            atackCollisionFilter.BelongsTo = uint.MaxValue;
            atackCollisionFilter.CollidesWith = enemyLayerMask;

            AddComponent(entity, new PlayerAttackData
            {
                AttackPrefab = GetEntity(authoring.AttackPrefab, TransformUsageFlags.Dynamic),
                CoolDownTime = authoring.CoolDownTime,
                DetectationSize = authoring.DetectationSize,
                CollisionFilter = atackCollisionFilter
            });

            AddComponent<PlayerCoolDownExpirationTimestamp>(entity);
            
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

public struct PlayerAttackData: IComponentData
{
    public Entity AttackPrefab;
    public float CoolDownTime;
    public float DetectationSize;
    public CollisionFilter CollisionFilter;
}

public struct PlayerCoolDownExpirationTimestamp: IComponentData
{
    public double Value;
}