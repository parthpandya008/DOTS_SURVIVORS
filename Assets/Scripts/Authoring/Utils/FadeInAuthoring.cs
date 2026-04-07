using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Survivors.Game
{
    [RequireComponent(typeof(URPBaseColorAuthoring))]
    public class FadeInAuthoring : MonoBehaviour
    {
        [Header("Fade In")]        
        public float FadeInDuration = 1f;

        public class Baker : Baker<FadeInAuthoring>
        {
            public override void Bake(FadeInAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new FadeInData
                {
                    Duration = authoring.FadeInDuration
                });                                                                                               
            }
        }
       
    }

    #region ComponentData
    public struct FadeInData : IComponentData, IEnableableComponent
    {
        public float Elapsed;
        public float Duration;
    }

    #endregion
}
