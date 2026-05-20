using RimWorld;

namespace Verse.AI.Group;

public class PsychicRitualToil_SummonPitGate : PsychicRitualToil
{
	private PsychicRitualRoleDef invokerRole;

	private FloatRange combatPointMultiplierFromQualityRange;

	protected PsychicRitualToil_SummonPitGate()
	{
	}

	public PsychicRitualToil_SummonPitGate(PsychicRitualRoleDef invokerRole, FloatRange combatPointMultiplierFromQualityRange)
	{
		this.invokerRole = invokerRole;
		this.combatPointMultiplierFromQualityRange = combatPointMultiplierFromQualityRange;
	}

	public override void Start(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		base.Start(psychicRitual, parent);
		Pawn pawn = psychicRitual.assignments.FirstAssignedPawn(invokerRole);
		float combatPointMultiplier = combatPointMultiplierFromQualityRange.LerpThroughRange(psychicRitual.PowerPercent);
		psychicRitual.ReleaseAllPawnsAndBuildings();
		if (pawn != null)
		{
			ApplyOutcome(psychicRitual, pawn, combatPointMultiplier);
		}
	}

	private void ApplyOutcome(PsychicRitual psychicRitual, Pawn invoker, float combatPointMultiplier)
	{
		IncidentParms incidentParms = new IncidentParms();
		incidentParms.target = invoker.Map;
		incidentParms.pointMultiplier = combatPointMultiplier;
		incidentParms.forced = true;
		Find.Storyteller.incidentQueue.Add(IncidentDefOf.PitGate, Find.TickManager.TicksGame, incidentParms);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref invokerRole, "invokerRole");
		Scribe_Values.Look(ref combatPointMultiplierFromQualityRange, "combatPointMultiplierFromQualityRange");
	}
}
