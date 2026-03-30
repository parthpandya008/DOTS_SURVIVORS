using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Survivors.Game
{
    public class EnemySpawnAuthoring : MonoBehaviour
    {
        public GameObject EnemyPrefab;
        public float SpawnInterval;
        public float SpawnDistance;
        public uint RandomSeed;

        private class Baker : Baker<EnemySpawnAuthoring>
        {
            public override void Bake(EnemySpawnAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EnemySpawnData
                {
                    EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic),
                    SpawnInterval = authoring.SpawnInterval,
                    SpawnDistance = authoring.SpawnDistance,

                });

                AddComponent(entity, new EnemySpawnState
                {
                    SpawnTimer = 0,
                    Random = Random.CreateFromIndex(authoring.RandomSeed),
                });
            }
        }
    }

    #region ComponentData
    public struct EnemySpawnData: IComponentData
    {
        public Entity EnemyPrefab;
        public float SpawnInterval;
        public float SpawnDistance;
    }

    public struct EnemySpawnState : IComponentData
    {
        public float SpawnTimer;
        public Random Random;
    }
    #endregion
}

