using Verse.AI.Group;

namespace RimWorld;

public class LordJob_DevourerAssault : LordJob
{
	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil toil = new LordToil_DevourerAssault();
		stateGraph.AddToil(toil);
		return stateGraph;
	}
}
