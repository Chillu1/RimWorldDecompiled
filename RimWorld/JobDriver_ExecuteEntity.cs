using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ExecuteEntity : JobDriver
{
	private const TargetIndex PlatformIndex = TargetIndex.A;

	private Thing Platform => base.TargetThingA;

	private Pawn InnerPawn => (Platform as Building_HoldingPlatform)?.HeldPawn;

	public override string GetReport()
	{
		return JobUtility.GetResolvedJobReport(job.def.reportString, InnerPawn, job.targetB, job.targetC);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Platform, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(delegate
		{
			Pawn innerPawn = InnerPawn;
			if (innerPawn == null || innerPawn.Destroyed)
			{
				return true;
			}
			if (job.ignoreDesignations)
			{
				return false;
			}
			CompHoldingPlatformTarget compHoldingPlatformTarget = innerPawn.TryGetComp<CompHoldingPlatformTarget>();
			return compHoldingPlatformTarget == null || compHoldingPlatformTarget.containmentMode != EntityContainmentMode.Execute;
		});
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		Toil toil = Toils_General.Do(delegate
		{
			Messages.Message("MessageEntityExecuted".Translate(pawn.Named("EXECUTIONER"), InnerPawn.Named("VICTIM")), pawn, MessageTypeDefOf.NeutralEvent);
			ExecutionUtility.DoExecutionByCut(pawn, InnerPawn);
			pawn.MentalState?.Notify_SlaughteredTarget();
		});
		toil.activeSkill = () => SkillDefOf.Melee;
		yield return toil;
	}
}
