using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_CameraJump : QuestPart
{
	public string inSignal;

	public LookTargets lookTargets;

	public bool getLookTargetsFromSignal = true;

	public bool select = true;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			foreach (GlobalTargetInfo questLookTarget in base.QuestLookTargets)
			{
				yield return questLookTarget;
			}
			GlobalTargetInfo globalTargetInfo = lookTargets.TryGetPrimaryTarget();
			if (globalTargetInfo.IsValid)
			{
				yield return globalTargetInfo;
			}
		}
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		LookTargets lookTargets = this.lookTargets;
		if (getLookTargetsFromSignal && !lookTargets.IsValid())
		{
			SignalArgsUtility.TryGetLookTargets(signal.args, "SUBJECT", out lookTargets);
		}
		if (lookTargets.IsValid())
		{
			if (select)
			{
				CameraJumper.TryJumpAndSelect(lookTargets.TryGetPrimaryTarget());
			}
			else
			{
				CameraJumper.TryJump(lookTargets.TryGetPrimaryTarget());
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Deep.Look(ref lookTargets, "lookTargets");
		Scribe_Values.Look(ref getLookTargetsFromSignal, "getLookTargetsFromSignal", defaultValue: true);
		Scribe_Values.Look(ref select, "select", defaultValue: true);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		lookTargets = Find.Maps.SelectMany((Map x) => x.mapPawns.FreeColonistsSpawned).RandomElementWithFallback();
	}
}
