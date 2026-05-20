using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ReleaseMechs : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate
			{
				pawn.TryGetComp<CompMechCarrier>().TrySpawnPawns();
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
		}
	}
}
