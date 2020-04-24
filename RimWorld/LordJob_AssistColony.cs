using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_AssistColony : LordJob
	{
		private Faction faction;

		private IntVec3 fallbackLocation;

		public LordJob_AssistColony()
		{
		}

		public LordJob_AssistColony(Faction faction, IntVec3 fallbackLocation)
		{
			this.faction = faction;
			this.fallbackLocation = fallbackLocation;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_HuntEnemies lordToil_HuntEnemies = new LordToil_HuntEnemies(fallbackLocation);
			stateGraph.AddToil(lordToil_HuntEnemies);
			StateGraph stateGraph2 = new LordJob_Travel(IntVec3.Invalid).CreateGraph();
			LordToil startingToil = stateGraph.AttachSubgraph(stateGraph2).StartingToil;
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap();
			stateGraph.AddToil(lordToil_ExitMap);
			LordToil_ExitMap lordToil_ExitMap2 = new LordToil_ExitMap(LocomotionUrgency.Jog, canDig: true);
			stateGraph.AddToil(lordToil_ExitMap2);
			Transition transition = new Transition(lordToil_HuntEnemies, startingToil);
			transition.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
			transition.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			transition.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_HuntEnemies, lordToil_ExitMap2);
			transition2.AddSource(lordToil_ExitMap);
			transition2.AddSources(stateGraph2.lordToils);
			transition2.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
			transition2.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_ExitMap2, startingToil);
			transition3.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition3.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_HuntEnemies, startingToil);
			transition4.AddPreAction(new TransitionAction_Message("MessageFriendlyFightersLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
			transition4.AddTrigger(new Trigger_TicksPassed(25000));
			transition4.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(startingToil, lordToil_ExitMap);
			transition5.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition5);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref fallbackLocation, "fallbackLocation");
		}
	}
}
