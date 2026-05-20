using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_FormAndSendCaravan : LordJob
{
	public List<TransferableOneWay> transferables;

	public List<Pawn> downedPawns;

	private IntVec3 meetingPoint;

	private IntVec3 exitSpot;

	private PlanetTile startingTile;

	private PlanetTile destinationTile;

	private bool caravanSent;

	private LordToil gatherAnimals;

	private LordToil gatherAnimals_pause;

	private LordToil gatherItems;

	private LordToil gatherItems_pause;

	private LordToil gatherDownedPawns;

	private LordToil gatherDownedPawns_pause;

	private LordToil collectAnimals;

	private LordToil collectAnimals_pause;

	private LordToil leave;

	private LordToil leave_pause;

	public const float CustomWakeThreshold = 0.5f;

	public bool GatheringItemsNow => lord.CurLordToil == gatherItems;

	public override bool AllowStartNewGatherings => false;

	public override bool NeverInRestraints => true;

	public override bool AddFleeToil => false;

	public override bool ManagesRopableAnimals => true;

	public IntVec3 ExitSpot => exitSpot;

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
			if (curLordToil == gatherAnimals)
			{
				return "FormingCaravanStatus_GatheringAnimals".Translate();
			}
			if (curLordToil == gatherAnimals_pause)
			{
				return "FormingCaravanStatus_GatheringAnimals_Pause".Translate();
			}
			if (curLordToil == collectAnimals)
			{
				return "FormingCaravanStatus_GatheringAnimals".Translate();
			}
			if (curLordToil == collectAnimals_pause)
			{
				return "FormingCaravanStatus_GatheringAnimals_Pause".Translate();
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

	public LordJob_FormAndSendCaravan(List<TransferableOneWay> transferables, List<Pawn> downedPawns, IntVec3 meetingPoint, IntVec3 exitSpot, PlanetTile startingTile, PlanetTile destinationTile)
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
		gatherAnimals = new LordToil_PrepareCaravan_GatherAnimals(meetingPoint);
		stateGraph.AddToil(gatherAnimals);
		gatherAnimals_pause = new LordToil_PrepareCaravan_Pause();
		stateGraph.AddToil(gatherAnimals_pause);
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
		collectAnimals = new LordToil_PrepareCaravan_CollectAnimals(exitSpot);
		stateGraph.AddToil(collectAnimals);
		collectAnimals_pause = new LordToil_PrepareCaravan_Pause();
		stateGraph.AddToil(collectAnimals_pause);
		leave = new LordToil_PrepareCaravan_Leave(exitSpot);
		stateGraph.AddToil(leave);
		leave_pause = new LordToil_PrepareCaravan_Pause();
		stateGraph.AddToil(leave_pause);
		LordToil_End lordToil_End = new LordToil_End();
		stateGraph.AddToil(lordToil_End);
		Transition transition = new Transition(gatherAnimals, gatherItems);
		transition.AddTrigger(new Trigger_Memo("AllAnimalsGathered"));
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(gatherItems, gatherDownedPawns);
		transition2.AddTrigger(new Trigger_Memo("AllItemsGathered"));
		transition2.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition2);
		Transition transition3 = new Transition(gatherDownedPawns, lordToil_PrepareCaravan_Wait);
		transition3.AddTrigger(new Trigger_Memo("AllDownedPawnsGathered"));
		transition3.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition3);
		Transition transition4 = new Transition(lordToil_PrepareCaravan_Wait, collectAnimals);
		transition4.AddTrigger(new Trigger_NoPawnsVeryTiredAndSleeping());
		transition4.AddPostAction(new TransitionAction_WakeAll());
		stateGraph.AddTransition(transition4);
		Transition transition5 = new Transition(collectAnimals, leave);
		transition5.AddTrigger(new Trigger_Memo("AllAnimalsCollected"));
		stateGraph.AddTransition(transition5);
		Transition transition6 = new Transition(leave, collectAnimals);
		transition6.AddTrigger(new Trigger_Memo("AddedRopeAnimal"));
		transition6.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition6);
		Transition transition7 = new Transition(leave, collectAnimals);
		transition7.AddTrigger(new Trigger_Memo("RemovedPawn"));
		transition7.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition7);
		Transition transition8 = new Transition(leave, lordToil_End);
		transition8.AddTrigger(new Trigger_Memo("ReadyToExitMap"));
		transition8.AddPreAction(new TransitionAction_Custom(SendCaravan));
		stateGraph.AddTransition(transition8);
		Transition transition9 = PauseTransition(gatherAnimals, gatherAnimals_pause);
		stateGraph.AddTransition(transition9);
		Transition transition10 = UnpauseTransition(gatherAnimals_pause, gatherAnimals);
		stateGraph.AddTransition(transition10);
		Transition transition11 = PauseTransition(gatherItems, gatherItems_pause);
		stateGraph.AddTransition(transition11);
		Transition transition12 = UnpauseTransition(gatherItems_pause, gatherItems);
		stateGraph.AddTransition(transition12);
		Transition transition13 = PauseTransition(gatherDownedPawns, gatherDownedPawns_pause);
		stateGraph.AddTransition(transition13);
		Transition transition14 = UnpauseTransition(gatherDownedPawns_pause, gatherDownedPawns);
		stateGraph.AddTransition(transition14);
		Transition transition15 = PauseTransition(collectAnimals, collectAnimals_pause);
		stateGraph.AddTransition(transition15);
		Transition transition16 = UnpauseTransition(collectAnimals_pause, collectAnimals);
		stateGraph.AddTransition(transition16);
		Transition transition17 = PauseTransition(leave, leave_pause);
		stateGraph.AddTransition(transition17);
		Transition transition18 = UnpauseTransition(leave_pause, collectAnimals);
		stateGraph.AddTransition(transition18);
		Transition transition19 = PauseTransition(lordToil_PrepareCaravan_Wait, lordToil_PrepareCaravan_Pause);
		stateGraph.AddTransition(transition19);
		Transition transition20 = UnpauseTransition(lordToil_PrepareCaravan_Pause, lordToil_PrepareCaravan_Wait);
		stateGraph.AddTransition(transition20);
		Transition transition21 = new Transition(gatherAnimals, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
		transition21 = new Transition(gatherItems, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
		transition21 = new Transition(gatherDownedPawns, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
		transition21 = new Transition(lordToil_PrepareCaravan_Wait, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
		transition21 = new Transition(collectAnimals, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
		transition21 = new Transition(gatherAnimals_pause, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
		transition21 = new Transition(gatherItems_pause, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
		transition21 = new Transition(gatherDownedPawns_pause, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
		transition21 = new Transition(lordToil_PrepareCaravan_Pause, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
		transition21 = new Transition(collectAnimals_pause, leave);
		transition21.AddTrigger(new Trigger_Memo("ForceDepartNow"));
		transition21.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition21);
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
		Scribe_Values.Look(ref startingTile, "startingTile");
		Scribe_Values.Look(ref destinationTile, "destinationTile");
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
		p.jobs?.SetFormingCaravanTick();
		ReachabilityUtility.ClearCacheFor(p);
		if (AnimalPenUtility.NeedsToBeManagedByRope(p))
		{
			if (p.roping.IsRopedByPawn)
			{
				p.roping.BreakAllRopes();
			}
			lord.ReceiveMemo("AddedRopeAnimal");
		}
	}

	public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
	{
		base.Notify_PawnLost(p, condition);
		ReachabilityUtility.ClearCacheFor(p);
		if (!caravanSent)
		{
			if ((condition == PawnLostCondition.Incapped || condition == PawnLostCondition.Killed) && p.Downed)
			{
				downedPawns.Add(p);
			}
			CaravanFormingUtility.RemovePawnFromCaravan(p, lord, removeFromDowned: false);
			lord.ReceiveMemo("RemovedPawn");
		}
	}

	public override bool CanOpenAnyDoor(Pawn p)
	{
		if (p.FenceBlocked)
		{
			return false;
		}
		return true;
	}
}
