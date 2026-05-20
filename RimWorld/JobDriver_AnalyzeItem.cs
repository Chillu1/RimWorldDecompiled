using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_AnalyzeItem : JobDriver_StudyItem
{
	public CompAnalyzable AnalyzableComp => base.ThingToStudy.TryGetComp<CompAnalyzable>();

	protected override IEnumerable<Toil> GetStudyToils()
	{
		int num = Mathf.CeilToInt((float)Mathf.CeilToInt(AnalyzableComp.Props.analysisDurationHours * 2500f) / pawn.GetStatValue(StatDefOf.ResearchSpeed));
		Toil toil = Toils_General.Wait(num).FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch).WithProgressBarToilDelay(TargetIndex.A, num);
		toil.activeSkill = () => SkillDefOf.Intellectual;
		toil.handlingFacing = true;
		toil.tickIntervalAction = delegate
		{
			pawn.rotationTracker.FaceTarget(job.GetTarget(TargetIndex.A));
		};
		yield return toil;
		yield return Toils_General.DoAtomic(delegate
		{
			AnalyzableComp.OnAnalyzed(pawn);
		});
	}
}
