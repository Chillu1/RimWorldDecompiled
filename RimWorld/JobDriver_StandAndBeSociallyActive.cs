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

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = new Toil();
			toil.tickAction = delegate
			{
				Pawn pawn = FindClosePawn();
				if (pawn != null)
				{
					base.pawn.rotationTracker.FaceCell(pawn.Position);
				}
				base.pawn.GainComfortFromCellIfPossible();
			};
			toil.socialMode = RandomSocialMode.SuperActive;
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			toil.handlingFacing = true;
			yield return toil;
		}

		private Pawn FindClosePawn()
		{
			IntVec3 position = pawn.Position;
			for (int i = 0; i < 24; i++)
			{
				IntVec3 intVec = position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(base.Map))
				{
					Thing thing = intVec.GetThingList(base.Map).Find((Thing x) => x is Pawn);
					if (thing != null && thing != pawn && GenSight.LineOfSight(position, intVec, base.Map))
					{
						return (Pawn)thing;
					}
				}
			}
			return null;
		}
	}
}
