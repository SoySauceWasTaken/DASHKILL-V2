using Photon.Deterministic;
using Quantum;
using System.Collections.Generic;

public unsafe class CharacterMasterSystem : SystemMainThreadFilter<CharacterMasterSystem.Filter>, ISignalOnComponentAdded<CharacterMaster>, ISignalOnComponentRemoved<CharacterMaster>
{
    public struct Filter
    {
        public EntityRef Entity;
        public CharacterMaster* Master;
        public AnimatorComponent* Animator;
        public Transform2D* Transform;
        public KCC2D* KCC;
        public MovementData* MovementData;
    }

    public override void Update(Frame frame, ref Filter filter)
    {
        // 0. SET INPUT
        if (frame.TryGet<PlayerLink>(filter.Entity, out var link))
        {
            QuantumDemoInputPlatformer2D input = *frame.GetPlayerInput(link.Player);
            filter.Master->Input = input;
        }

        // 1. GET the list of StateRequests
        var requests = frame.ResolveList(filter.Master->StateRequests);

        // 2. FIND highest priority request
        StateType winningState = StateType.NONE;
        int winningPriority = 0;
        EntityRef winningRequester = EntityRef.None;

        for (int i = 0; i < requests.Count; i++)
        {
            var request = requests[i];
            if (request.RequestedState != StateType.NONE && request.Priority > winningPriority)
            {
                winningPriority = request.Priority;
                winningState = request.RequestedState;
                winningRequester = request.Requester;
            }
        }

        // 3. LOG active requests for debugging
        if (requests.Count > 0)
        {
            //Log.Debug($"{filter.Entity} - [CharacterMaster] Processing {requests.Count} requests. Winner: {winningState} (priority {winningPriority})");
        }

        // 4. Create a StateFilter real quick (Is there a better place to put this?)
        StateFilter stateFilter = StateFilterUtils.CreateStateFilter(frame, filter.Entity);


        // 5. SWITCH state if needed AND winning priority >= current state priority
        if (winningState != StateType.NONE &&
            winningState != filter.Master->CurrentState &&
            winningPriority >= filter.Master->CurrentStatePriority)
        {
            filter.Master->CurrentStatePriority = winningPriority;
            SwitchState(frame, ref filter, winningState, &stateFilter);
        }

        // 6. UPDATE current state
        if (filter.Master->CurrentState != StateType.NONE)
        {
            var currentConfig = StateMachineUtils.GetConfigForState(frame, filter.Master->CurrentState, filter.Master);
            if (currentConfig != null)
            {
                filter.Master->StateTimer += frame.DeltaTime;
                currentConfig.UpdateState(frame, filter.Entity, &stateFilter);

                // Check if we should exit the current state
                if (currentConfig.CanExit(frame, filter.Entity, &stateFilter))
                {
                    filter.Master->CurrentStatePriority = 0;
                    //Log.Debug($"[CharacterMaster] State {filter.Master->CurrentState} requested exit");
                }
            }
        }

        // 7. KCC APPLIES physics
        var kccConfig = frame.FindAsset<KCC2DConfig>(filter.KCC->Config.Id);
        if (kccConfig != null)
        {
            kccConfig.Move(frame, filter.Entity, filter.Transform, filter.KCC);
        }

        // 8. CLEAR all requests for next frame
        requests.Clear();
    }

    private void SwitchState(Frame frame, ref Filter filter, StateType newState, StateFilter* stateFilter)
    {
        // ExitState current
        if (filter.Master->CurrentState != StateType.NONE)
        {
            var oldConfig = StateMachineUtils.GetConfigForState(frame, filter.Master->CurrentState, filter.Master);
            oldConfig?.ExitState(frame, filter.Entity, stateFilter);
        }

        // Update master
        filter.Master->CurrentState = newState;
        filter.Master->StateTimer = FP._0;

        // Update config reference
        var newConfigRef = StateMachineUtils.GetConfigRefForState(newState, filter.Master);
        filter.Master->CurrentStateConfig = newConfigRef;

        // EnterState new
        var newConfig = frame.FindAsset<StateConfig>(newConfigRef.Id);
        newConfig?.EnterState(frame, filter.Entity, stateFilter);

        // Debug log
        //Log.Debug($"[CharacterMaster] Switched to {newState}");
    }

    public void OnAdded(Frame frame, EntityRef entity, CharacterMaster* component)
    {
        // Allocate the list for StateRequests
        component->StateRequests = frame.AllocateList<StateRequest>();
    }

    public void OnRemoved(Frame frame, EntityRef entity, CharacterMaster* component)
    {
        // Free the list to prevent memory leaks
        frame.FreeList(component->StateRequests);
        component->StateRequests = default;
    }
}