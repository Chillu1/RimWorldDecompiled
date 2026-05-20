using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_AssaultColony : LordJob, ILordAvoidTraps
{
	private Faction assaulterFaction;

	private bool canKidnap = true;

	private bool canTimeoutOrFlee = true;

	private bool sappers;

	private bool useAvoidGridSmart;

	private bool canSteal = true;

	private bool breachers;

	private bool canPickUpOpportunisticWeapons;

	private static readonly IntRange AssaultTimeBeforeGiveUp = new IntRange(26000, 38000);

	private static readonly IntRange SapTimeBeforeGiveUp = new IntRange(33000, 38000);

	private static readonly IntRange BreachTimeBeforeGiveUp = new IntRange(33000, 38000);

	public override bool GuiltyOnDowned => true;

	public float AvoidTrapRatio
	{
		get
		{
			if (!useAvoidGridSmart)
			{
				return 0f;
			}
			return 0.3f;
		}
	}

	public LordJob_AssaultColony()
	{
	}

	public LordJob_AssaultColony(SpawnedPawnParams parms)
	{
		assaulterFaction = parms.spawnerThing.Faction;
		canKidnap = false;
		canTimeoutOrFlee = false;
		canSteal = false;
	}

	public LordJob_AssaultColony(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true, bool breachers = false, bool canPickUpOpportunisticWeapons = false)
	{
		this.assaulterFaction = assaulterFaction;
		this.canKidnap = canKidnap;
		this.canTimeoutOrFlee = canTimeoutOrFlee;
		this.sappers = sappers;
		this.useAvoidGridSmart = useAvoidGridSmart;
		this.canSteal = canSteal;
		this.breachers = breachers;
		this.canPickUpOpportunisticWeapons = canPickUpOpportunisticWeapons;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		List<LordToil> list = new List<LordToil>();
		LordToil lordToil = null;
		if (sappers)
		{
			lordToil = new LordToil_AssaultColonySappers();
			if (useAvoidGridSmart)
			{
				lordToil.useAvoidGrid = true;
			}
			stateGraph.AddToil(lordToil);
			list.Add(lordToil);
			Transition transition = new Transition(lordToil, lordToil, canMoveToSameState: true);
			transition.AddTrigger(new Trigger_PawnLost());
			stateGraph.AddTransition(transition);
		}
		LordToil lordToil2 = null;
		if (breachers)
		{
			lordToil2 = new LordToil_AssaultColonyBreaching();
			if (useAvoidGridSmart)
			{
				lordToil2.useAvoidGrid = useAvoidGridSmart;
			}
			stateGraph.AddToil(lordToil2);
			list.Add(lordToil2);
		}
		LordToil lordToil3 = new LordToil_AssaultColony(attackDownedIfStarving: false, canPickUpOpportunisticWeapons);
		if (useAvoidGridSmart)
		{
			lordToil3.useAvoidGrid = true;
		}
		stateGraph.AddToil(lordToil3);
		LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, canDig: false, interruptCurrentJob: true);
		lordToil_ExitMap.useAvoidGrid = true;
		stateGraph.AddToil(lordToil_ExitMap);
		if (sappers)
		{
			Transition transition2 = new Transition(lordToil, lordToil3);
			transition2.AddTrigger(new Trigger_NoFightingSappers());
			stateGraph.AddTransition(transition2);
		}
		if (assaulterFaction != null && assaulterFaction.def.humanlikeFaction)
		{
			if (canTimeoutOrFlee)
			{
				Transition transition3 = new Transition(lordToil3, lordToil_ExitMap);
				transition3.AddSources(list);
				Trigger_TicksPassed trigger_TicksPassed = new Trigger_TicksPassed((sappers ? SapTimeBeforeGiveUp : ((!breachers) ? AssaultTimeBeforeGiveUp : BreachTimeBeforeGiveUp)).RandomInRange);
				trigger_TicksPassed.WithFilter(new TriggerFilter_MapExitable());
				transition3.AddTrigger(trigger_TicksPassed);
				transition3.AddPreAction(new TransitionAction_Message("MessageRaidersGivenUpLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
				stateGraph.AddTransition(transition3);
				Transition transition4 = new Transition(lordToil3, lordToil_ExitMap);
				transition4.AddSources(list);
				Trigger_FractionColonyDamageTaken trigger_FractionColonyDamageTaken = new Trigger_FractionColonyDamageTaken(new FloatRange(0.25f, 0.35f).RandomInRange, 900f);
				trigger_FractionColonyDamageTaken.WithFilter(new TriggerFilter_MapExitable());
				transition4.AddTrigger(trigger_FractionColonyDamageTaken);
				transition4.AddPreAction(new TransitionAction_Message("MessageRaidersSatisfiedLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
				stateGraph.AddTransition(transition4);
			}
			if (canKidnap)
			{
				LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Kidnap().CreateGraph()).StartingToil;
				Transition transition5 = new Transition(lordToil3, startingToil);
				transition5.AddSources(list);
				transition5.AddPreAction(new TransitionAction_Message("MessageRaidersKidnapping".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
				transition5.AddTrigger(new Trigger_KidnapVictimPresent());
				stateGraph.AddTransition(transition5);
			}
			if (canSteal)
			{
				LordToil startingToil2 = stateGraph.AttachSubgraph(new LordJob_Steal().CreateGraph()).StartingToil;
				Transition transition6 = new Transition(lordToil3, startingToil2);
				transition6.AddSources(list);
				transition6.AddPreAction(new TransitionAction_Message("MessageRaidersStealing".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
				transition6.AddTrigger(new Trigger_HighValueThingsAround());
				stateGraph.AddTransition(transition6);
			}
		}
		if (assaulterFaction != null && assaulterFaction == Faction.OfMechanoids)
		{
			Transition transition7 = new Transition(lordToil3, lordToil_ExitMap);
			transition7.AddSources(list);
			transition7.AddTrigger(new Trigger_GameEnding());
			transition7.AddPreAction(new TransitionAction_Message("MechanoidsMovingOn".Translate()));
			stateGraph.AddTransition(transition7);
		}
		if (assaulterFaction != null)
		{
			Transition transition8 = new Transition(lordToil3, lordToil_ExitMap);
			transition8.AddSources(list);
			transition8.AddTrigger(new Trigger_BecameNonHostileToPlayer());
			transition8.AddPreAction(new TransitionAction_Message("MessageRaidersLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
			stateGraph.AddTransition(transition8);
		}
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref assaulterFaction, "assaulterFaction");
		Scribe_Values.Look(ref canKidnap, "canKidnap", defaultValue: true);
		Scribe_Values.Look(ref canTimeoutOrFlee, "canTimeoutOrFlee", defaultValue: true);
		Scribe_Values.Look(ref sappers, "sappers", defaultValue: false);
		Scribe_Values.Look(ref useAvoidGridSmart, "useAvoidGridSmart", defaultValue: false);
		Scribe_Values.Look(ref canSteal, "canSteal", defaultValue: true);
		Scribe_Values.Look(ref breachers, "breaching", defaultValue: false);
		Scribe_Values.Look(ref canPickUpOpportunisticWeapons, "canPickUpOpportunisticWeapons", defaultValue: false);
	}
}
