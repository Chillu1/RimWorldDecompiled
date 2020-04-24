using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_BuildRoof : JobDriver_AffectRoof
	{
		private static List<IntVec3> builtRoofs = new List<IntVec3>();

		protected override PathEndMode PathEndMode => PathEndMode.Touch;

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !base.Map.areaManager.BuildRoof[base.Cell]);
			this.FailOn(() => !RoofCollapseUtility.WithinRangeOfRoofHolder(base.Cell, base.Map));
			this.FailOn(() => !RoofCollapseUtility.ConnectedToRoofHolder(base.Cell, base.Map, assumeRoofAtRoot: true));
			foreach (Toil item in base.MakeNewToils())
			{
				yield return item;
			}
		}

		protected override void DoEffect()
		{
			builtRoofs.Clear();
			for (int i = 0; i < 9; i++)
			{
				IntVec3 intVec = base.Cell + GenAdj.AdjacentCellsAndInside[i];
				if (intVec.InBounds(base.Map) && base.Map.areaManager.BuildRoof[intVec] && !intVec.Roofed(base.Map) && RoofCollapseUtility.WithinRangeOfRoofHolder(intVec, base.Map) && RoofUtility.FirstBlockingThing(intVec, base.Map) == null)
				{
					base.Map.roofGrid.SetRoof(intVec, RoofDefOf.RoofConstructed);
					MoteMaker.PlaceTempRoof(intVec, base.Map);
					builtRoofs.Add(intVec);
				}
			}
			builtRoofs.Clear();
		}

		protected override bool DoWorkFailOn()
		{
			return base.Cell.Roofed(base.Map);
		}
	}
}
