using Survivors.Game;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;


// This system manages the lifecycle and updates of the player's world-space UI.
// It is responsible for:
// 1. Instantiating the UI prefab for the Player
// 2. Updating the UI position and health bar
// 3. Cleaning up the UI when the Player is no longer valid
partial struct PlayerWorldUISystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        // Create world-space UI for  the Player
        foreach (var (uiPrefab, entity) in
            SystemAPI.Query<PlayerWorldUIPrefab>().WithNone<PlayerWorldUI>().WithEntityAccess())
        {
            var newWorldUI = Object.Instantiate(uiPrefab.Value.Value);

            // Attach PlayerWorldUI component to store references to UI elements
            ecb.AddComponent(entity, new PlayerWorldUI
            {
                CanvasTransform = newWorldUI.transform,
                HealthBar = newWorldUI.GetComponentInChildren<Slider>()
            });
        }

        // Update world-space UI position and health bar value 
        foreach (var (transform, playerWorldUI, currentHitPoints, maxHitPoints) in 
                SystemAPI.Query<LocalToWorld,PlayerWorldUI, CharacterCurrentHitPoints, CharacterMaxHitPoints >())
        {
            playerWorldUI.CanvasTransform.Value.position = transform.Position;
            var healthValue = currentHitPoints.Value / maxHitPoints.Value;
            playerWorldUI.HealthBar.Value.value = healthValue;
        }

        // Cleanup world-space UI when the Player no longer has a transform
        foreach (var  (playerUI, entity) in SystemAPI.Query<PlayerWorldUI>().WithNone<LocalToWorld>().WithEntityAccess())
        {
           if(playerUI.CanvasTransform != null)
           {
                Object.Destroy(playerUI.CanvasTransform.Value.gameObject);
           }
            ecb.RemoveComponent<PlayerWorldUI>(entity);
        }

        // Apply all buffered structural changes
        ecb.Playback(state.EntityManager);
    }
}
   
