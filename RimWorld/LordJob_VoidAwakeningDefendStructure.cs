using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_VoidAwakeningDefendStructure : LordJob
{
	private IntVec3 defendSpot;

	private float defendRadius;

	public LordJob_VoidAwakeningDefendStructure()
	{
	}

	public LordJob_VoidAwakeningDefendStructure(IntVec3 defendSpot, float defendRadius)
	{
		this.defendSpot = defendSpot;
		this.defendRadius = defendRadius;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref defendSpot, "defendSpot");
		Scribe_Values.Look(ref defendRadius, "defendRadius", 0f);
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_DefendPoint lordToil_DefendPoint = new LordToil_DefendPoint(defendSpot, defendRadius);
		stateGraph.AddToil(lordToil_DefendPoint);
		LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_AssaultColony(Faction.OfEntities).CreateGraph()).StartingToil;
		Transition transition = new Transition(lordToil_DefendPoint, startingToil);
		transition.AddTrigger(new Trigger_PawnHarmed(0.33f));
		transition.AddTrigger(new Trigger_FractionPawnsLost(0.3f));
		stateGraph.AddTransition(transition);
		return stateGraph;
	}
}
