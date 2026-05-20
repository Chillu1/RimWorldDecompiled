using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_FleeAndCowerShort : JobDriver_Flee
{
	private const int CowerTicks = 30;

	public override string GetReport()
	{
		if (pawn.CurJob == job && pawn.Position == job.GetTarget(TargetIndex.A).Cell)
		{
			return "ReportCowering".Translate();
		}
		return base.GetReport();
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		foreach (Toil item in base.MakeNewToils())
		{
			yield return item;
		}
		yield return Toils_General.Wait(30);
	}
}
