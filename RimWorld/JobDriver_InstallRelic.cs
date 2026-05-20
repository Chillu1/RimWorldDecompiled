using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class JobDriver_InstallRelic : JobDriver
	{
		private const TargetIndex RelicInd = TargetIndex.A;

		private const TargetIndex ContainerInd = TargetIndex.B;

		private const TargetIndex ContainerInteractionCellInd = TargetIndex.C;

		private const int InstallTicks = 300;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed);
			}
			return false;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (ModLister.CheckIdeology("Relic"))
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOn((Toil to) => ReliquaryFull());
				yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOn((Toil to) => ReliquaryFull());
				yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.C).FailOn((Toil to) => ReliquaryFull());
				Toil toil = Toils_General.Wait(300, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B)
					.FailOn((Toil to) => ReliquaryFull());
				toil.handlingFacing = true;
				yield return toil;
				yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A, delegate
				{
					job.GetTarget(TargetIndex.A).Thing.def.soundDrop.PlayOneShot(new TargetInfo(job.GetTarget(TargetIndex.B).Cell, pawn.Map));
					SoundDefOf.Relic_Installed.PlayOneShot(new TargetInfo(job.GetTarget(TargetIndex.B).Cell, pawn.Map));
				});
			}
			bool ReliquaryFull()
			{
				return pawn.jobs.curJob.GetTarget(TargetIndex.B).Thing.TryGetComp<CompRelicContainer>()?.Full ?? true;
			}
		}
	}
}
