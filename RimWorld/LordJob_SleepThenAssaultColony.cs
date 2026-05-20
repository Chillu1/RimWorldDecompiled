using System;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class LordJob_SleepThenAssaultColony : LordJob
{
	private Faction faction;

	private bool sendWokenUpMessage;

	public bool awakeOnClamor;

	public override bool GuiltyOnDowned => true;

	public LordJob_SleepThenAssaultColony()
	{
	}

	public LordJob_SleepThenAssaultColony(Faction faction, bool sendWokenUpMessage = true)
	{
		this.faction = faction;
		this.sendWokenUpMessage = sendWokenUpMessage;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_Sleep firstSource = (LordToil_Sleep)(stateGraph.StartingToil = new LordToil_Sleep());
		LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction).CreateGraph()).StartingToil;
		Transition transition = new Transition(firstSource, startingToil);
		transition.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.DormancyWakeup || (awakeOnClamor && signal.type == TriggerSignalType.Clamor)));
		if (sendWokenUpMessage)
		{
			transition.AddPreAction(new TransitionAction_Message("MessageSleepingPawnsWokenUp".Translate(faction.def.pawnsPlural).CapitalizeFirst(), MessageTypeDefOf.ThreatBig, null, 1f, AnyAsleep));
		}
		transition.AddTrigger(new Trigger_PawnHarmed(1f, requireInstigatorWithFaction: false, Faction.OfPlayer));
		transition.AddPostAction(new TransitionAction_WakeAll());
		transition.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				zero += lord.ownedPawns[i].Position.ToVector3();
			}
			zero /= (float)lord.ownedPawns.Count;
			if (faction == Faction.OfMechanoids)
			{
				SoundDefOf.MechanoidsWakeUp.PlayOneShot(new TargetInfo(zero.ToIntVec3(), base.Map));
			}
			else if (ModsConfig.IdeologyActive && faction == Faction.OfInsects)
			{
				SoundDefOf.InsectsWakeUp.PlayOneShot(new TargetInfo(zero.ToIntVec3(), base.Map));
			}
		}));
		stateGraph.AddTransition(transition);
		return stateGraph;
	}

	private bool AnyAsleep()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].Spawned && !lord.ownedPawns[i].Dead && !lord.ownedPawns[i].Awake())
			{
				return true;
			}
		}
		return false;
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
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref sendWokenUpMessage, "sendWokenUpMessage", defaultValue: true);
		Scribe_Values.Look(ref awakeOnClamor, "awakeOnClamor", defaultValue: false);
	}
}
