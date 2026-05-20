using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_ReimplantXenogerm : LordJob
{
	private IntVec3 idleSpot;

	private string inSignalReimplanted;

	private int waitDurationTicks;

	public LordJob_ReimplantXenogerm()
	{
	}

	public LordJob_ReimplantXenogerm(IntVec3 idleSpot, int waitDurationTicks, string inSignalReimplanted)
	{
		this.idleSpot = idleSpot;
		this.waitDurationTicks = waitDurationTicks;
		this.inSignalReimplanted = inSignalReimplanted;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		if (!ModLister.CheckBiotech("Xenogerm reimplantation reward"))
		{
			return stateGraph;
		}
		LordToil_Travel lordToil_Travel = new LordToil_Travel(idleSpot);
		stateGraph.AddToil(lordToil_Travel);
		stateGraph.StartingToil = lordToil_Travel;
		LordToil_ReimplantXenogerm lordToil_ReimplantXenogerm = new LordToil_ReimplantXenogerm(idleSpot);
		stateGraph.AddToil(lordToil_ReimplantXenogerm);
		LordToil_ExitMapRandom lordToil_ExitMapRandom = new LordToil_ExitMapRandom();
		stateGraph.AddToil(lordToil_ExitMapRandom);
		LordToil_ExitMapAndDefendSelf lordToil_ExitMapAndDefendSelf = new LordToil_ExitMapAndDefendSelf();
		stateGraph.AddToil(lordToil_ExitMapAndDefendSelf);
		Transition transition = new Transition(lordToil_Travel, lordToil_ReimplantXenogerm);
		transition.AddTrigger(new Trigger_Memo("TravelArrived"));
		transition.canMoveToSameState = true;
		stateGraph.AddTransition(transition);
		Transition transition2 = new Transition(lordToil_ReimplantXenogerm, lordToil_ExitMapRandom);
		transition2.AddSource(lordToil_Travel);
		transition2.AddTrigger(new Trigger_Signal(inSignalReimplanted));
		transition2.AddTrigger(new Trigger_TicksPassed(waitDurationTicks));
		transition2.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition2);
		Transition transition3 = new Transition(lordToil_ReimplantXenogerm, lordToil_ExitMapAndDefendSelf);
		transition3.AddSource(lordToil_Travel);
		transition3.AddSource(lordToil_ExitMapRandom);
		transition3.AddTrigger(new Trigger_BecamePlayerEnemy());
		transition3.AddTrigger(new Trigger_PawnKilled());
		transition3.AddPostAction(new TransitionAction_EndAllJobs());
		stateGraph.AddTransition(transition3);
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref idleSpot, "idleSpot");
		Scribe_Values.Look(ref inSignalReimplanted, "inSignalReimplanted");
		Scribe_Values.Look(ref waitDurationTicks, "waitDurationTicks", 0);
	}
}
