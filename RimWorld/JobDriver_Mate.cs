using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Mate : JobDriver
	{
		private const int MateDuration = 500;

		private const TargetIndex FemInd = TargetIndex.A;

		private const int TicksBetweenHeartMotes = 100;

		private Pawn Female => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnNotCasualInterruptible(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil toil = Toils_General.WaitWith(TargetIndex.A, 500);
			toil.tickAction = delegate
			{
				if (pawn.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, ThingDefOf.Mote_Heart);
				}
				if (Female.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(Female.Position, pawn.Map, ThingDefOf.Mote_Heart);
				}
			};
			yield return toil;
			yield return Toils_General.Do(delegate
			{
				PawnUtility.Mated(pawn, Female);
			});
		}
	}
}
