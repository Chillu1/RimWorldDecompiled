using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RepairMechRemote : JobDriver_RepairMech
	{
		protected override bool Remote => true;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => base.Mech.Position.DistanceTo(pawn.Position) > pawn.GetStatValue(StatDefOf.MechRemoteRepairDistance));
			foreach (Toil item in base.MakeNewToils())
			{
				yield return item;
			}
		}
	}
}
