using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_ShamblerSwarm : LordJob_EntitySwarm
{
	public LordJob_ShamblerSwarm()
	{
	}

	public LordJob_ShamblerSwarm(IntVec3 startPos, IntVec3 destPos)
		: base(startPos, destPos)
	{
	}

	protected override LordToil CreateTravelingToil(IntVec3 start, IntVec3 dest)
	{
		return new LordToil_ShamblerSwarm(start, dest);
	}

	public override bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
	{
		if (reason == PawnLostCondition.Incapped && p.IsMutant && p.mutant.Def.canAttackWhileCrawling && p.health.CanCrawl)
		{
			return false;
		}
		return base.ShouldRemovePawn(p, reason);
	}

	public override void Notify_PawnDowned(Pawn p)
	{
		base.Notify_PawnDowned(p);
		if (!p.health.CanCrawl)
		{
			lord.Notify_PawnLost(p, PawnLostCondition.Incapped);
		}
	}
}
