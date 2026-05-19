using Photon.Deterministic;

namespace Quantum
{
    public unsafe class StatusStateMachineSystem : SystemMainThreadFilter<StatusStateMachineSystem.Filter>, ISignalOnDamageDealt
    {
        public struct Filter
        {
            public EntityRef Entity;
            public CharacterMaster* Master;
            public KCC2D* KCC;
            public StatusStateMachine* Status;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // 1. Get input
            var input = filter.Master->Input;

            // 2. Determine what state we WANT based on physics state + input
            StateType desiredState = DetermineDesiredState(frame, ref filter, input);

            // 3. Submit request to CharacterMaster
            if (desiredState != StateType.NONE)
            {
                SubmitRequest(frame, filter.Master, desiredState, (int)StateMachinePriority.Status, filter.Entity);
            }
        }

        private void SubmitRequest(Frame frame, CharacterMaster* master, StateType desiredState, int priority, EntityRef requester)
        {
            // Resolve the list from the master
            var requests = frame.ResolveList(master->StateRequests);

            // Create and add the new request
            StateRequest newRequest = new StateRequest
            {
                RequestedState = desiredState,
                Priority = priority,
                Requester = requester
            };
            requests.Add(newRequest);

            Log.Debug($"[ActionSM] Submitted request: {desiredState} with priority {priority} for entity {requester}");
        }


        private StateType DetermineDesiredState(Frame frame, ref Filter filter, QuantumDemoInputPlatformer2D input)
        {
            if (filter.Status->IsStunned)
            {
                Log.Debug("[DetermineDesiredState] to STUN!");
                return StateType.STUN;
            }

            return StateType.NONE;
        }

        public void OnDamageDealt(Frame f, EntityRef target, AssetRef<HitBoxConfig> HitBoxConfig, EntityRef source)
        {
            // Only apply knockback and stun to the target (not the source)
            if (target == source) return;

            // Set up stun state on the target
            if (f.Unsafe.TryGetPointer(target, out StatusStateMachine* status))
            {
                status->HitBoxConfig = HitBoxConfig;
                status->Attacker = source;
                Log.Debug("OnDamageDealt CALLBACK..., Setting IsStunned to true");
                status->IsStunned = true;
            }
        }
    }
}