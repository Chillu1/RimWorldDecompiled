using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class CompWakeUpDormant : ThingComp
{
	public bool wakeUpIfTargetClose;

	private bool sentSignal;

	public int groupID = -1;

	private CompCanBeDormant dormantComp;

	private static List<Thing> tmpActivatedThings = new List<Thing>();

	private CompProperties_WakeUpDormant Props => (CompProperties_WakeUpDormant)props;

	private CompCanBeDormant DormantComp => dormantComp ?? (dormantComp = parent.TryGetComp<CompCanBeDormant>());

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		wakeUpIfTargetClose = Props.wakeUpIfAnyTargetClose;
	}

	public override void CompTick()
	{
		if (parent.IsHashIntervalTick(250))
		{
			TickRareWorker();
		}
	}

	public void TickRareWorker()
	{
		if (!parent.Spawned || parent.Faction == Faction.OfPlayer)
		{
			return;
		}
		if (wakeUpIfTargetClose)
		{
			int num = GenRadial.NumCellsInRadius(Props.wakeUpCheckRadius);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = parent.Position + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(parent.Map) || !GenSight.LineOfSight(parent.Position, intVec, parent.Map))
				{
					continue;
				}
				foreach (Thing thing2 in intVec.GetThingList(parent.Map))
				{
					if (Props.wakeUpTargetingParams.CanTarget(thing2))
					{
						Activate(thing2);
						return;
					}
				}
			}
		}
		if (Props.wakeUpOnThingConstructedRadius > 0f)
		{
			Thing thing = GenClosest.ClosestThingReachable(parent.Position, parent.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors), Props.wakeUpOnThingConstructedRadius, (Thing t) => (t.def.building == null || t.def.building.wakeDormantPawnsOnConstruction) && t.Faction == Faction.OfPlayer);
			if (thing != null)
			{
				Activate(thing);
			}
		}
	}

	public void Activate(Thing waker, bool sendSignal = true, bool silent = false, bool instant = false)
	{
		if (DormantComp != null && DormantComp.Awake)
		{
			return;
		}
		if (sendSignal && (!sentSignal || !Props.onlySendSignalOnce))
		{
			if (!string.IsNullOrEmpty(Props.wakeUpSignalTag))
			{
				if (Props.onlyWakeUpSameFaction)
				{
					Find.SignalManager.SendSignal(new Signal(Props.wakeUpSignalTag, parent.Named("SUBJECT"), parent.Faction.Named("FACTION")));
				}
				else
				{
					Find.SignalManager.SendSignal(new Signal(Props.wakeUpSignalTag, parent.Named("SUBJECT")));
				}
			}
			if (!silent && parent.Spawned && Props.wakeUpSound != null)
			{
				Props.wakeUpSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
			}
			if (parent.Spawned && waker is Pawn && !silent && (!Props.activateMessageKey.NullOrEmpty() || !Props.activatePluralMessageKey.NullOrEmpty()))
			{
				tmpActivatedThings.Clear();
				if (groupID >= 0)
				{
					List<Thing> list = parent.Map.listerThings.ThingsInGroup(ThingRequestGroup.WakeUpDormant);
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].TryGetComp<CompWakeUpDormant>().groupID == groupID)
						{
							tmpActivatedThings.Add(list[i]);
						}
					}
				}
				else
				{
					tmpActivatedThings.Add(parent);
				}
				if (tmpActivatedThings.Count > 1 && !Props.activatePluralMessageKey.NullOrEmpty())
				{
					Messages.Message(Props.activatePluralMessageKey.Translate(waker.Named("WAKER")), tmpActivatedThings, Props.activateMessageType ?? MessageTypeDefOf.NegativeEvent, historical: false);
				}
				else if (tmpActivatedThings.Count > 0 && !Props.activateMessageKey.NullOrEmpty())
				{
					Messages.Message(Props.activateMessageKey.Translate(waker.Named("WAKER")), tmpActivatedThings, Props.activateMessageType ?? MessageTypeDefOf.NegativeEvent, historical: false);
				}
				tmpActivatedThings.Clear();
			}
			sentSignal = true;
		}
		if (DormantComp != null)
		{
			if (Props.wakeUpWithDelay && !instant)
			{
				DormantComp.WakeUpWithDelay();
			}
			else
			{
				DormantComp.WakeUp();
			}
		}
	}

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (Props.wakeUpOnDamage && dinfo.Def.ExternalViolenceFor(parent))
		{
			Activate(dinfo.Instigator, sendSignal: true, silent: false, instant: true);
		}
	}

	public override string CompInspectStringExtra()
	{
		string text = base.CompInspectStringExtra();
		if (wakeUpIfTargetClose && !Props.radiusCheckInspectPaneKey.NullOrEmpty())
		{
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text = string.Concat(text, Props.radiusCheckInspectPaneKey.Translate() + ": ", Mathf.RoundToInt(Props.wakeUpCheckRadius).ToString());
		}
		return text;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref sentSignal, "sentSignal", defaultValue: false);
		Scribe_Values.Look(ref groupID, "groupID", -1);
		Scribe_Values.Look(ref wakeUpIfTargetClose, "wakeUpIfColonistClose", defaultValue: false);
	}
}
