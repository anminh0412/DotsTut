namespace Test
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Rendering;
    using Unity.Transforms;
    using UnityEngine;
    using Random = Unity.Mathematics.Random;

    // public partial struct EnemySpawnerSystem : ISystem
    // {
    //     public void OnCreate(ref SystemState state) { state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>(); }
    //
    //     public void OnUpdate(ref SystemState state)
    //     {
    //         var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
    //         var ecb          = ecbSingleton.CreateCommandBuffer(World.DefaultGameObjectInjectionWorld.Unmanaged);
    //
    //         foreach (var spawner in SystemAPI.Query<RefRW<EnemySpawnerComponent>>())
    //         {
    //             if (spawner.ValueRW.CurrentSpawnCount <= spawner.ValueRW.SpawnCount)
    //             {
    //                 Entity                   enemy    = ecb.Instantiate(spawner.ValueRO.EnemyPrefab);
    //                 float3                   spawnPos = new float3();
    //                 uint                     seed     = (uint)(Time.realtimeSinceStartup * 1000) + 1;
    //                 Random rng      = new Random(seed);
    //                 spawnPos.x = rng.NextFloat(-10.0f, 10.0f);
    //                 spawnPos.z = rng.NextFloat(-10.0f, 10.0f);
    //                 ecb.SetComponent(enemy, new LocalTransform { Position = spawnPos, Rotation = quaternion.identity, Scale = 1f });
    //
    //                 ecb.AddComponent(enemy, new EnemyColorComponent()
    //                 {
    //                     EntityID    = enemy.Index,
    //                     RandomColor = rng.NextFloat(0f, 10.0f)
    //                 });
    //                 Debug.LogError((int)seed);
    //                 ecb.AddComponent(enemy, new EnemyComponent()
    //                 {
    //                     EntityID = (int)seed,
    //                     Health   = 100,
    //                     Speed    = 5
    //                 });
    //
    //                 ecb.AddComponent(enemy, new URPMaterialPropertyBaseColor() { Value = new float4(1f, 1f, 1f, 1f) });
    //                 ecb.AddComponent(enemy, new RequestChangePosition());
    //                 ecb.SetComponentEnabled<RequestChangePosition>(enemy, true);
    //
    //                 ecb.AddComponent(enemy, new EnemyComponent()
    //                 {
    //                     EntityID = enemy.Index,
    //                     Health   = 100,
    //                     Speed    = 5
    //                 });
    //
    //                 spawner.ValueRW.CurrentSpawnCount += 1;
    //                 Debug.LogError("Spawn Count: " + spawner.ValueRW.CurrentSpawnCount + "_" + spawner.ValueRW.SpawnCount);
    //             }
    //         }
    //     }
    // }
    [BurstCompile]
    public partial struct EnemySpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            uint seed = (uint)(Time.realtimeSinceStartup * 1000) + 1;

            new EnemySpawnJob
            {
                ECB  = ecb,
                Seed = seed
            }.ScheduleParallel();
        }
    }
    
    [BurstCompile]
    public partial struct EnemySpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public uint                               Seed;

        private void Execute([ChunkIndexInQuery] int chunkIndex, ref EnemySpawnerComponent spawner)
        {
            if (spawner.CurrentSpawnCount <= spawner.SpawnCount)
            {
                int                 countToSpawn = math.min(spawner.SpawnCount - spawner.CurrentSpawnCount, 10000); // Spawn tối đa 10k
                NativeArray<Entity> enemies      = new NativeArray<Entity>(countToSpawn, Allocator.Temp);

                ECB.Instantiate(chunkIndex, spawner.EnemyPrefab, enemies);

                Random rng = new Random(Seed + (uint)chunkIndex);

                for (int i = 0; i < countToSpawn; i++)
                {
                    float3 spawnPos = new float3(rng.NextFloat(-40.0f, 40.0f), 0, rng.NextFloat(-40.0f, 40.0f));

                    ECB.SetComponent(chunkIndex, enemies[i], new LocalTransform
                    {
                        Position = spawnPos,
                        Rotation = quaternion.identity,
                        Scale    = 1f
                    });

                    ECB.AddComponent(chunkIndex, enemies[i], new EnemyComponent
                    {
                        EntityID = (int)(Seed + chunkIndex + i),
                        Health   = 100,
                        Speed    = 5
                    });

                    ECB.AddComponent(chunkIndex, enemies[i], new EnemyColorComponent()
                    {
                        EntityID    = enemies[i].Index,
                        RandomColor = rng.NextFloat(0f, 10.0f)
                    });
                    
                    this.ECB.AddComponent(chunkIndex,  enemies[i], new URPMaterialPropertyBaseColor());
                }

                spawner.CurrentSpawnCount += countToSpawn;
                enemies.Dispose();
            }
        }
    }

    [BurstCompile]
    public partial struct ChangeColorSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            new ChangeColorJob()
            {
                time = (float)SystemAPI.Time.ElapsedTime
            }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct ChangeColorJob : IJobEntity
        {
            public float time;

            void Execute(ref EnemyColorComponent enemyColorComponent, ref URPMaterialPropertyBaseColor baseColor)
            {
                float  t        = math.sin(time + enemyColorComponent.EntityID * enemyColorComponent.RandomColor) * 0.5f + 0.5f;
                float4 newColor = new float4(t, t, 1.0f - t, 1.0f);

                enemyColorComponent.Color = newColor;
                baseColor.Value           = newColor; // Gán màu vào material
            }
        }
    }
}