namespace Test
{
    using Unity.Entities;
    using UnityEngine;

    public class PlayerAuthoring : MonoBehaviour
    {
        public GameObject playerPrefab;
        class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Debug.Log($"🚀 Bake: {authoring.gameObject.name}");
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
                AddComponent(entity, new PlayerSpawnerComponent
                {
                    PlayerPrefab = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic)
                });
                Debug.Log($"🚀 Bake Done: {authoring.gameObject.name}");
            }
        }
    }
}