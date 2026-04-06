using Photon.Deterministic;
using Quantum;
using System.Collections.Generic;

public unsafe class CharacterMasterSystem : SystemMainThreadFilter<CharacterMasterSystem.Filter>, ISignalOnPlayerAdded
{
    public struct Filter
    {
        public EntityRef Entity;
        public CharacterMaster* Master;
        public AnimatorComponent* Animator;
        public MovementStateMachine* MovementSM;
        public Transform2D* Transform;
        public KCC2D* KCC;
        public PlayerLink* PlayerLink;
    }

    public override void Update(Frame frame, ref Filter filter)
    {
        // 0. SET INPUT
        QuantumDemoInputPlatformer2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);
        var config = frame.FindAsset(filter.KCC->Config);
        if (frame.TryGet<PlayerLink>(filter.Entity, out var link))
        {
            filter.Master->Input = input;
        }

        // 1. COLLECT all non-empty requests
        var activeRequests = new List<(StateType state, int priority, EntityRef requester)>();

        // Check Movement request
        if (filter.Master->MovementRequest.RequestedState != StateType.NONE)
        {
            activeRequests.Add((
                filter.Master->MovementRequest.RequestedState,
                filter.Master->MovementRequest.Priority,
                filter.Master->MovementRequest.Requester
            ));
        }

        // 2. FIND highest priority winner
        StateType winningState = StateType.NONE;
        if (activeRequests.Count > 0)
        {
            // Sort by priority descending, take first
            activeRequests.Sort((a, b) => b.priority.CompareTo(a.priority));
            winningState = activeRequests[0].state;
        }

        // 3. SWITCH state if changed
        if (winningState != StateType.NONE && winningState != filter.Master->CurrentState)
        {
            SwitchState(frame, ref filter, winningState);
        }
        // If no requests, default to IDLE
        else if (winningState == StateType.NONE && filter.Master->CurrentState != StateType.IDLE)
        {
            SwitchState(frame, ref filter, StateType.IDLE);
        }

        // 4. UPDATE current state
        if (filter.Master->CurrentState != StateType.NONE)
        {
            var currentConfig = GetConfigForState(frame, filter.Master->CurrentState, filter.Master);
            if (currentConfig != null)
            {
                filter.Master->StateTimer += frame.DeltaTime;
                currentConfig.UpdateState(frame, filter.Master, filter.KCC, filter.Animator);
            }
        }

        // 5. KCC APPLIES physics
        var kccConfig = frame.FindAsset<KCC2DConfig>(filter.KCC->Config.Id);
        if (kccConfig != null)
        {
            kccConfig.Move(frame, filter.Entity, filter.Transform, filter.KCC);
        }

        // 6. CLEAR all requests for next frame
        ClearRequests(ref filter);
    }

    private StateConfig GetConfigForState(Frame frame, StateType state, CharacterMaster* master)
    {
        var configRef = GetConfigRefForState(state, master);
        return frame.FindAsset<StateConfig>(configRef.Id);
    }

    private AssetRef<StateConfig> GetConfigRefForState(StateType state, CharacterMaster* master)
    {
        switch (state)
        {
            case StateType.IDLE: return master->IdleConfig;
            case StateType.RUN: return master->RunConfig;
            case StateType.JUMP: return master->JumpConfig;
            case StateType.MID_AIR: return master->MidAirConfig;
            default: return master->IdleConfig;
        }
    }

    private void SwitchState(Frame frame, ref Filter filter, StateType newState)
    {
        // ExitState current
        if (filter.Master->CurrentState != StateType.NONE)
        {
            var oldConfig = GetConfigForState(frame, filter.Master->CurrentState, filter.Master);
            oldConfig?.ExitState(frame, filter.Master, filter.KCC, filter.Animator);
        }

        // Update master
        filter.Master->CurrentState = newState;
        filter.Master->StateTimer = FP._0;

        // Update config reference
        var newConfigRef = GetConfigRefForState(newState, filter.Master);
        filter.Master->CurrentStateConfig = newConfigRef;

        // EnterState new
        var newConfig = frame.FindAsset<StateConfig>(newConfigRef.Id);
        newConfig?.EnterState(frame, filter.Master, filter.KCC, filter.Animator);

        // Debug log
        //Log.Debug($"[CharacterMaster] Switched to {newState}");
    }

    private void ClearRequests(ref Filter filter)
    {
        // Reset Movement request
        filter.Master->MovementRequest.RequestedState = StateType.NONE;
        filter.Master->MovementRequest.Priority = 0;
        filter.Master->MovementRequest.Requester = default;

        // Reset Attack request (for Part 3)
        //filter.Master->AttackRequest.RequestedState = StateType.NONE;
        //filter.Master->AttackRequest.Priority = 0;
        //filter.Master->AttackRequest.Requester = default;
    }

    public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
    {
        var playerData = f.GetPlayerData(player);
        var playerEntity = f.Create(playerData.PlayerAvatar);
        PlayerLink* playerLink = f.Unsafe.GetPointer<PlayerLink>(playerEntity);
        playerLink->Player = player;
    }
}