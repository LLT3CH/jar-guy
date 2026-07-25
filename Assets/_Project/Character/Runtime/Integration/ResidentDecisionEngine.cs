using HumanGlassWatcher.Character.Model;
using HumanGlassWatcher.Character.Planning;

namespace HumanGlassWatcher.Character.Integration
{
    /// <summary>
    /// Produces requests only. The gameplay port is responsible for revalidating and executing.
    /// </summary>
    public sealed class ResidentDecisionEngine
    {
        private readonly UtilityActionPlanner planner;
        private readonly DialogueIntentGate intentGate;

        public ResidentDecisionEngine(
            UtilityActionPlanner planner = null,
            DialogueIntentGate intentGate = null)
        {
            this.planner = planner ?? new UtilityActionPlanner();
            this.intentGate = intentGate ?? new DialogueIntentGate();
        }

        public ActionRequest SelectLocal(
            ResidentState resident,
            CharacterPerceptionSnapshot perception)
        {
            return planner.Select(resident, perception.LegalOffers, perception.Items);
        }

        public IntentValidationResult SelectServiceIntent(
            BrainTurn turn,
            CharacterPerceptionSnapshot perception)
        {
            return intentGate.Validate(turn, perception.LegalOffers);
        }
    }
}
