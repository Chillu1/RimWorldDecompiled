using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_GorehulkAssault : LordJob
{
	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil toil = new LordToil_GorehulkAssault();
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
