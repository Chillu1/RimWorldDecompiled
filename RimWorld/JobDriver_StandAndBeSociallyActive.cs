using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_StandAndBeSociallyActive : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected virtual Toil GetGotoToil()
		{
			return Toils_Goto.GotoCell(base.TargetLocA, PathEndMode.OnCell);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (base.TargetLocA.IsValid)
			{
				yield return GetGotoToil();
			}
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate
			{
				pawn.pather.StopDead();
			};
			toil.tickIntervalAction = delegate(int delta)
			{
				base.pawn.pather.StopDead();
				if (!job.forceMaintainFacing)
				{
					if (job.lookDirection != Direction8Way.Invalid)
					{
						base.pawn.rotationTracker.Face(base.pawn.Position.ToVector3() + job.lookDirection.AsVector());
					}
					else
					{
						Pawn pawn = FindClosePawn(base.pawn);
						if (pawn != null)
						{
							base.pawn.rotationTracker.FaceCell(pawn.Position);
						}
					}
				}
				base.pawn.GainComfortFromCellIfPossible(delta);
			};
			toil.socialMode = RandomSocialMode.SuperActive;
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.handlingFacing = true;
			yield return toil;
		}

		public static Pawn FindClosePawn(Pawn pawn)
		{
			IntVec3 position = pawn.Position;
			Map map = pawn.Map;
			for (int i = 0; i < 24; i++)
			{
				IntVec3 intVec = position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map))
				{
					Thing thing = intVec.GetThingList(map).Find((Thing x) => x is Pawn);
					if (thing != null && thing != pawn && GenSight.LineOfSight(position, intVec, map))
					{
						return (Pawn)thing;
					}
				}
			}
			return null;
		}
	}
}
