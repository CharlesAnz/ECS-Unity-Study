using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;



public class FallSystem : SystemBase   // JobComponentSystem //SystemBase //ComponentSystem
{

    //Jarek's original ECS Code
    /*
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, ref FallComponent moveComponent) =>
        {
            translation.Value.y -= moveComponent.Value * Time.DeltaTime;
            if (translation.Value.y < 0) translation.Value.y = 30f;
        });
    }
    */


    private EntityQuery m_Query;
    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<FallComponent>());
    }

    //using IJobEntityBatch
    public struct FallJobEntityBatch : IJobEntityBatch
    {
        public float DeltaTime;
        public ComponentTypeHandle<Translation> translationHandle;
        [ReadOnly] public ComponentTypeHandle<FallComponent> fallCompHandle;

        [BurstCompile(CompileSynchronously = true)]
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var translations = batchInChunk.GetNativeArray(translationHandle);
            var fallComponents = batchInChunk.GetNativeArray(fallCompHandle);

            for (var i = 0; i < batchInChunk.Count; i++)
            {
                var translation = translations[i].Value;
                var fallComp = fallComponents[i].Value;

                translation.y -= fallComp * DeltaTime;

                if (translation.y < 0) { translation.y = 30f; }

                translations[i] = new Translation { Value = translation };
            }

            translations.Dispose();
            fallComponents.Dispose();
        }
    }

    //THiS USES IJOBCHUNK
    struct FallJobChunk : IJobChunk
    {
        public float DeltaTime;
        public ComponentTypeHandle<Translation> translationHandle;
        [ReadOnly] public ComponentTypeHandle<FallComponent> componentTypeHandle;

        [BurstCompile(CompileSynchronously = true)]
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(translationHandle);
            var chunkFallComponents = chunk.GetNativeArray(componentTypeHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var translationY = chunkTranslations[i].Value.y;

                translationY -= chunkFallComponents[i].Value * DeltaTime;

                if (translationY < 0)
                {
                    translationY = 30f;
                }

                chunkTranslations[i] = new Translation { Value = new float3(
                    chunkTranslations[i].Value.x,
                    translationY,
                    chunkTranslations[i].Value.z) };
            }

            chunkTranslations.Dispose();
            chunkFallComponents.Dispose();

        }
    }

    protected override void OnUpdate()
    {
        Dependency.Complete();

        /*
        var fallJob = new FallJobEntityBatch()
        {
            translationHandle = GetComponentTypeHandle<Translation>(false),
            fallCompHandle = GetComponentTypeHandle<FallComponent>(true),

            DeltaTime = World.Time.DeltaTime
        };
        
        Dependency = fallJob.ScheduleParallel(m_Query, 8, Dependency);
        */

        var jobChunk = new FallJobChunk()
        {
            translationHandle = GetComponentTypeHandle<Translation>(false),
            componentTypeHandle = GetComponentTypeHandle<FallComponent>(true),
            DeltaTime = Time.DeltaTime
        };
        Dependency = jobChunk.ScheduleParallel(m_Query, Dependency);
        
    }

    
    //Using IJobForEach
    /*
    struct fallJobECS : IJobForEach<Translation, FallComponent>
    {
        public float deltaTime;

        [BurstCompile]
        public void Execute(ref Translation c0, ref FallComponent c1)
        {
            c0.Value.y -= c1.Value * deltaTime;
            if (c0.Value.y < 0) c0.Value.y = 10f;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        fallJobECS fallJob = new fallJobECS
        {
            deltaTime = Time.DeltaTime
        };

        JobHandle handle = fallJob.Schedule(this);

        return handle;
    }
    */
}