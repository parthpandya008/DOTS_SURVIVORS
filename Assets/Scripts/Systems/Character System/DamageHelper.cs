using Survivors.Game;
using Unity.Entities;
using UnityEngine;

// Shared damage application logic
public static class DamageHelper
{
    public static bool ApplyDamage(
            ref RefRW<CharacterCurrentHitPoints> characterCurrentHitPoints,
            DynamicBuffer<DamageThisFrame> damageThisFrame)
    {
        // Skip entities that received no damage this frame
        if (damageThisFrame.IsEmpty == true) return false;

        // Apply each damage event stored in the buffer
        foreach (var damage in damageThisFrame)
        {
            characterCurrentHitPoints.ValueRW.Value -= damage.Value;
        }

        damageThisFrame.Clear();

        return characterCurrentHitPoints.ValueRO.Value <= 0;
    }
}
