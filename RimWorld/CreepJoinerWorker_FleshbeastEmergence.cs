using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CreepJoinerWorker_FleshbeastEmergence : BaseCreepJoinerWorker
{
	public override bool CanOccurOnDeath => true;

	public override void DoResponse(List<TargetInfo> looktargets, List<NamedArgument> namedArgs)
	{
		Pawn pawn = FleshbeastUtility.SpawnFleshbeastFromPawn(base.Pawn, true, false, PawnKindDefOf.Bulbfreak);
		looktargets.Add(pawn);
	}
}
