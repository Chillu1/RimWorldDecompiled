using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CreepJoinerWorker_DoAggressive : BaseCreepJoinerWorker
{
	public override void DoResponse(List<TargetInfo> looktargets, List<NamedArgument> namedArgs)
	{
		base.Tracker.DoAggressive();
	}
}
