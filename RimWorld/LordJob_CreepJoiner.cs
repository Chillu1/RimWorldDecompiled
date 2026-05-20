using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_CreepJoiner : LordJob
{
	private IntVec3 idleSpot;

	private Pawn target;

	public LordJob_CreepJoiner()
	{
	}

	public LordJob_CreepJoiner(IntVec3 idleSpot, Pawn target)
	{
		this.idleSpot = idleSpot;
		this.target = target;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_Travel lordToil_Travel = new LordToil_Travel(idleSpot);
		stateGraph.AddToil(lordToil_Travel);
		stateGraph.StartingToil = lordToil_Travel;
		LordToil_WanderNearby lordToil_WanderNearby = new LordToil_WanderNearby();
		stateGraph.AddToil(lordToil_WanderNearby);
		LordToil_End toil = new LordToil_End();
		stateGraph.AddToil(toil);
		Transition transition = new Transition(lordToil_Travel, lordToil_WanderNearby);
		transition.AddTrigger(new Trigger_Memo("TravelArrived"));
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(lordToil_Travel, lordToil_WanderNearby);
		transition2.AddTrigger(new Trigger_Memo("SpokenTo"));
		transition2.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition2);
		Transition transition3 = new Transition(lordToil_WanderNearby, toil);
		transition3.AddSource(lordToil_Travel);
		transition3.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition3);
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref idleSpot, "idleSpot");
		Scribe_References.Look(ref target, "target");
	}
}
