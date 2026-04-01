using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Survivors.Game
{
    public class FadeOutAuthoring : MonoBehaviour
    {
        [Header("Fade Out")]       
        public float FadeOutDuration = 1f;
        public bool DestroyOnFadeOutComplete;

        public class Baker : Baker<FadeOutAuthoring>
        {
            public override void Bake(FadeOutAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new FadeOutData
                {
                    Duration = authoring.FadeOutDuration,
                    DestroyOnComplete = authoring.DestroyOnFadeOutComplete
                });
                // Starts disabled — enable via ECB when you want the fade to begin
                SetComponentEnabled<FadeOutData>(entity, false);

                // add URPMaterialPropertyBaseColor so the system can write alpha
                AddComponent(entity, new URPMaterialPropertyBaseColor
                {
                    Value = new Unity.Mathematics.float4(1, 1, 1, 1)
                });
            }
        }

        #region ComponentData

        public struct FadeOutData : IComponentData, IEnableableComponent
        {
            public float Elapsed;
            public float Duration;
            public bool DestroyOnComplete;
        }

        #endregion
    }
}
