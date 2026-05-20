using Verse.AI.Group;

namespace RimWorld;

public class LordJob_FleshbeastAssault : LordJob
{
	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_FleshbeastAssault toil = new LordToil_FleshbeastAssault();
		stateGraph.AddToil(toil);
		return stateGraph;
	}
}
