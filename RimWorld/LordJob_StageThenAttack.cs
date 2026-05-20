using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_StageThenAttack : LordJob
{
	private Faction faction;

	private IntVec3 stageLoc;

	private int raidSeed;

	private bool canTimeoutFlee;

	private bool canKidnap;

	private bool canSteal;

	private IntRange delay;

	public override bool GuiltyOnDowned => true;

	public LordJob_StageThenAttack()
	{
	}

	public LordJob_StageThenAttack(Faction faction, IntVec3 stageLoc, int raidSeed, bool canTimeoutFlee = true, bool canKidnap = true, bool canSteal = true, IntRange? delay = null)
	{
		this.faction = faction;
		this.stageLoc = stageLoc;
		this.raidSeed = raidSeed;
		this.canTimeoutFlee = canTimeoutFlee;
		this.canKidnap = canKidnap;
		this.canSteal = canSteal;
		this.delay = delay ?? new IntRange(5000, 15000);
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		LordToil_Stage lordToil_Stage = (LordToil_Stage)(stateGraph.StartingToil = new LordToil_Stage(stageLoc));
		LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction, canTimeoutOrFlee: canTimeoutFlee, canKidnap: canKidnap, sappers: false, useAvoidGridSmart: false, canSteal: canSteal).CreateGraph()).StartingToil;
		int tickLimit = Rand.RangeSeeded(delay.min, delay.max, raidSeed);
		Transition transition = new Transition(lordToil_Stage, startingToil);
		transition.AddTrigger(new Trigger_TicksPassed(tickLimit));
		transition.AddTrigger(new Trigger_FractionPawnsLost(0.3f));
		transition.AddPreAction(new TransitionAction_Message("MessageRaidersBeginningAssault".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name), MessageTypeDefOf.ThreatBig, "MessageRaidersBeginningAssault-" + raidSeed));
		transition.AddPostAction(new TransitionAction_WakeAll());
		stateGraph.AddTransition(transition);
		stateGraph.transitions.Find((Transition x) => x.triggers.Any((Trigger y) => y is Trigger_BecameNonHostileToPlayer)).AddSource(lordToil_Stage);
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref stageLoc, "stageLoc");
		Scribe_Values.Look(ref raidSeed, "raidSeed", 0);
		Scribe_Values.Look(ref canTimeoutFlee, "canTimeoutFlee", defaultValue: false);
		Scribe_Values.Look(ref canKidnap, "canKidnap", defaultValue: false);
		Scribe_Values.Look(ref canSteal, "canSteal", defaultValue: false);
		Scribe_Values.Look(ref delay, "delay");
	}
}
