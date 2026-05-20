namespace Verse.AI.Group;

public class LordJob_DefendPoint : LordJob
{
	private IntVec3 point;

	private float? wanderRadius;

	private float? defendRadius;

	private bool isCaravanSendable;

	private bool addFleeToil;

	public override bool IsCaravanSendable => isCaravanSendable;

	public override bool AddFleeToil => addFleeToil;

	public LordJob_DefendPoint()
	{
	}

	public LordJob_DefendPoint(IntVec3 point, float? wanderRadius = null, float? defendRadius = null, bool isCaravanSendable = false, bool addFleeToil = true)
	{
		this.point = point;
		this.wanderRadius = wanderRadius;
		this.defendRadius = defendRadius;
		this.isCaravanSendable = isCaravanSendable;
		this.addFleeToil = addFleeToil;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		stateGraph.AddToil(new LordToil_DefendPoint(point, wanderRadius: wanderRadius, defendRadius: defendRadius));
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref point, "point");
		Scribe_Values.Look(ref wanderRadius, "wanderRadius");
		Scribe_Values.Look(ref defendRadius, "defendRadius");
		Scribe_Values.Look(ref isCaravanSendable, "isCaravanSendable", defaultValue: false);
		Scribe_Values.Look(ref addFleeToil, "addFleeToil", defaultValue: false);
	}
}
