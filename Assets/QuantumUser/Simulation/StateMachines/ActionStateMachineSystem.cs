using Photon.Deterministic;

namespace Quantum
{
    public unsafe class ActionStateMachineSystem : SystemMainThreadFilter<ActionStateMachineSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public CharacterMaster* Master;
            public KCC2D* KCC;
            public ActionStateMachine* ActionSM;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Update cooldowns FIRST
            UpdateCooldowns(frame, ref filter);

            // 1. Get input
            var input = filter.Master->Input;

            // 2. Determine what state we WANT based on physics state + input
            StateType desiredState = DetermineDesiredState(frame, ref filter, input);

            // 3. Submit request to CharacterMaster's dynamic list
            if (desiredState != StateType.NONE)
            {
                SubmitRequest(frame, filter.Master, desiredState, (int)StateMachinePriority.Action, filter.Entity);
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

            //Log.Debug($"[ActionSM] Submitted request: {desiredState} with priority {priority} for entity {requester}");
        }

        private StateType DetermineDesiredState(Frame frame, ref Filter filter, QuantumDemoInputPlatformer2D input)
        {
            if (input.Dodge.WasPressed && input.Direction.X == FP._0 && CanSpotDodge(frame, ref filter))
            {
                //Log.Debug($"[Action's DetermineDesiredState] CanSpotDodge: {CanSpotDodge(frame, ref filter)}");
                return StateType.SPOTDODGE;
            }

            if (input.Dodge.WasPressed && input.Direction.X != FP._0 && StateMachineUtils.CanTransitionTo(frame, StateType.DASH, filter.Master))
            {
                return StateType.DASH;
            }

            // ATTACK next priority
            if (input.HeavyAttack.WasPressed && StateMachineUtils.CanTransitionTo(frame, StateType.ATTACK, filter.Master))
            {
                //Log.Debug($"Attack requested - Light Attack pressed");
                return StateType.ATTACK;
            }

            return StateType.NONE;
        }

        private void UpdateCooldowns(Frame frame, ref Filter filter)
        {
            // Reduce dodge cooldown
            if (filter.ActionSM->SpotDodgeCooldown > FP._0)
            {
                filter.ActionSM->SpotDodgeCooldown -= frame.DeltaTime;
                if (filter.ActionSM->SpotDodgeCooldown < FP._0)
                    filter.ActionSM->SpotDodgeCooldown = FP._0;
            }
        }

        private bool CanSpotDodge(Frame frame, ref Filter filter)
        {
            // Check cooldown
            //Log.Debug($"[CanSpotDodge]: {filter.ActionSM->SpotDodgeCooldown}");
            if (filter.ActionSM->SpotDodgeCooldown > FP._0) return false;

            return true;
        }
    }
}