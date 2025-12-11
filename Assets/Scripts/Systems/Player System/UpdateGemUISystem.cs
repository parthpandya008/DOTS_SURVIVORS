using Survivors.Game;
using Survivors.UI;
using Unity.Burst;
using Unity.Entities;

/* We have a separate system for the gem update because we need GemCollectJob with BurstCompile 
   (as we deal with a large number of gem entities). 
   With BurstCompile, we cannot update the UI, so we use a different system to handle UI updates 
   and then disable it afterward. */
partial struct UpdateGemUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach(var (gemsCollectedCount, updateGemUIFlag) 
            in SystemAPI.Query<GemsCollectedCount, EnabledRefRW<UpdateGemUIFlag>>())
        {
            // Update the gem UI with the new gem count.
            GameUIController.Instance.UpdateGemsCollectedText(gemsCollectedCount.Value);
            updateGemUIFlag.ValueRW = false;
        }
    }
}
