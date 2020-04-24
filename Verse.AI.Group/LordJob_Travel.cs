namespace Verse.AI.Group
{
	public class LordJob_Travel : LordJob
	{
		private IntVec3 travelDest;

		public LordJob_Travel()
		{
		}

		public LordJob_Travel(IntVec3 travelDest)
		{
			this.travelDest = travelDest;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Travel lordToil_Travel = (LordToil_Travel)(stateGraph.StartingToil = new LordToil_Travel(travelDest));
			LordToil_DefendPoint lordToil_DefendPoint = new LordToil_DefendPoint(canSatisfyLongNeeds: false);
			stateGraph.AddToil(lordToil_DefendPoint);
			Transition transition = new Transition(lordToil_Travel, lordToil_DefendPoint);
			transition.AddTrigger(new Trigger_PawnHarmed());
			transition.AddPreAction(new TransitionAction_SetDefendLocalGroup());
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_DefendPoint, lordToil_Travel);
			transition2.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
			transition2.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			stateGraph.AddTransition(transition2);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref travelDest, "travelDest");
		}
	}
}
