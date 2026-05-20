using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Research : JobDriver
{
	private const int JobEndInterval = 4000;

	private ResearchProjectDef Project => Find.ResearchManager.GetProject();

	private Building_ResearchBench ResearchBench => (Building_ResearchBench)base.TargetThingA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(ResearchBench, job, 1, -1, null, errorOnFailed))
		{
			if (ResearchBench.def.hasInteractionCell)
			{
				return pawn.ReserveSittableOrSpot(ResearchBench.InteractionCell, job, errorOnFailed);
			}
			return true;
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		Toil research = ToilMaker.MakeToil("MakeNewToils");
		research.tickIntervalAction = delegate(int delta)
		{
			Pawn actor = research.actor;
			float statValue = actor.GetStatValue(StatDefOf.ResearchSpeed);
			statValue *= base.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor);
			Find.ResearchManager.ResearchPerformed(statValue * (float)delta, actor);
			actor.skills.Learn(SkillDefOf.Intellectual, 0.1f * (float)delta);
			actor.GainComfortFromCellIfPossible(delta, chairsOnly: true);
		};
		research.FailOn(() => Project == null);
		research.FailOn(() => !Project.CanBeResearchedAt(ResearchBench, ignoreResearchBenchPowerStatus: false));
		research.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
		research.WithEffect(EffecterDefOf.Research, TargetIndex.A);
		research.WithProgressBar(TargetIndex.A, () => Project?.ProgressPercent ?? 0f);
		research.defaultCompleteMode = ToilCompleteMode.Delay;
		research.defaultDuration = 4000;
		research.activeSkill = () => SkillDefOf.Intellectual;
		yield return research;
		yield return Toils_General.Wait(2);
	}
}
