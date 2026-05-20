using Verse.AI.Group;

namespace RimWorld;

public class LordJob_VoidAwakeningWander : LordJob
{
	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil toil = new LordToil_VoidAwakeningWander();
		stateGraph.AddToil(toil);
		return stateGraph;
	}
}
