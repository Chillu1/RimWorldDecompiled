using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualToil_SummonAnimals : PsychicRitualToil
{
	private PsychicRitualRoleDef invokerRole;

	private static readonly IntRange AnimalsDelayTicks = new IntRange(2500, 7500);

	protected PsychicRitualToil_SummonAnimals()
	{
	}

	public PsychicRitualToil_SummonAnimals(PsychicRitualRoleDef invokerRole)
	{
		this.invokerRole = invokerRole;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.Start(psychicRitual, parent);
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		float manhunterChance = ((PsychicRitualDef_SummonAnimals)psychicRitual.def).manhunterSpawnChanceFromQualityCurve.Evaluate(psychicRitual.PowerPercent);
		psychicRitual.ReleaseAllPawnsAndBuildings();
		if (pawn != null)
		{
			ApplyOutcome(psychicRitual, pawn, manhunterChance);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, float manhunterChance)
	{
		IncidentParms incidentParms = new IncidentParms();
		incidentParms.target = invoker.Map;
		incidentParms.forced = true;
		LetterDef textLetterDef;
		TaggedString text;
		if (Rand.Chance(manhunterChance))
		{
			textLetterDef = LetterDefOf.ThreatBig;
			text = "SummonAnimalsFailureText".Translate(invoker, psychicRitual.def.Named("RITUAL"));
			incidentParms.points = StorytellerUtility.DefaultThreatPointsNow(invoker.Map);
			Find.Storyteller.incidentQueue.Add(IncidentDefOf.FrenziedAnimals, Find.TickManager.TicksGame + AnimalsDelayTicks.RandomInRange, incidentParms);
		}
		else
		{
			textLetterDef = LetterDefOf.NeutralEvent;
			text = "SummonAnimalsSuccessText".Translate(invoker, psychicRitual.def.Named("RITUAL"));
			Find.Storyteller.incidentQueue.Add(IncidentDefOf.HerdMigration, Find.TickManager.TicksGame + AnimalsDelayTicks.RandomInRange, incidentParms);
		}
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), text, textLetterDef);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
	}
}
