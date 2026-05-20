using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_AssaultThings : LordJob
{
	private Faction assaulterFaction;

	private List<Thing> things;

	private bool useAvoidGridSmart;

	private float damageFraction;

	public LordJob_AssaultThings()
	{
	}

	public LordJob_AssaultThings(Faction assaulterFaction, List<Thing> things, float damageFraction = 1f, bool useAvoidGridSmart = false)
	{
		this.assaulterFaction = assaulterFaction;
		this.things = things;
		this.useAvoidGridSmart = useAvoidGridSmart;
		this.damageFraction = damageFraction;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil lordToil = new LordToil_AssaultThings(things);
		if (useAvoidGridSmart)
		{
			lordToil.useAvoidGrid = true;
		}
		stateGraph.AddToil(lordToil);
		LordToil_ExitMapAndDefendSelf lordToil_ExitMapAndDefendSelf = new LordToil_ExitMapAndDefendSelf
		{
			useAvoidGrid = true
		};
		stateGraph.AddToil(lordToil_ExitMapAndDefendSelf);
		Transition transition = new Transition(lordToil, lordToil_ExitMapAndDefendSelf);
		transition.AddTrigger(new Trigger_ThingsDamageTaken(things, damageFraction));
		stateGraph.AddTransition(transition);
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref assaulterFaction, "assaulterFaction");
		Scribe_Collections.Look(ref things, "things", LookMode.Reference);
		Scribe_Values.Look(ref useAvoidGridSmart, "useAvoidGridSmart", defaultValue: false);
		Scribe_Values.Look(ref damageFraction, "damageFraction", 0f);
	}
}
