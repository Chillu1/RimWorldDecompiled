using Verse.AI.Group;

namespace RimWorld;

public class LordJob_SightstealerAssault : LordJob
{
	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil toil = new LordToil_SightstealerAssault();
		stateGraph.AddToil(toil);
		return stateGraph;
	}
}
