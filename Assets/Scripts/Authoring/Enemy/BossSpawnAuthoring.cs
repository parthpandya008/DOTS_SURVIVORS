using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Survivors.Game
{
    public class BossSpawnAuthoring : MonoBehaviour
    {
        public GameObject BossPrefab;
        public float SpawnDelay; // seconds after game start before boss spawns
        public float SpawnDistance; // distance from player when spawned
        public uint RandomSeed;

        private class Baker : Baker<BossSpawnAuthoring>
        {
            public override void Bake(BossSpawnAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new BossSpawnData
                {
                    //Convert Boss game object into entity
                    BossPrefab = GetEntity(authoring.BossPrefab, TransformUsageFlags.Dynamic),
                    SpawnDelay = authoring.SpawnDelay,
                    SpawnDistance = authoring.SpawnDistance
                });

                //AddComponent<BossSpawnState>(entity);
                AddComponent(entity, new BossSpawnState
                {
                    Random = Random.CreateFromIndex(authoring.RandomSeed)
                });
            }
        }
    }

    #region ComponentData

    public struct BossSpawnData: IComponentData
    {
        public Entity BossPrefab;
        public float SpawnDelay;
        public float SpawnDistance;
    }

    // IEnableableComponent — disabled by the system after the boss has been spawned,
    // so the query naturally stops matching and the system does zero work afterward.
    public struct BossSpawnState: IComponentData, IEnableableComponent 
    {
        public Random Random;
    }

    #endregion
}
