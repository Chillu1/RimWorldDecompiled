using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_MarryAdjacentPawn : JobDriver
	{
		private int ticksLeftToMarry = 2500;

		private const TargetIndex OtherFianceInd = TargetIndex.A;

		private const int Duration = 2500;

		private Pawn OtherFiance => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		public int TicksLeftToMarry => ticksLeftToMarry;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => OtherFiance.Drafted || !pawn.Position.AdjacentTo8WayOrInside(OtherFiance));
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				ticksLeftToMarry = 2500;
			};
			toil.tickAction = delegate
			{
				ticksLeftToMarry--;
				if (ticksLeftToMarry <= 0)
				{
					ticksLeftToMarry = 0;
					ReadyForNextToil();
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.FailOn(() => !pawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, OtherFiance));
			yield return toil;
			Toil toil2 = new Toil();
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			toil2.initAction = delegate
			{
				if (pawn.thingIDNumber < OtherFiance.thingIDNumber)
				{
					MarriageCeremonyUtility.Married(pawn, OtherFiance);
				}
			};
			yield return toil2;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksLeftToMarry, "ticksLeftToMarry", 0);
		}
	}
}
