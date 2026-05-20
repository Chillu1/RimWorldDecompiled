using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualToil_SummonShamblers : PsychicRitualToil
{
	private PsychicRitualRoleDef invokerRole;

	private FloatRange combatPointsFromQualityRange;

	private static readonly IntRange ShamblersDelayTicks = new IntRange(2500, 7500);

	protected PsychicRitualToil_SummonShamblers()
	{
	}

	public PsychicRitualToil_SummonShamblers(PsychicRitualRoleDef invokerRole, FloatRange combatPointsFromQualityRange)
	{
		this.invokerRole = invokerRole;
		this.combatPointsFromQualityRange = combatPointsFromQualityRange;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.Start(psychicRitual, parent);
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		float combatPoints = combatPointsFromQualityRange.LerpThroughRange(psychicRitual.PowerPercent);
		psychicRitual.ReleaseAllPawnsAndBuildings();
		if (pawn != null)
		{
			ApplyOutcome(psychicRitual, pawn, combatPoints);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, float combatPoints)
	{
		IncidentParms incidentParms = new IncidentParms();
		incidentParms.target = invoker.Map;
		incidentParms.points = combatPoints;
		incidentParms.forced = true;
		Find.Storyteller.incidentQueue.Add(IncidentDefOf.ShamblerSwarm, Find.TickManager.TicksGame + ShamblersDelayTicks.RandomInRange, incidentParms);
		Find.LetterStack.ReceiveLetter("PsychicRitualCompleteLabel".Translate(psychicRitual.def.label), "SummonShamblersCompleteText".Translate(invoker, psychicRitual.def.Named("RITUAL")), LetterDefOf.ThreatBig);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Values.Look(ref combatPointsFromQualityRange, "combatPointsFromQualityRange");
	}
}
