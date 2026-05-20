using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_SitePawns : LordJob
{
	private Faction faction;

	private IntVec3 baseCenter;

	private int delayBeforeAssault;

	private List<Pawn> tmpPawns;

	private List<IntVec3> tmpCells;

	public LordJob_SitePawns()
	{
	}

	public LordJob_SitePawns(Faction faction, IntVec3 baseCenter, int delayBeforeAssault)
	{
		this.faction = faction;
		this.baseCenter = baseCenter;
		this.delayBeforeAssault = delayBeforeAssault;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_DefendBase firstSource = (LordToil_DefendBase)(stateGraph.StartingToil = new LordToil_DefendBase(baseCenter));
		LordToil_HuntDownColonists lordToil_HuntDownColonists = new LordToil_HuntDownColonists();
		stateGraph.AddToil(lordToil_HuntDownColonists);
		Transition transition = new Transition(firstSource, lordToil_HuntDownColonists);
		transition.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
		transition.AddTrigger(new Trigger_PawnHarmed(0.4f));
		transition.AddTrigger(new Trigger_ChanceOnTickInterval(2500, 0.03f));
		transition.AddTrigger(new Trigger_TicksPassed(delayBeforeAssault));
		transition.AddTrigger(new Trigger_UrgentlyHungry());
		transition.AddTrigger(new Trigger_ChanceOnPlayerHarmNPCBuilding(0.4f));
		transition.AddTrigger(new Trigger_OnClamor(ClamorDefOf.Ability));
		transition.AddPostAction(new TransitionAction_WakeAll());
		TaggedString taggedString = faction.def.messageDefendersAttacking.Formatted(faction.def.pawnsPlural, faction.Name, Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
		transition.AddPreAction(new TransitionAction_Message(taggedString, MessageTypeDefOf.ThreatBig));
		stateGraph.AddTransition(transition);
		return stateGraph;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref baseCenter, "baseCenter");
		Scribe_Values.Look(ref delayBeforeAssault, "delayBeforeAssault", 25000);
	}
}
