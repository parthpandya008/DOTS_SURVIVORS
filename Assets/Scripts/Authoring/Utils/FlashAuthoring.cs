using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Survivors.Game
{
    public class FlashAuthoring : MonoBehaviour
    {
        [Tooltip("How fast the white flash fades away. Higher = faster.")]
        public float FlashSpeed = 10.0f;

        [Tooltip("The default color of the flash.")]
        [ColorUsage(true,true)]
        public Color FlashColor = Color.white;

        public class Baker : Baker<FlashAuthoring>
        {
            public override void Bake(FlashAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                //  The GPU Data
                AddComponent(entity, new FlashAmount
                {
                    Value = 0f
                });
                
                AddComponent(entity, new FlashColorData
                {
                    Value = new float4(authoring.FlashColor.r,
                                        authoring.FlashColor.g,
                                        authoring.FlashColor.b,
                                        authoring.FlashColor.a)
                });

                // The CPU Data 
                AddComponent(entity, new FlashSpeedData
                {
                    Value = authoring.FlashSpeed
                });
                SetComponentEnabled<FlashSpeedData>(entity, false);
            }
        }
    }

    // This MUST perfectly match the Reference name in your Shader Graph
    [MaterialProperty("_FlashAmount")]
    public struct FlashAmount : IComponentData
    {
        public float Value;
    }

    // This MUST perfectly match the Reference name of the Color property in your Shader Graph
    [MaterialProperty("_FlashColor")]
    public struct FlashColorData : IComponentData
    {
        public float4 Value;
    }

    /*
     * FlashAmount has the [MaterialProperty] tag. That tag tells Unity to copy that exact chunk of memory directly to the GPU every frame. 
     * If we put "Speed" in there, we are sending useless data to the graphics card, wasting memory bandwidth.
     * Why me made this as IEnableableComponent not FlashAmount
     *      If [MaterialProperty] is attached on the component Unity's rendering system  continuously gather that data from all entities
     *      and pack it into a special array for the GPU (GPU Instancing).
     *      So when you enable and disable it would add and remove that component from that special array.
     *      Dynamically adding and removing material overrides frame-by-frame 
     *      can cause batch fragmentation, memory reallocation overhead, 
     *      or even visual flickering as entities jump between different rendering batches.
     */
    public struct FlashSpeedData : IComponentData, IEnableableComponent
    {
        public float Value;
    }
}
