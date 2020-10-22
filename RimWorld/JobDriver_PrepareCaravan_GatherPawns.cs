using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	[Obsolete]
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
			yield break;
		}

		[Obsolete]
		private Toil SetFollowerToil()
		{
			return null;
		}
	}
}
