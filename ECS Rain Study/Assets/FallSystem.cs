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



public class FallSystem : SystemBase  // JobComponentSystem
{
    private EntityQuery m_Query;
    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<FallComponent>());
    }

    public struct FallJob : IJobEntityBatch
    {
        public float DeltaTime;
        public ComponentTypeHandle<Translation> translationHandle;
        [ReadOnly] public ComponentTypeHandle<FallComponent> fallCompHandle;

        [BurstCompile(CompileSynchronously = true)]
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            NativeArray<Translation> translations = batchInChunk.GetNativeArray(translationHandle);
            NativeArray<FallComponent> fallComponents = batchInChunk.GetNativeArray(fallCompHandle);

            for (int i = 0; i < batchInChunk.Count; i++)
            {
                float3 translation = translations[i].Value;
                float fallComp = fallComponents[i].Value;

                translation.y -= fallComp * DeltaTime;

                if (translation.y < 0) { translation.y = 30f; }

                translations[i] = new Translation { Value = translation };
            }
        }
    }

    protected override void OnUpdate()
    {
        // Instantiate the job struct
        var fallJob = new FallJob();

        // Set the job component type handles
        // "this" is your SystemBase subclass
        fallJob.translationHandle = GetComponentTypeHandle<Translation>(false);
        fallJob.fallCompHandle = GetComponentTypeHandle<FallComponent>(true);

        // Set other data need in job, such as time
        fallJob.DeltaTime = World.Time.DeltaTime;

        // Schedule the job
        Dependency = fallJob.ScheduleParallel(m_Query, 5, Dependency);
    }
}


    //THiS USES IJOBCHUNK
    /*
    private EntityQuery m_Query;
    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<FallComponent>());
    }


    struct FallJob : IJobChunk
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
                var translation = chunkTranslations[i];
                var fallComp = chunkFallComponents[i];

                chunkTranslations[i] = new Translation
                {
                    Value = new float3(
                        translation.Value.x,
                        translation.Value.y - fallComp.Value * DeltaTime,
                        translation.Value.z)
                };

                if (translation.Value.y < 0)
                { 
                    chunkTranslations[i] = new Translation
                    {
                        Value = new float3(
                            translation.Value.x,
                            30f,
                            translation.Value.z)
                    };
                }
            }

            chunkTranslations.Dispose();
            chunkFallComponents.Dispose();
        }
    }
}

    protected override void OnUpdate()
    {
        var job = new FallJob()
        {
            translationHandle = GetComponentTypeHandle<Translation>(false),
            componentTypeHandle = GetComponentTypeHandle<FallComponent>(true),
            DeltaTime = Time.DeltaTime
        };
        this.Dependency = job.ScheduleParallel(m_Query, this.Dependency);
    }
    */


    //Using Deprecated IJobForEach
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
    
}
    */