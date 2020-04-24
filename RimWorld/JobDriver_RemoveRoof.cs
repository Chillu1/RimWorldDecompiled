using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RemoveRoof : JobDriver_AffectRoof
	{
		private static List<IntVec3> removedRoofs = new List<IntVec3>();

		protected override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !base.Map.areaManager.NoRoof[base.Cell]);
			foreach (Toil item in base.MakeNewToils())
			{
				yield return item;
			}
		}

		protected override void DoEffect()
		{
			removedRoofs.Clear();
			base.Map.roofGrid.SetRoof(base.Cell, null);
			removedRoofs.Add(base.Cell);
			RoofCollapseCellsFinder.CheckCollapseFlyingRoofs(removedRoofs, base.Map, removalMode: true);
			removedRoofs.Clear();
		}

		protected override bool DoWorkFailOn()
		{
			return !base.Cell.Roofed(base.Map);
		}
	}
}
