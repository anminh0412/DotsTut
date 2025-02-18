namespace Test
{
    using Unity.Entities;
    using UnityEngine;

    public class EnemyAuthoring : MonoBehaviour
    {
        public GameObject EnemyPrefab;

        class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
               
                AddComponent(entity, new EnemySpawnerComponent
                {
                    EnemyPrefab       = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic),
                    SpawnCount        = 10000,
                    CurrentSpawnCount = 0,
                });
            }
        }
    }
}