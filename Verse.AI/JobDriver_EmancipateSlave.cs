using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class JobDriver_EmancipateSlave : JobDriver
	{
		private const TargetIndex SlaveInd = TargetIndex.A;

		private const TargetIndex BedInd = TargetIndex.B;

		private Pawn Slave => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Slave, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (ModLister.CheckIdeology("Emancipate slave"))
			{
				this.FailOnDestroyedOrNull(TargetIndex.A);
				this.FailOn(() => Slave.guest.slaveInteractionMode != SlaveInteractionModeDefOf.Emancipate);
				this.FailOnDowned(TargetIndex.A);
				this.FailOnAggroMentalState(TargetIndex.A);
				this.FailOnForbidden(TargetIndex.A);
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOn(() => !Slave.IsSlaveOfColony || !Slave.guest.SlaveIsSecure).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
				Toil toil = ToilMaker.MakeToil("MakeNewToils");
				toil.initAction = delegate
				{
					GenGuest.EmancipateSlave(pawn, Slave);
				};
				yield return toil;
			}
		}
	}
}
