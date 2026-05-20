namespace Verse.AI.Group;

public class LordJob_TravelAndExit : LordJob
{
	private IntVec3 travelDest;

	public LordJob_TravelAndExit()
	{
	}

	public LordJob_TravelAndExit(IntVec3 travelDest)
	{
		this.travelDest = travelDest;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil firstSource = (stateGraph.StartingToil = stateGraph.AttachSubgraph(new LordJob_Travel(travelDest).CreateGraph()).StartingToil);
		LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap();
		stateGraph.AddToil(lordToil_ExitMap);
		stateGraph.AddTransition(new Transition(firstSource, lordToil_ExitMap)
		{
			triggers = { (Trigger)new Trigger_Memo("TravelArrived") }
		});
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref travelDest, "travelDest");
	}
}
