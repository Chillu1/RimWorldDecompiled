using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_LoadAndEnterPortal : LordJob
{
	public MapPortal portal;

	public override bool AllowStartNewGatherings => false;

	public override bool AllowStartNewRituals => true;

	public override bool AddFleeToil => false;

	public LordJob_LoadAndEnterPortal()
	{
	}

	public LordJob_LoadAndEnterPortal(MapPortal portal)
	{
		this.portal = portal;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref portal, "portal");
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_LoadAndEnterPortal startingToil = new LordToil_LoadAndEnterPortal(portal);
		stateGraph.StartingToil = startingToil;
		LordToil_End toil = new LordToil_End();
		stateGraph.AddToil(toil);
		return stateGraph;
	}
}
