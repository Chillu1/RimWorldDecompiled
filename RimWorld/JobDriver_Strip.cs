using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Strip : JobDriver
	{
		private const int StripTicks = 60;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnAggroMentalState(TargetIndex.A);
			this.FailOn(() => !StrippableUtility.CanBeStrippedByColony(base.TargetThingA));
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				pawn.pather.StartPath(base.TargetThingA, PathEndMode.ClosestTouch);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return toil;
			yield return Toils_General.Wait(60).WithProgressBarToilDelay(TargetIndex.A);
			Toil toil2 = new Toil();
			toil2.initAction = delegate
			{
				Thing thing = job.targetA.Thing;
				base.Map.designationManager.DesignationOn(thing, DesignationDefOf.Strip)?.Delete();
				(thing as IStrippable)?.Strip();
				pawn.records.Increment(RecordDefOf.BodiesStripped);
			};
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
		}

		public override object[] TaleParameters()
		{
			Corpse corpse = base.TargetA.Thing as Corpse;
			return new object[2]
			{
				pawn,
				(corpse != null) ? corpse.InnerPawn : base.TargetA.Thing
			};
		}
	}
}
