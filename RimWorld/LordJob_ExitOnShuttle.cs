using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_ExitOnShuttle : LordJob
	{
		public Thing shuttle;

		private bool addFleeToil = true;

		public override bool AddFleeToil => addFleeToil;

		public override bool RemoveDownedPawns => false;

		public LordJob_ExitOnShuttle()
		{
		}

		public LordJob_ExitOnShuttle(Thing shuttle, bool addFleeToil = true)
		{
			this.shuttle = shuttle;
			this.addFleeToil = addFleeToil;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Shuttle crash rescue is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 3454535);
				return stateGraph;
			}
			LordToil_Wait lordToil_Wait = new LordToil_Wait();
			stateGraph.AddToil(lordToil_Wait);
			stateGraph.StartingToil = lordToil_Wait;
			LordToil_EnterShuttleOrLeave lordToil_EnterShuttleOrLeave = new LordToil_EnterShuttleOrLeave(shuttle, LocomotionUrgency.Sprint);
			stateGraph.AddToil(lordToil_EnterShuttleOrLeave);
			Transition transition = new Transition(lordToil_Wait, lordToil_EnterShuttleOrLeave);
			transition.AddPreAction(new TransitionAction_Custom(InitializeLoading));
			transition.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && shuttle.Spawned));
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		private void InitializeLoading()
		{
			if (!shuttle.TryGetComp<CompTransporter>().LoadingInProgressOrReadyToLaunch)
			{
				TransporterUtility.InitiateLoading(Gen.YieldSingle(shuttle.TryGetComp<CompTransporter>()));
			}
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref shuttle, "shuttle");
			Scribe_Values.Look(ref addFleeToil, "addFleeToil", defaultValue: false);
		}
	}
}
