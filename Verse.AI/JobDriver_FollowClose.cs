using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public class JobDriver_FollowClose : JobDriver
	{
		private const TargetIndex FolloweeInd = TargetIndex.A;

		private const int CheckPathIntervalTicks = 30;

		private Pawn Followee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

		private bool CurrentlyWalkingToFollowee
		{
			get
			{
				if (pawn.pather.Moving)
				{
					return pawn.pather.Destination == Followee;
				}
				return false;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			if (job.followRadius <= 0f)
			{
				Log.Error("Follow radius is <= 0. pawn=" + pawn.ToStringSafe());
				job.followRadius = 10f;
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				Pawn followee = Followee;
				float followRadius = job.followRadius;
				if (!pawn.pather.Moving || pawn.IsHashIntervalTick(30))
				{
					bool flag = false;
					if (CurrentlyWalkingToFollowee)
					{
						if (NearFollowee(pawn, followee, followRadius))
						{
							flag = true;
						}
					}
					else
					{
						float radius = followRadius * 1.2f;
						if (NearFollowee(pawn, followee, radius))
						{
							flag = true;
						}
						else
						{
							if (!pawn.CanReach(followee, PathEndMode.Touch, Danger.Deadly))
							{
								EndJobWith(JobCondition.Incompletable);
								return;
							}
							pawn.pather.StartPath(followee, PathEndMode.Touch);
							locomotionUrgencySameAs = null;
						}
					}
					if (flag)
					{
						if (NearDestinationOrNotMoving(pawn, followee, followRadius))
						{
							EndJobWith(JobCondition.Succeeded);
						}
						else
						{
							IntVec3 lastPassableCellInPath = followee.pather.LastPassableCellInPath;
							if (!pawn.pather.Moving || pawn.pather.Destination.HasThing || !pawn.pather.Destination.Cell.InHorDistOf(lastPassableCellInPath, followRadius))
							{
								IntVec3 intVec = CellFinder.RandomClosewalkCellNear(lastPassableCellInPath, base.Map, Mathf.FloorToInt(followRadius));
								if (intVec == pawn.Position)
								{
									EndJobWith(JobCondition.Succeeded);
								}
								else if (intVec.IsValid && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly))
								{
									pawn.pather.StartPath(intVec, PathEndMode.OnCell);
									locomotionUrgencySameAs = followee;
								}
								else
								{
									EndJobWith(JobCondition.Incompletable);
								}
							}
						}
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			yield return toil;
		}

		public override bool IsContinuation(Job j)
		{
			return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
		}

		public static bool FarEnoughAndPossibleToStartJob(Pawn follower, Pawn followee, float radius)
		{
			if (radius <= 0f)
			{
				string text = "Checking follow job with radius <= 0. pawn=" + follower.ToStringSafe();
				if (follower.mindState != null && follower.mindState.duty != null)
				{
					text = text + " duty=" + follower.mindState.duty.def;
				}
				Log.ErrorOnce(text, follower.thingIDNumber ^ 0x324308F9);
				return false;
			}
			if (!follower.CanReach(followee, PathEndMode.OnCell, Danger.Deadly))
			{
				return false;
			}
			float radius2 = radius * 1.2f;
			if (NearFollowee(follower, followee, radius2))
			{
				if (!NearDestinationOrNotMoving(follower, followee, radius2))
				{
					return follower.CanReach(followee.pather.LastPassableCellInPath, PathEndMode.OnCell, Danger.Deadly);
				}
				return false;
			}
			return true;
		}

		private static bool NearFollowee(Pawn follower, Pawn followee, float radius)
		{
			if (follower.Position.AdjacentTo8WayOrInside(followee.Position))
			{
				return true;
			}
			if (follower.Position.InHorDistOf(followee.Position, radius))
			{
				return GenSight.LineOfSight(follower.Position, followee.Position, follower.Map);
			}
			return false;
		}

		private static bool NearDestinationOrNotMoving(Pawn follower, Pawn followee, float radius)
		{
			if (!followee.pather.Moving)
			{
				return true;
			}
			IntVec3 lastPassableCellInPath = followee.pather.LastPassableCellInPath;
			if (!lastPassableCellInPath.IsValid)
			{
				return true;
			}
			if (follower.Position.AdjacentTo8WayOrInside(lastPassableCellInPath))
			{
				return true;
			}
			return follower.Position.InHorDistOf(lastPassableCellInPath, radius);
		}
	}
}
