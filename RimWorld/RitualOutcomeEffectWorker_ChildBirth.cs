using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RitualOutcomeEffectWorker_ChildBirth : RitualOutcomeEffectWorker_FromQuality
{
	public override bool ShowQuality => !Find.Storyteller.difficulty.babiesAreHealthy;

	public override string Description
	{
		get
		{
			if (!Find.Storyteller.difficulty.babiesAreHealthy)
			{
				return base.Description;
			}
			return "";
		}
	}

	public RitualOutcomeEffectWorker_ChildBirth()
	{
	}

	public RitualOutcomeEffectWorker_ChildBirth(RitualOutcomeEffectDef def)
		: base(def)
	{
	}

	public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
	{
		if (progress != 0f)
		{
			float quality = GetQuality(jobRitual, progress);
			RitualOutcomePossibility outcome = GetOutcome(quality, jobRitual);
			Pawn pawn = jobRitual.assignments.FirstAssignedPawn("mother");
			Pawn doctor = jobRitual.assignments.FirstAssignedPawn("doctor");
			Hediff_LaborPushing hediff_LaborPushing = (Hediff_LaborPushing)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PregnancyLaborPushing);
			PregnancyUtility.ApplyBirthOutcome(outcome, quality, jobRitual.Ritual, hediff_LaborPushing.geneSet?.GenesListForReading, hediff_LaborPushing.Mother ?? pawn, pawn, hediff_LaborPushing.Father, doctor, jobRitual, jobRitual.assignments);
		}
	}

	public override RitualOutcomePossibility GetOutcome(float quality, LordJob_Ritual ritual)
	{
		if (Find.Storyteller.difficulty.babiesAreHealthy)
		{
			return def.BestOutcome;
		}
		return base.GetOutcome(quality, ritual);
	}
}
