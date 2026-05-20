using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class LordJob_SleepThenMechanoidsDefend : LordJob_MechanoidDefendBase
{
	public bool awakeOnClamor;

	public override bool GuiltyOnDowned => true;

	public LordJob_SleepThenMechanoidsDefend()
	{
	}

	public LordJob_SleepThenMechanoidsDefend(List<Thing> things, Faction faction, float defendRadius, IntVec3 defSpot, bool canAssaultColony, bool isMechCluster)
	{
		if (things != null)
		{
			base.things.AddRange(things);
		}
		base.faction = faction;
		base.defendRadius = defendRadius;
		base.defSpot = defSpot;
		base.canAssaultColony = canAssaultColony;
		base.isMechCluster = isMechCluster;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_Sleep firstSource = (LordToil_Sleep)(stateGraph.StartingToil = new LordToil_Sleep());
		LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_MechanoidsDefend(things, faction, defendRadius, defSpot, canAssaultColony, isMechCluster).CreateGraph()).StartingToil;
		Transition transition = new Transition(firstSource, startingToil);
		transition.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.DormancyWakeup || (awakeOnClamor && signal.type == TriggerSignalType.Clamor)));
		transition.AddTrigger(new Trigger_OnHumanlikeHarmAnyThing(things));
		transition.AddTrigger(new Trigger_OnPlayerMechHarmAnything(things));
		transition.AddTrigger(new Trigger_PawnHarmed(1f, requireInstigatorWithFaction: true));
		transition.AddPreAction(new TransitionAction_Message("MessageSleepingPawnsWokenUp".Translate(faction.def.pawnsPlural).CapitalizeFirst(), MessageTypeDefOf.ThreatBig));
		transition.AddPostAction(new TransitionAction_WakeAll());
		transition.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			Find.SignalManager.SendSignal(new Signal("CompCanBeDormant.WakeUp", things.First().Named("SUBJECT"), Faction.OfMechanoids.Named("FACTION")));
			SoundDefOf.MechanoidsWakeUp.PlayOneShot(new TargetInfo(defSpot, base.Map));
		}));
		stateGraph.AddTransition(transition);
		return stateGraph;
	}

	public override bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
	{
		if (!p.Dead)
		{
			CompCanBeDormant compCanBeDormant = p.TryGetComp<CompCanBeDormant>();
			if (compCanBeDormant != null && !compCanBeDormant.Awake)
			{
				return false;
			}
		}
		return base.ShouldRemovePawn(p, reason);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref awakeOnClamor, "awakeOnClamor", defaultValue: false);
	}
}
