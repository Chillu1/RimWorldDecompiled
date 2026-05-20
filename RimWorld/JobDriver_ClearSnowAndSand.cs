using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ClearSnowAndSand : JobDriver
{
	private float workDone;

	private const float ClearWorkPerDepositedDepth = 50f;

	private float TotalNeededWork => 50f * (base.Map.snowGrid.GetDepth(base.TargetLocA) + base.Map.sandGrid?.GetDepth(base.TargetLocA)).GetValueOrDefault();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
		Toil clearToil = ToilMaker.MakeToil("MakeNewToils");
		clearToil.tickIntervalAction = delegate
		{
			float statValue = clearToil.actor.GetStatValue(StatDefOf.GeneralLaborSpeed);
			workDone += statValue;
			if (workDone >= TotalNeededWork)
			{
				base.Map.snowGrid.SetDepth(base.TargetLocA, 0f);
				base.Map.sandGrid?.SetDepth(base.TargetLocA, 0f);
				ReadyForNextToil();
			}
		};
		EffecterDef effectDef = (IsSand(base.TargetLocA) ? EffecterDefOf.ClearSand : EffecterDefOf.ClearSnow);
		clearToil.defaultCompleteMode = ToilCompleteMode.Never;
		clearToil.WithEffect(effectDef, TargetIndex.A);
		clearToil.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
		clearToil.WithProgressBar(TargetIndex.A, () => workDone / TotalNeededWork, interpolateBetweenActorAndTarget: true);
		clearToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		yield return clearToil;
	}

	private bool IsSand(IntVec3 cell)
	{
		float? num = base.Map?.sandGrid?.GetDepth(cell);
		if (num.HasValue)
		{
			return num.GetValueOrDefault() > base.Map?.snowGrid.GetDepth(cell);
		}
		return false;
	}

	public override string GetReport()
	{
		string reportStringOverride = GetReportStringOverride();
		if (!reportStringOverride.NullOrEmpty())
		{
			return reportStringOverride;
		}
		LocalTargetInfo localTargetInfo = (job.targetA.IsValid ? job.targetA : job.targetQueueA.FirstValid());
		if (localTargetInfo.Cell.IsValid && IsSand(localTargetInfo.Cell))
		{
			return "JobClearingSand".Translate();
		}
		return ReportStringProcessed(job.def.reportString);
	}
}
