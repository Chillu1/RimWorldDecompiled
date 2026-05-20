using System;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class LordJob_StructureThreatCluster : LordJob
{
	private Faction faction;

	private bool sendWokenUpMessage;

	public bool awakeOnClamor;

	public IntVec3 position;

	public float wanderRadius;

	public override bool GuiltyOnDowned => true;

	public LordJob_StructureThreatCluster()
	{
	}

	public LordJob_StructureThreatCluster(Faction faction, IntVec3 position, float wanderRadius, bool sendWokenUpMessage = true, bool awakeOnClamor = false)
	{
		this.faction = faction;
		this.sendWokenUpMessage = sendWokenUpMessage;
		this.position = position;
		this.wanderRadius = wanderRadius;
		this.awakeOnClamor = awakeOnClamor;
	}

	protected virtual LordToil GetIdleToil()
	{
		return new LordToil_Sleep();
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil firstSource = (stateGraph.StartingToil = GetIdleToil());
		LordToil_DefendPoint lordToil_DefendPoint = new LordToil_DefendPoint(position, wanderRadius: wanderRadius, defendRadius: wanderRadius);
		stateGraph.AddToil(lordToil_DefendPoint);
		LordToil_HuntDownColonists lordToil_HuntDownColonists = new LordToil_HuntDownColonists();
		stateGraph.AddToil(lordToil_HuntDownColonists);
		Transition transition = new Transition(firstSource, lordToil_DefendPoint);
		transition.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.DormancyWakeup || (awakeOnClamor && signal.type == TriggerSignalType.Clamor)));
		if (sendWokenUpMessage)
		{
			transition.AddPreAction(new TransitionAction_Message("MessageSleepingPawnsWokenUp".Translate(faction.def.pawnsPlural).CapitalizeFirst(), MessageTypeDefOf.ThreatBig, null, 1f, AnyAsleep));
		}
		transition.AddPostAction(new TransitionAction_WakeAll());
		transition.AddPostAction(WakeUpPostAction());
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(firstSource, lordToil_HuntDownColonists);
		transition2.AddTrigger(new Trigger_PawnHarmed(1f, requireInstigatorWithFaction: false, Faction.OfPlayer));
		transition2.AddTrigger(new Trigger_AcquiredTarget(Faction.OfPlayer));
		if (sendWokenUpMessage)
		{
			transition2.AddPreAction(new TransitionAction_Message("MessageSleepingPawnsWokenUp".Translate(faction.def.pawnsPlural).CapitalizeFirst(), MessageTypeDefOf.ThreatBig, null, 1f, AnyAsleep));
		}
		transition2.AddPostAction(new TransitionAction_WakeAll());
		transition2.AddPostAction(WakeUpPostAction());
		stateGraph.AddTransition(transition2);
		Transition transition3 = new Transition(lordToil_DefendPoint, lordToil_HuntDownColonists);
		transition3.AddTrigger(new Trigger_PawnHarmed(1f, requireInstigatorWithFaction: false, Faction.OfPlayer));
		transition3.AddTrigger(new Trigger_AcquiredTarget(Faction.OfPlayer));
		stateGraph.AddTransition(transition3);
		Transition transition4 = new Transition(lordToil_HuntDownColonists, lordToil_DefendPoint);
		transition4.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
		stateGraph.AddTransition(transition4);
		return stateGraph;
	}

	private TransitionAction_Custom WakeUpPostAction()
	{
		return new TransitionAction_Custom((Action)delegate
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
		});
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
		Scribe_Values.Look(ref position, "position");
		Scribe_Values.Look(ref wanderRadius, "wanderRadius", 0f);
	}
}
