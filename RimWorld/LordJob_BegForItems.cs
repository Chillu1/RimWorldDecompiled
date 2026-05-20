using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_BegForItems : LordJob
{
	private IntVec3 idleSpot;

	private Faction faction;

	private Pawn target;

	private ThingDef thingDef;

	private int amount;

	private string outSignalItemsReceived;

	public LordJob_BegForItems()
	{
	}

	public LordJob_BegForItems(Faction faction, IntVec3 idleSpot, Pawn target, ThingDef thingDef, int amount, string outSignalItemsReceived = null)
	{
		this.idleSpot = idleSpot;
		this.faction = faction;
		this.target = target;
		this.thingDef = thingDef;
		this.amount = amount;
		this.outSignalItemsReceived = outSignalItemsReceived;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		if (!ModLister.CheckIdeology("Beg for items"))
		{
			return stateGraph;
		}
		LordToil_TravelAndWaitForItems lordToil_TravelAndWaitForItems = new LordToil_TravelAndWaitForItems(idleSpot, target, thingDef, amount);
		stateGraph.AddToil(lordToil_TravelAndWaitForItems);
		stateGraph.StartingToil = lordToil_TravelAndWaitForItems;
		LordToil_WaitForItems waitForItems = new LordToil_WaitForItems(target, thingDef, amount, idleSpot);
		stateGraph.AddToil(waitForItems);
		LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap();
		stateGraph.AddToil(lordToil_ExitMap);
		LordToil_ExitMapAndDefendSelf toil = new LordToil_ExitMapAndDefendSelf();
		stateGraph.AddToil(toil);
		Transition transition = new Transition(lordToil_TravelAndWaitForItems, waitForItems);
		transition.AddTrigger(new Trigger_Memo("TravelArrived"));
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(waitForItems, lordToil_ExitMap);
		transition2.AddSource(lordToil_TravelAndWaitForItems);
		transition2.AddTrigger(new Trigger_Custom((TriggerSignal s) => waitForItems.HasAllRequestedItems));
		transition2.AddPostAction(new TransitionAction_EndAllJobs());
		if (!outSignalItemsReceived.NullOrEmpty())
		{
			transition2.AddPostAction(new TransitionAction_Custom((Action)delegate
			{
				Find.SignalManager.SendSignal(new Signal(outSignalItemsReceived));
			}));
		}
		stateGraph.AddTransition(transition2);
		Transition transition3 = new Transition(waitForItems, toil);
		transition3.AddSource(lordToil_TravelAndWaitForItems);
		transition3.AddSource(lordToil_ExitMap);
		transition3.AddTrigger(new Trigger_BecamePlayerEnemy());
		transition3.AddTrigger(new Trigger_PawnKilled());
		transition3.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition3);
		Transition transition4 = new Transition(lordToil_TravelAndWaitForItems, lordToil_ExitMap);
		transition4.AddSource(waitForItems);
		transition4.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
		transition4.AddPostAction(new TransitionAction_EndAllJobs());
		transition4.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
		stateGraph.AddTransition(transition4);
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref target, "target");
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref idleSpot, "idleSpot");
		Scribe_Values.Look(ref amount, "amount", 0);
		Scribe_Values.Look(ref outSignalItemsReceived, "outSignalItemsReceived");
		Scribe_Defs.Look(ref thingDef, "thingDef");
	}
}
