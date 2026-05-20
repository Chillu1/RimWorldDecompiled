using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CreepJoinerWorker_DepartDownside : BaseCreepJoinerWorker
{
	public override bool CanDoResponse()
	{
		if (base.Pawn.Spawned)
		{
			return base.Pawn.Map.CanEverExit;
		}
		return false;
	}

	public override void DoResponse(List<TargetInfo> looktargets, List<NamedArgument> namedArgs)
	{
		base.Tracker.DoLeave();
	}
}
