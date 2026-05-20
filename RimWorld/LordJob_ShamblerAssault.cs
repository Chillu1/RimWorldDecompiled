using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_ShamblerAssault : LordJob
{
	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil toil = new LordToil_ShamblerAssault();
		stateGraph.AddToil(toil);
		return stateGraph;
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
