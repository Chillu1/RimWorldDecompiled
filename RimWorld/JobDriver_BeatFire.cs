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
			JobDriver_BeatFire jobDriver_BeatFire = this;
			this.FailOnDespawnedOrNull(TargetIndex.A);
			Toil beat = new Toil();
			Toil approach = new Toil();
			approach.initAction = delegate
			{
				if (jobDriver_BeatFire.Map.reservationManager.CanReserve(jobDriver_BeatFire.pawn, jobDriver_BeatFire.TargetFire))
				{
					jobDriver_BeatFire.pawn.Reserve(jobDriver_BeatFire.TargetFire, jobDriver_BeatFire.job);
				}
				jobDriver_BeatFire.pawn.pather.StartPath(jobDriver_BeatFire.TargetFire, PathEndMode.Touch);
			};
			approach.tickAction = delegate
			{
				if (jobDriver_BeatFire.pawn.pather.Moving && jobDriver_BeatFire.pawn.pather.nextCell != jobDriver_BeatFire.TargetFire.Position)
				{
					jobDriver_BeatFire.StartBeatingFireIfAnyAt(jobDriver_BeatFire.pawn.pather.nextCell, beat);
				}
				if (jobDriver_BeatFire.pawn.Position != jobDriver_BeatFire.TargetFire.Position)
				{
					jobDriver_BeatFire.StartBeatingFireIfAnyAt(jobDriver_BeatFire.pawn.Position, beat);
				}
			};
			approach.FailOnDespawnedOrNull(TargetIndex.A);
			approach.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			approach.atomicWithPrevious = true;
			yield return approach;
			beat.tickAction = delegate
			{
				if (!jobDriver_BeatFire.pawn.CanReachImmediate(jobDriver_BeatFire.TargetFire, PathEndMode.Touch))
				{
					jobDriver_BeatFire.JumpToToil(approach);
				}
				else if (!(jobDriver_BeatFire.pawn.Position != jobDriver_BeatFire.TargetFire.Position) || !jobDriver_BeatFire.StartBeatingFireIfAnyAt(jobDriver_BeatFire.pawn.Position, beat))
				{
					jobDriver_BeatFire.pawn.natives.TryBeatFire(jobDriver_BeatFire.TargetFire);
					if (jobDriver_BeatFire.TargetFire.Destroyed)
					{
						jobDriver_BeatFire.pawn.records.Increment(RecordDefOf.FiresExtinguished);
						jobDriver_BeatFire.pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
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
