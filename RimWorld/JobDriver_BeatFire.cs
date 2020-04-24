using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_BeatFire : JobDriver
	{
		protected Fire TargetFire => (Fire)job.targetA.Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			Toil beat = new Toil();
			Toil approach = new Toil();
			approach.initAction = delegate
			{
				if (base.Map.reservationManager.CanReserve(pawn, TargetFire))
				{
					pawn.Reserve(TargetFire, job);
				}
				pawn.pather.StartPath(TargetFire, PathEndMode.Touch);
			};
			approach.tickAction = delegate
			{
				if (pawn.pather.Moving && pawn.pather.nextCell != TargetFire.Position)
				{
					StartBeatingFireIfAnyAt(pawn.pather.nextCell, beat);
				}
				if (pawn.Position != TargetFire.Position)
				{
					StartBeatingFireIfAnyAt(pawn.Position, beat);
				}
			};
			approach.FailOnDespawnedOrNull(TargetIndex.A);
			approach.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			approach.atomicWithPrevious = true;
			yield return approach;
			beat.tickAction = delegate
			{
				if (!pawn.CanReachImmediate(TargetFire, PathEndMode.Touch))
				{
					JumpToToil(approach);
				}
				else if (!(pawn.Position != TargetFire.Position) || !StartBeatingFireIfAnyAt(pawn.Position, beat))
				{
					pawn.natives.TryBeatFire(TargetFire);
					if (TargetFire.Destroyed)
					{
						pawn.records.Increment(RecordDefOf.FiresExtinguished);
						pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
					}
				}
			};
			beat.FailOnDespawnedOrNull(TargetIndex.A);
			beat.defaultCompleteMode = ToilCompleteMode.Never;
			yield return beat;
		}

		private bool StartBeatingFireIfAnyAt(IntVec3 cell, Toil nextToil)
		{
			List<Thing> thingList = cell.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Fire fire = thingList[i] as Fire;
				if (fire != null && fire.parent == null)
				{
					job.targetA = fire;
					pawn.pather.StopDead();
					JumpToToil(nextToil);
					return true;
				}
			}
			return false;
		}
	}
}
