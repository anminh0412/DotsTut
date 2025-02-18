namespace Test
{
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public struct EnemyComponent : IComponentData
    {
        public int   EntityID;
        public float Health;
        public float Speed;
    }
    public struct EnemySpawnerComponent : IComponentData
    {
        public Entity EnemyPrefab;
        public int    SpawnCount;
        public int    CurrentSpawnCount;
    }

    public struct EnemyColorComponent : IComponentData
    {
        public float4 Color;
        public int    EntityID;
        public float  RandomColor;
    }
    public struct BulletComponent : IComponentData
    {
        public float Damage;
    }

    public struct PlayerComponent : IComponentData
    {
        
    }
    
    public struct PlayerSpawnerComponent : IComponentData
    {
        public Entity PlayerPrefab;
    }

    public struct PlayerCreatedTag : IComponentData
    {
        
    }

    public struct RequestChangePosition : IComponentData, IEnableableComponent
    {
        
    }

    
    public struct MoveTargetComponent : IComponentData
    {
        public float3 Position;
    }

}