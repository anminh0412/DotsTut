namespace Test
{
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [BurstCompile]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct MoveSystem : ISystem
    {
        float                    calTimeToMove;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyComponent>();
            state.RequireForUpdate<LocalTransform>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if(!Input.GetKey(KeyCode.K)) return;
            float deltaTime = SystemAPI.Time.DeltaTime;

            if (this.calTimeToMove < 0.15f)
            {
                this.calTimeToMove += deltaTime;
            }
            else
            {
                this.calTimeToMove = 0;
                
                new MoveEnemyJob()
                {
                }.ScheduleParallel();
            }
        }

        [BurstCompile]
        public partial struct MoveEnemyJob : IJobEntity
        {

            [BurstCompile]
            public void Execute(ref LocalTransform transform, in EnemyComponent enemy)
            {
                float3 randomDirection =  math.normalize(new float3(
                        1, 
                        0f, 
                       0
                    ));

                float3 newPosition = transform.Position + randomDirection;
                transform.Position = newPosition; // Giờ sẽ thực sự cập nhật entity
            }
        }
    } 
    
    [BurstCompile]
    public partial struct RandomMoveEnemySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyComponent>();
            state.RequireForUpdate<LocalTransform>();
        }
        public void OnUpdate(ref SystemState state)
        {
            Unity.Mathematics.Random rng         
                = new Unity.Mathematics.Random((uint)SystemAPI.Time.ElapsedTime+1);
            new RandomSpeedJob()
            {
                seed = (uint)(Time.realtimeSinceStartup * 1000) + 1
            }.ScheduleParallel();
        }
        
        [BurstCompile]
        public partial struct RandomSpeedJob : IJobEntity
        {
            public uint seed;
            void Execute(ref EnemyComponent enemyComponent)
            {
                //uint                     newSeed = (uint)(Time.realtimeSinceStartup * 1000) + 1;
                //uint                     newSeed = (uint)(seed * 747796405u + enemyComponent.EntityID * 2891336453u);
                Unity.Mathematics.Random rng     = new Unity.Mathematics.Random( seed);
                enemyComponent.Speed = rng.NextInt(0, 2) * 5;
                //Debug.LogError($"Random {enemyComponent.EntityID} Speed " +  enemyComponent.Speed);
            }
        }
    }
    
    // [BurstCompile]
    // public partial struct MoveOnClickSystem : ISystem
    // {
    //     public void OnCreate(ref SystemState state)
    //     {
    //         state.RequireForUpdate<EnemyComponent>();
    //         state.RequireForUpdate<LocalTransform>();
    //         state.RequireForUpdate<MoveTargetComponent>();
    //     }
    //     public void OnUpdate(ref SystemState state)
    //     {
    //        
    //         new MoveOnClickJob()
    //         {
    //         }.ScheduleParallel();
    //     }
    //     
    //     [BurstCompile]
    //     public partial struct MoveOnClickJob : IJobEntity
    //     {
    //         public float3 TargetPosition;
    //         public float  DeltaTime;
    //
    //         public void Execute(ref LocalTransform transform, ref EnemyComponent enemy)
    //         {
    //             float3 direction = math.normalize(TargetPosition - transform.Position);
    //             transform.Position += direction * enemy.Speed * DeltaTime; // Di chuyển theo hướng click chuột
    //         }
    //     }
    // }
    // [BurstCompile]
    // public partial struct ClickDetectionSystem : ISystem
    // {
    //     public void OnCreate(ref SystemState state)
    //     {
    //         state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    //     }
    //
    //     public void OnUpdate(ref SystemState state)
    //     {
    //         if (!Input.GetMouseButtonDown(0)) return; // Chỉ xử lý khi click chuột trái
    //
    //         var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
    //         var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
    //
    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //         if (Physics.Raycast(ray, out RaycastHit hit))
    //         {
    //             float3 clickPosition = hit.point;
    //
    //             // Kiểm tra xem entity có MoveTargetComponent đã tồn tại chưa
    //             bool found = false;
    //             foreach (var (target, entity) in SystemAPI.Query<RefRW<MoveTargetComponent>>().WithEntityAccess())
    //             {
    //                 target.ValueRW.Position = clickPosition;
    //                 found                   = true;
    //                 break;
    //             }
    //
    //             // Nếu không tìm thấy, tạo mới một entity chứa MoveTargetComponent
    //             if (!found)
    //             {
    //                 Entity targetEntity = ecb.CreateEntity();
    //                 ecb.AddComponent(targetEntity, new MoveTargetComponent { Position = clickPosition });
    //             }
    //         }
    //     }
    // }
}