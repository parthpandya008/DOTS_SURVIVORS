using Survivors.UI;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;


namespace Survivors.Game
{
    //Run this system at late inside the SimulationSystemGroup. We need to check,
    //if entity needs to be destroyed, spawing (gems) or showing Game over UI
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    
    //The EndSimulationEntityCommandBufferSystem is the moment when all buffered structural changes for this frame are finally executed.
    //As we are destroying the entites, the system should run beofre ECB playback.
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct DestroyEntitysystem : ISystem
    {
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //A system that buffers structural changes (ex: destroy) and plays them back at the end
            //It batches all entity changes and applies them only once per frame.
            var endECBSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var endECB = endECBSystem.CreateCommandBuffer(state.WorldUnmanaged);

            //A system that plays back its ECB at the very start of the next frame
            //If you use EndSimulation ECB to destroy an entity and want to spawn something immediately at the start of the next frame
            //Begin ECB for gem spawing
            var beginECBSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var beginECB = beginECBSystem.CreateCommandBuffer(state.WorldUnmanaged);
            
            /*
             foreach (var (_, entity) in SystemAPI.Query<DestroyEntityFlag>().WithEntityAccess())
             {
                 //Show Game Over UI on player destroyed
                 if (SystemAPI.HasComponent<PlayerTag>(entity))
                 {
                     GameUIController.Instance.ShowGameOverUI();
                 }

                 //Spawn gem on enemy destroyed
                 if (SystemAPI.HasComponent<GemPrefab>(entity))
                 {
                     //Instantiate GemPrefab
                     var gemPrefeb = SystemAPI.GetComponent<GemPrefab>(entity).Value;
                     var newGem = beginECB.Instantiate(gemPrefeb);

                     //Set enemy position to newly spawnGem
                     var spawnPosition = SystemAPI.GetComponent<LocalTransform>(entity).Position;
                     beginECB.SetComponent(newGem, LocalTransform.FromPosition(spawnPosition));
                 }

                 endECB.DestroyEntity(entity);
             } 
            */
            
            //  Main thread: Player only (managed UI call) 
            foreach (var (_, entity) in SystemAPI.Query<DestroyEntityFlag>()
                                    .WithAll<PlayerTag>().WithEntityAccess())
            {
                GameUIController.Instance.ShowGameOverUI();
                endECB.DestroyEntity(entity);
            }


            /* We split this into two jobs 
               Reason 1: To decouple spawning logic from cleanup logic.
               Reason 2: For a single job we need to use HasComponent, inside an IJobEntity forces random memory access, 
                         which destroys the,sequential chunk iteration that makes Burst so fast.
             */
            var dropGemJob = new DropGemJob
            {
                BeginECB = beginECB.AsParallelWriter()
            };

            state.Dependency = dropGemJob.ScheduleParallel(state.Dependency);

            var destroyJob = new DestroyEntityJob
            {
                EndECB = endECB.AsParallelWriter(),
            };
            state.Dependency = destroyJob.ScheduleParallel(state.Dependency);
        }
    }

    /*NOTE: To maintain determinism, ParallelWriter ECBs must sort their commands by chunk index on the main thread. 
     *      This sorting adds overhead when destroying only few entities per frame.
     *      But following structure would be still good to handle massive entities destruction (e.g., AoE attacks)
     */

    //This JOB ONLY handles spawning gems for enemy
    [WithAll(typeof(EnemyTag), typeof(DestroyEntityFlag))]
    [BurstCompile]
    public partial struct DropGemJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter BeginECB;
        public void Execute([ChunkIndexInQuery] int chunkIndex, in GemPrefab gemPrefab, in LocalTransform enemyTransform)
        {
            var newGem = BeginECB.Instantiate(chunkIndex, gemPrefab.Value);
            BeginECB.SetComponent(chunkIndex, newGem, LocalTransform.FromPosition(enemyTransform.Position));
        }
    }

    // This JOB to destroy Prop, Flash, or Enemy, etc.
    [WithAll(typeof(DestroyEntityFlag))]
    [WithNone(typeof(PlayerTag))]
    [BurstCompile]
    public partial struct DestroyEntityJob: IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter EndECB;

        // [ChunkIndexInQuery] gets the memory block ID for the current batch of entities.
        // We MUST pass this into our ParallelWriter ECBs (BeginECB/EndECB) as the first parameter.
        // This acts as a 'sortKey' so that when the main thread eventually plays back these commands, 
        // it can organize them deterministically, preventing race conditions and physics glitches.
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity)
        {
            EndECB.DestroyEntity(chunkIndex, entity);
        }
    }
}
