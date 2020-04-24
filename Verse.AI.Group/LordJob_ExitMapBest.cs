namespace Verse.AI.Group
{
	public class LordJob_ExitMapBest : LordJob
	{
		private LocomotionUrgency locomotion = LocomotionUrgency.Jog;

		private bool canDig;

		private bool canDefendSelf;

		public LordJob_ExitMapBest()
		{
		}

		public LordJob_ExitMapBest(LocomotionUrgency locomotion, bool canDig = false, bool canDefendSelf = false)
		{
			this.locomotion = locomotion;
			this.canDig = canDig;
			this.canDefendSelf = canDefendSelf;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(locomotion, canDig);
			lordToil_ExitMap.useAvoidGrid = true;
			stateGraph.AddToil(lordToil_ExitMap);
			if (canDefendSelf)
			{
				LordToil_ExitMapFighting lordToil_ExitMapFighting = new LordToil_ExitMapFighting(LocomotionUrgency.Jog, canDig);
				stateGraph.AddToil(lordToil_ExitMapFighting);
				Transition transition = new Transition(lordToil_ExitMap, lordToil_ExitMapFighting);
				transition.AddTrigger(new Trigger_PawnHarmed());
				transition.AddPostAction(new TransitionAction_WakeAll());
				transition.AddPostAction(new TransitionAction_EndAllJobs());
				stateGraph.AddTransition(transition);
			}
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref locomotion, "locomotion", LocomotionUrgency.Jog);
			Scribe_Values.Look(ref canDig, "canDig", defaultValue: false);
			Scribe_Values.Look(ref canDefendSelf, "canDefendSelf", defaultValue: false);
		}
	}
}
