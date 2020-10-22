using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_FormAndSendCaravan : LordJob
	{
		public List<TransferableOneWay> transferables;

		public List<Pawn> downedPawns;

		private IntVec3 meetingPoint;

		private IntVec3 exitSpot;

		private int startingTile;

		private int destinationTile;

		private bool caravanSent;

		private LordToil gatherItems;

		private LordToil gatherItems_pause;

		private LordToil gatherDownedPawns;

		private LordToil gatherDownedPawns_pause;

		private LordToil leave;

		private LordToil leave_pause;

		public const float CustomWakeThreshold = 0.5f;

		public bool GatheringItemsNow => lord.CurLordToil == gatherItems;

		public override bool AllowStartNewGatherings => false;

		public override bool NeverInRestraints => true;

		public override bool AddFleeToil => false;

		public string Status
		{
			get
			{
				LordToil curLordToil = lord.CurLordToil;
				if (curLordToil == gatherItems)
				{
					return "FormingCaravanStatus_GatheringItems".Translate();
				}
				if (curLordToil == gatherItems_pause)
				{
					return "FormingCaravanStatus_GatheringItems_Pause".Translate();
				}
				if (curLordToil == gatherDownedPawns)
				{
					return "FormingCaravanStatus_GatheringDownedPawns".Translate();
				}
				if (curLordToil == gatherDownedPawns_pause)
				{
					return "FormingCaravanStatus_GatheringDownedPawns_Pause".Translate();
				}
				if (curLordToil == leave)
				{
					return "FormingCaravanStatus_Leaving".Translate();
				}
				if (curLordToil == leave_pause)
				{
					return "FormingCaravanStatus_Leaving_Pause".Translate();
				}
				return "FormingCaravanStatus_Waiting".Translate();
			}
		}

		public LordJob_FormAndSendCaravan()
		{
		}

		public LordJob_FormAndSendCaravan(List<TransferableOneWay> transferables, List<Pawn> downedPawns, IntVec3 meetingPoint, IntVec3 exitSpot, int startingTile, int destinationTile)
		{
			this.transferables = transferables;
			this.downedPawns = downedPawns;
			this.meetingPoint = meetingPoint;
			this.exitSpot = exitSpot;
			this.startingTile = startingTile;
			this.destinationTile = destinationTile;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			gatherItems = new LordToil_PrepareCaravan_GatherItems(meetingPoint);
			stateGraph.AddToil(gatherItems);
			gatherItems_pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(gatherItems_pause);
			gatherDownedPawns = new LordToil_PrepareCaravan_GatherDownedPawns(meetingPoint, exitSpot);
			stateGraph.AddToil(gatherDownedPawns);
			gatherDownedPawns_pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(gatherDownedPawns_pause);
			LordToil_PrepareCaravan_Wait lordToil_PrepareCaravan_Wait = new LordToil_PrepareCaravan_Wait(meetingPoint);
			stateGraph.AddToil(lordToil_PrepareCaravan_Wait);
			LordToil_PrepareCaravan_Pause lordToil_PrepareCaravan_Pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(lordToil_PrepareCaravan_Pause);
			leave = new LordToil_PrepareCaravan_Leave(exitSpot);
			stateGraph.AddToil(leave);
			leave_pause = new LordToil_PrepareCaravan_Pause();
			stateGraph.AddToil(leave_pause);
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			Transition transition = new Transition(gatherItems, gatherDownedPawns);
			transition.AddTrigger(new Trigger_Memo("AllItemsGathered"));
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(gatherDownedPawns, lordToil_PrepareCaravan_Wait);
			transition2.AddTrigger(new Trigger_Memo("AllDownedPawnsGathered"));
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_PrepareCaravan_Wait, leave);
			transition3.AddTrigger(new Trigger_NoPawnsVeryTiredAndSleeping());
			transition3.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(leave, lordToil_End);
			transition4.AddTrigger(new Trigger_Memo("ReadyToExitMap"));
			transition4.AddPreAction(new TransitionAction_Custom(SendCaravan));
			stateGraph.AddTransition(transition4);
			Transition transition5 = PauseTransition(gatherItems, gatherItems_pause);
			stateGraph.AddTransition(transition5);
			Transition transition6 = UnpauseTransition(gatherItems_pause, gatherItems);
			stateGraph.AddTransition(transition6);
			Transition transition7 = PauseTransition(gatherDownedPawns, gatherDownedPawns_pause);
			stateGraph.AddTransition(transition7);
			Transition transition8 = UnpauseTransition(gatherDownedPawns_pause, gatherDownedPawns);
			stateGraph.AddTransition(transition8);
			Transition transition9 = PauseTransition(leave, leave_pause);
			stateGraph.AddTransition(transition9);
			Transition transition10 = UnpauseTransition(leave_pause, leave);
			stateGraph.AddTransition(transition10);
			Transition transition11 = PauseTransition(lordToil_PrepareCaravan_Wait, lordToil_PrepareCaravan_Pause);
			stateGraph.AddTransition(transition11);
			Transition transition12 = UnpauseTransition(lordToil_PrepareCaravan_Pause, lordToil_PrepareCaravan_Wait);
			stateGraph.AddTransition(transition12);
			return stateGraph;
		}

		public override void LordJobTick()
		{
			base.LordJobTick();
			for (int num = downedPawns.Count - 1; num >= 0; num--)
			{
				if (downedPawns[num].Destroyed)
				{
					downedPawns.RemoveAt(num);
				}
				else if (!downedPawns[num].Downed)
				{
					lord.AddPawn(downedPawns[num]);
					downedPawns.RemoveAt(num);
				}
			}
		}

		public override string GetReport(Pawn pawn)
		{
			return "LordReportFormingCaravan".Translate();
		}

		private Transition PauseTransition(LordToil from, LordToil to)
		{
			Transition transition = new Transition(from, to);
			transition.AddPreAction(new TransitionAction_Message("MessageCaravanFormationPaused".Translate(), MessageTypeDefOf.NegativeEvent, () => lord.ownedPawns.FirstOrDefault((Pawn x) => x.InMentalState)));
			transition.AddTrigger(new Trigger_MentalState());
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			return transition;
		}

		private Transition UnpauseTransition(LordToil from, LordToil to)
		{
			Transition transition = new Transition(from, to);
			transition.AddPreAction(new TransitionAction_Message("MessageCaravanFormationUnpaused".Translate(), MessageTypeDefOf.SilentInput));
			transition.AddTrigger(new Trigger_NoMentalState());
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			return transition;
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref transferables, "transferables", LookMode.Deep);
			Scribe_Collections.Look(ref downedPawns, "downedPawns", LookMode.Reference);
			Scribe_Values.Look(ref meetingPoint, "meetingPoint");
			Scribe_Values.Look(ref exitSpot, "exitSpot");
			Scribe_Values.Look(ref startingTile, "startingTile", 0);
			Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				downedPawns.RemoveAll((Pawn x) => x.DestroyedOrNull());
			}
		}

		private void SendCaravan()
		{
			caravanSent = true;
			CaravanFormingUtility.FormAndCreateCaravan(lord.ownedPawns.Concat(downedPawns.Where((Pawn x) => JobGiver_PrepareCaravan_GatherDownedPawns.IsDownedPawnNearExitPoint(x, exitSpot))), lord.faction, base.Map.Tile, startingTile, destinationTile);
		}

		public override void Notify_PawnAdded(Pawn p)
		{
			base.Notify_PawnAdded(p);
			ReachabilityUtility.ClearCacheFor(p);
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
			base.Notify_PawnLost(p, condition);
			ReachabilityUtility.ClearCacheFor(p);
			if (!caravanSent)
			{
				if (condition == PawnLostCondition.IncappedOrKilled && p.Downed)
				{
					downedPawns.Add(p);
				}
				CaravanFormingUtility.RemovePawnFromCaravan(p, lord, removeFromDowned: false);
			}
		}

		public override bool CanOpenAnyDoor(Pawn p)
		{
			return true;
		}
	}
}
