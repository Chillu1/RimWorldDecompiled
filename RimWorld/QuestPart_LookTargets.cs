using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_LookTargets : QuestPart
{
	public List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets => targets;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref targets, "targets", LookMode.GlobalTargetInfo);
	}
}
