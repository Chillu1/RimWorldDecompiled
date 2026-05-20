using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_EscortPawn : LordJob
{
	public Pawn escortee;

	public Thing shuttle;

	private Faction escorteeFaction;

	public string leavingDangerMessage;

	public override bool AlwaysShowWeapon => true;

	public LordJob_EscortPawn()
	{
	}

	public LordJob_EscortPawn(Pawn escortee, Thing shuttle = null)
	{
		this.escortee = escortee;
		this.shuttle = shuttle;
		escorteeFaction = escortee.Faction;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		CompShuttle compShuttle = shuttle.TryGetComp<CompShuttle>();
		LordToil_EscortPawn lordToil_EscortPawn = new LordToil_EscortPawn(escortee);
		stateGraph.AddToil(lordToil_EscortPawn);
		LordToil lordToil = ((shuttle == null) ? ((LordToil)new LordToil_ExitMap()) : ((LordToil)new LordToil_EnterShuttleOrLeave(shuttle)));
		stateGraph.AddToil(lordToil);
		LordToil lordToil2 = ((shuttle == null) ? ((LordToil)new LordToil_ExitMapFighting(LocomotionUrgency.Jog, canDig: true)) : ((LordToil)new LordToil_EnterShuttleOrLeave(shuttle)));
		lordToil2.useAvoidGrid = true;
		stateGraph.AddToil(lordToil2);
		Transition transition = new Transition(lordToil_EscortPawn, lordToil);
		Trigger_Custom trigger = new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && escortee.Dead);
		transition.AddTrigger(trigger);
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(lordToil_EscortPawn, lordToil);
		Trigger_Custom trigger2 = new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && (escortee.MapHeld != lord.Map || (shuttle != null && escortee.ParentHolder == shuttle.TryGetComp<CompTransporter>() && shuttle.TryGetComp<CompShuttle>().shipParent.Waiting)));
		transition2.AddTrigger(trigger2);
		stateGraph.AddTransition(transition2);
		Transition transition3 = new Transition(lordToil_EscortPawn, lordToil);
		transition3.AddTrigger(new Trigger_BecamePlayerEnemy());
		stateGraph.AddTransition(transition3);
		Transition transition4 = new Transition(lordToil_EscortPawn, lordToil);
		transition4.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && escortee.Faction != escorteeFaction));
		stateGraph.AddTransition(transition4);
		Transition transition5 = new Transition(lordToil_EscortPawn, lordToil2);
		transition5.AddTrigger(new Trigger_PawnHarmed(1f, requireInstigatorWithFaction: true, base.Map.ParentFaction));
		transition5.AddTrigger(new Trigger_PawnLostViolently(allowRoofCollapse: true, escortee.Faction));
		transition5.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			if (!leavingDangerMessage.NullOrEmpty())
			{
				Messages.Message(leavingDangerMessage, escortee, MessageTypeDefOf.NegativeEvent);
			}
			compShuttle?.SetPawnToLeaveBehind((Pawn p) => p != escortee);
			QuestUtility.SendQuestTargetSignals(lord.questTags, "BeingAttacked", lord.Named("SUBJECT"));
		}));
		stateGraph.AddTransition(transition5);
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref escortee, "escortee");
		Scribe_References.Look(ref shuttle, "shuttle");
		Scribe_References.Look(ref escorteeFaction, "escorteeFaction");
		Scribe_Values.Look(ref leavingDangerMessage, "leavingDangerMessage");
	}
}
