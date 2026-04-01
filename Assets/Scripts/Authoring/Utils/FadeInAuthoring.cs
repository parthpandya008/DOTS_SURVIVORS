using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Survivors.Game
{
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

                // add URPMaterialPropertyBaseColor so the system can write alpha
                AddComponent(entity, new URPMaterialPropertyBaseColor
                {
                    Value = new Unity.Mathematics.float4(1, 1, 1, 0)
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
