using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_SlaveRebellion : LordJob
{
	private IntVec3 groupUpLoc;

	private IntVec3 exitPoint;

	private bool passive;

	private int sapperThingID = -1;

	private static FloatRange DesiredDamageRange = new FloatRange(0.25f, 0.35f);

	private const float MinDamage = 900f;

	private static readonly IntRange AssaultTimeBeforeGiveUp = new IntRange(26000, 38000);

	public override bool NeverInRestraints => true;

	public override bool AddFleeToil => false;

	public bool IsAggressiveRebellion => !passive;

	public LordJob_SlaveRebellion()
	{
	}

	public LordJob_SlaveRebellion(IntVec3 groupUpLoc, IntVec3 exitPoint, int sapperThingID, bool passive = true)
	{
		this.groupUpLoc = groupUpLoc;
		this.exitPoint = exitPoint;
		this.sapperThingID = sapperThingID;
		this.passive = passive;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		if (!ModLister.CheckIdeology("Slave rebellion"))
		{
			return stateGraph;
		}
		LordToil_Travel lordToil_Travel = new LordToil_Travel(groupUpLoc);
		lordToil_Travel.maxDanger = Danger.Deadly;
		lordToil_Travel.useAvoidGrid = true;
		stateGraph.StartingToil = lordToil_Travel;
		LordToil_AssaultColonyPrisoners lordToil_AssaultColonyPrisoners = new LordToil_AssaultColonyPrisoners();
		lordToil_AssaultColonyPrisoners.useAvoidGrid = true;
		stateGraph.AddToil(lordToil_AssaultColonyPrisoners);
		LordToil_PrisonerEscape lordToil_PrisonerEscape = new LordToil_PrisonerEscape(exitPoint, sapperThingID);
		lordToil_PrisonerEscape.useAvoidGrid = true;
		stateGraph.AddToil(lordToil_PrisonerEscape);
		LordToil_ExitMapFighting lordToil_ExitMapFighting = new LordToil_ExitMapFighting(LocomotionUrgency.Jog, canDig: true);
		lordToil_ExitMapFighting.useAvoidGrid = true;
		stateGraph.AddToil(lordToil_ExitMapFighting);
		LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, canDig: true);
		stateGraph.AddToil(lordToil_ExitMap);
		LordToil_ExitMap lordToil_ExitMap2 = new LordToil_ExitMap(LocomotionUrgency.Jog);
		lordToil_ExitMap2.useAvoidGrid = true;
		stateGraph.AddToil(lordToil_ExitMap2);
		if (!passive)
		{
			Transition transition = new Transition(lordToil_Travel, lordToil_AssaultColonyPrisoners);
			transition.AddTrigger(new Trigger_Memo("TravelArrived"));
			transition.AddTrigger(new Trigger_PawnHarmed());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_AssaultColonyPrisoners, lordToil_ExitMapFighting);
			transition2.AddTrigger(new Trigger_FractionColonyDamageTaken(DesiredDamageRange.RandomInRange, 900f));
			transition2.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_AssaultColonyPrisoners, lordToil_ExitMapFighting);
			transition3.AddTrigger(new Trigger_TicksPassed(AssaultTimeBeforeGiveUp.RandomInRange));
			transition3.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition3);
		}
		else
		{
			Transition transition4 = new Transition(lordToil_Travel, lordToil_PrisonerEscape);
			transition4.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_Travel, lordToil_PrisonerEscape);
			transition5.AddTrigger(new Trigger_PawnLost());
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_Travel, lordToil_ExitMap);
			transition6.AddSources(lordToil_AssaultColonyPrisoners, lordToil_PrisonerEscape, lordToil_ExitMapFighting);
			transition6.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			stateGraph.AddTransition(transition6);
			Transition transition7 = new Transition(lordToil_ExitMap, lordToil_PrisonerEscape);
			transition7.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition7.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition7);
		}
		Transition transition8 = new Transition(lordToil_PrisonerEscape, lordToil_ExitMap2);
		transition8.AddSource(lordToil_ExitMapFighting);
		transition8.AddTrigger(new Trigger_Memo("TravelArrived"));
		stateGraph.AddTransition(transition8);
		return stateGraph;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref groupUpLoc, "groupUpLoc");
		Scribe_Values.Look(ref exitPoint, "exitPoint");
		Scribe_Values.Look(ref sapperThingID, "sapperThingID", -1);
		Scribe_Values.Look(ref passive, "passive", defaultValue: false);
	}

	public override void Notify_PawnAdded(Pawn p)
	{
		ReachabilityUtility.ClearCacheFor(p);
	}

	public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
	{
		ReachabilityUtility.ClearCacheFor(p);
	}

	public override bool CanOpenAnyDoor(Pawn p)
	{
		return true;
	}

	public override bool ValidateAttackTarget(Pawn searcher, Thing target)
	{
		if (!(target is Pawn { MentalStateDef: var mentalStateDef }))
		{
			return true;
		}
		if (mentalStateDef == null)
		{
			return true;
		}
		return !mentalStateDef.escapingPrisonersIgnore;
	}
}
