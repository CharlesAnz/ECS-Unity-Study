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

public class FallSystem : JobComponentSystem   // JobComponentSystem //ComponentSystem
{
    
    private EntityQuery m_Query;
    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<FallComponent>());
    }
    
    
    
    //Original ECS Code
    /*
    protected override void OnUpdate()
    {
        var time = Time.DeltaTime;

        Entities.ForEach((ref Translation translation, ref FallComponent moveComponent) =>
        {
            translation.Value.y -= moveComponent.Value * time;
            if (translation.Value.y < 0) translation.Value.y = 30f;
        });
    }
    */
    
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps.Complete();
        /*
        FallJobEntityBatch fallBatchJob = new FallJobEntityBatch()
        {
            translationHandle = GetComponentTypeHandle<Translation>(false),
            fallCompHandle = GetComponentTypeHandle<FallComponent>(true),

            DeltaTime = World.Time.DeltaTime
        };
        JobHandle handle = fallBatchJob.ScheduleParallel(m_Query, 10, inputDeps);
        */

        FallJobChunk jobChunk = new FallJobChunk()
        {
            translationHandle = GetComponentTypeHandle<Translation>(false),
            componentTypeHandle = GetComponentTypeHandle<FallComponent>(true),
            DeltaTime = Time.DeltaTime
        };
        JobHandle handle = jobChunk.ScheduleParallel(m_Query, inputDeps);
        

        /*
        FallJobForEach fallJobForEach = new FallJobForEach()
        {
            deltaTime = Time.DeltaTime
        };
        JobHandle handle = fallJobForEach.Schedule(this);
        */

        m_Query.CompleteDependency();

        return handle;
    }
    
    //Using IJobForEach
    struct FallJobForEach : IJobForEach<Translation, FallComponent>
    {
        public float deltaTime;

        [BurstCompile]
        public void Execute(ref Translation trans, ref FallComponent fallComp)
        {
            trans.Value.y -= fallComp.Value * deltaTime;
            if (trans.Value.y < 0) trans.Value.y = 30f;
        }
    }

    //using IJobChunk
    struct FallJobChunk : IJobChunk
    {
        public float DeltaTime;
        public ComponentTypeHandle<Translation> translationHandle;
        [ReadOnly] public ComponentTypeHandle<FallComponent> componentTypeHandle;

        [BurstCompile]
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

                chunkTranslations[i] = new Translation
                {
                    Value = new float3(
                    chunkTranslations[i].Value.x,
                    translationY,
                    chunkTranslations[i].Value.z)
                };
            }

            chunkTranslations.Dispose();
            chunkFallComponents.Dispose();
        }
    }

    //using IJobEntityBatch
    public struct FallJobEntityBatch : IJobEntityBatch
    {
        public float DeltaTime;
        public ComponentTypeHandle<Translation> translationHandle;
        [ReadOnly] public ComponentTypeHandle<FallComponent> fallCompHandle;

        [BurstCompile]
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
}