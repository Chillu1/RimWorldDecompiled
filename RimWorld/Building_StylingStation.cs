using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Building_StylingStation : Building
	{
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
		{
			foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
			{
				yield return floatMenuOption;
			}
			if (!ModLister.IdeologyInstalled)
			{
				yield break;
			}
			if (!selPawn.CanReach(this, PathEndMode.OnCell, Danger.Deadly))
			{
				yield return new FloatMenuOption("CannotUseReason".Translate("NoPath".Translate().CapitalizeFirst()), null);
			}
			else
			{
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ChangeStyle".Translate().CapitalizeFirst(), delegate
				{
					selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.OpenStylingStationDialog, this), JobTag.Misc);
				}), selPawn, this);
			}
			if (JobGiver_OptimizeApparel.TryCreateRecolorJob(selPawn, out var _, dryRun: true))
			{
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RecolorApparel".Translate().CapitalizeFirst(), delegate
				{
					JobGiver_OptimizeApparel.TryCreateRecolorJob(selPawn, out var job2);
					selPawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
				}), selPawn, this);
			}
		}
	}
}
