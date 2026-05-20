using Verse;
using Verse.AI.Group;

namespace RimWorld;

public abstract class LordJob_EntitySwarm : LordJob
{
	private IntVec3 startPos;

	private IntVec3 destPos;

	public LordJob_EntitySwarm()
	{
	}

	public LordJob_EntitySwarm(IntVec3 startPos, IntVec3 destPos)
	{
		this.startPos = startPos;
		this.destPos = destPos;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil firstSource = (stateGraph.StartingToil = CreateTravelingToil(startPos, destPos));
		LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap();
		stateGraph.AddToil(lordToil_ExitMap);
		stateGraph.AddTransition(new Transition(firstSource, lordToil_ExitMap)
		{
			triggers = { (Trigger)new Trigger_Memo("TravelArrived") }
		});
		return stateGraph;
	}

	protected abstract LordToil CreateTravelingToil(IntVec3 start, IntVec3 dest);

	public override bool DutyActiveWhenDown(Pawn pawn)
	{
		return true;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref startPos, "startPos");
		Scribe_Values.Look(ref destPos, "destPos");
	}
}
