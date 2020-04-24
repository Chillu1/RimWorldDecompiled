using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_PrisonBreak : LordJob
	{
		private IntVec3 groupUpLoc;

		private IntVec3 exitPoint;

		private int sapperThingID = -1;

		public override bool NeverInRestraints => true;

		public override bool AddFleeToil => false;

		public LordJob_PrisonBreak()
		{
		}

		public LordJob_PrisonBreak(IntVec3 groupUpLoc, IntVec3 exitPoint, int sapperThingID)
		{
			this.groupUpLoc = groupUpLoc;
			this.exitPoint = exitPoint;
			this.sapperThingID = sapperThingID;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Travel lordToil_Travel = new LordToil_Travel(groupUpLoc);
			lordToil_Travel.maxDanger = Danger.Deadly;
			lordToil_Travel.useAvoidGrid = true;
			stateGraph.StartingToil = lordToil_Travel;
			LordToil_PrisonerEscape lordToil_PrisonerEscape = new LordToil_PrisonerEscape(exitPoint, sapperThingID);
			lordToil_PrisonerEscape.useAvoidGrid = true;
			stateGraph.AddToil(lordToil_PrisonerEscape);
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog);
			lordToil_ExitMap.useAvoidGrid = true;
			stateGraph.AddToil(lordToil_ExitMap);
			LordToil_ExitMap lordToil_ExitMap2 = new LordToil_ExitMap(LocomotionUrgency.Jog, canDig: true);
			stateGraph.AddToil(lordToil_ExitMap2);
			Transition transition = new Transition(lordToil_Travel, lordToil_ExitMap2);
			transition.AddSources(lordToil_PrisonerEscape, lordToil_ExitMap);
			transition.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_ExitMap2, lordToil_ExitMap);
			transition2.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_Travel, lordToil_PrisonerEscape);
			transition3.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_Travel, lordToil_PrisonerEscape);
			transition4.AddTrigger(new Trigger_PawnLost());
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_PrisonerEscape, lordToil_PrisonerEscape, canMoveToSameState: true);
			transition5.AddTrigger(new Trigger_PawnLost());
			transition5.AddTrigger(new Trigger_PawnHarmed());
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_PrisonerEscape, lordToil_ExitMap);
			transition6.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition6);
			return stateGraph;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref groupUpLoc, "groupUpLoc");
			Scribe_Values.Look(ref exitPoint, "exitPoint");
			Scribe_Values.Look(ref sapperThingID, "sapperThingID", -1);
		}

		public override void Notify_PawnAdded(Pawn p)
		{
			ReachabilityUtility.ClearCacheFor(p);
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			ReachabilityUtility.ClearCacheFor(p);
		}

		public override bool CanOpenAnyDoor(Pawn p)
		{
			return true;
		}

		public override bool ValidateAttackTarget(Pawn searcher, Thing target)
		{
			Pawn pawn = target as Pawn;
			if (pawn == null)
			{
				return true;
			}
			MentalStateDef mentalStateDef = pawn.MentalStateDef;
			if (mentalStateDef == null)
			{
				return true;
			}
			return !mentalStateDef.escapingPrisonersIgnore;
		}
	}
}
