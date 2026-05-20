using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_DefendCerebrexCore : LordJob
{
	private IntVec3 baseCenter;

	public override bool ShouldExistWithoutPawns => true;

	public LordJob_DefendCerebrexCore()
	{
	}

	public LordJob_DefendCerebrexCore(IntVec3 baseCenter)
	{
		this.baseCenter = baseCenter;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref baseCenter, "baseCenter");
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_DefendBase startingToil = new LordToil_DefendBase(baseCenter);
		stateGraph.StartingToil = startingToil;
		return stateGraph;
	}
}
