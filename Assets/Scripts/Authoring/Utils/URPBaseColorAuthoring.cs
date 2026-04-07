using System.Drawing;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Survivors.Game
{
    // add URPMaterialPropertyBaseColor so the system can write alpha
    public class URPBaseColorAuthoring : MonoBehaviour
    {
        private class Baker : Baker<URPBaseColorAuthoring>
        {
            public override void Bake(URPBaseColorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                UnityEngine.Color color = UnityEngine.Color.white; // Initialize color to avoid unassigned variable issues
                                                                   // Try to get Renderer and its material color
                var renderer = authoring.GetComponent<Renderer>();
                
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    if (renderer.sharedMaterial.HasProperty("_BaseColor"))
                    {
                        color = renderer.sharedMaterial.GetColor("_BaseColor");
                        // Convert to linear space so DOTS/URP shader interprets the color correctly
                        color = color.linear;
                    }
                }

                AddComponent(entity, new URPMaterialPropertyBaseColor
                {
                    Value = new float4(color.r, color.g, color.b, color.a)
                });
            }
        }
    }
}
