using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Collections;

public class RainCreator : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    [SerializeField] private int num = 5;
    [SerializeField] private float rotation = 0;
    [SerializeField] private float scale = 1f;
    // Start is called before the first frame update
    void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(FallComponent)
            );

        NativeArray<Entity> entityArray = new NativeArray<Entity>(num, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        foreach(Entity entity in entityArray)
        {
            entityManager.SetComponentData(entity, new Translation
            {
                Value = new float3(
                UnityEngine.Random.Range(-80f, 80f),
                UnityEngine.Random.Range(10f, 30f),
                UnityEngine.Random.Range(-50f, 50f))
                //1f, 1f, 1f)
            });
            entityManager.SetComponentData(entity, new Rotation
            {
                Value = quaternion.EulerXYZ(0, rotation * Mathf.Deg2Rad, 0)
            });
            entityManager.SetComponentData(entity, new Scale
            {
                Value = scale
            });
            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                material = material,
            });
            entityManager.SetComponentData(entity, new FallComponent
            {
                Value = UnityEngine.Random.Range(1f, 4f)
            });
        }

        entityArray.Dispose();
    }
        
}

