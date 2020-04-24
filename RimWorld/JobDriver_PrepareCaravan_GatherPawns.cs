using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobDriver_PrepareCaravan_GatherPawns : JobDriver
	{
		private const TargetIndex AnimalOrSlaveInd = TargetIndex.A;

		private Pawn AnimalOrSlave => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(AnimalOrSlave, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !base.Map.lordManager.lords.Contains(job.lord));
			this.FailOn(() => AnimalOrSlave == null || AnimalOrSlave.GetLord() != job.lord);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A).FailOn(() => GatherAnimalsAndSlavesForCaravanUtility.IsFollowingAnyone(AnimalOrSlave));
			yield return SetFollowerToil();
		}

		private Toil SetFollowerToil()
		{
			return new Toil
			{
				initAction = delegate
				{
					GatherAnimalsAndSlavesForCaravanUtility.SetFollower(AnimalOrSlave, pawn);
					RestUtility.WakeUp(pawn);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
