using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_WanderNest : LordJob
{
	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_WanderNest toil = new LordToil_WanderNest();
		stateGraph.AddToil(toil);
		return stateGraph;
	}

	public override bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
	{
		if (reason == PawnLostCondition.Incapped)
		{
			return false;
		}
		return true;
	}
}
