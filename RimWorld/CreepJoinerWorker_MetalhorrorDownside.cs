using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CreepJoinerWorker_MetalhorrorDownside : BaseCreepJoinerWorker
{
	public override void OnCreated()
	{
		MetalhorrorUtility.Infect(base.Pawn);
	}

	public override void DoResponse(List<TargetInfo> looktargets, List<NamedArgument> namedArgs)
	{
	}
}
