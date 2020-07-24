using System.Linq;
using Verse;

namespace RimWorld.SketchGen
{
	public class SketchResolver_Symmetry : SketchResolver
	{
		protected override void ResolveInt(ResolveParams parms)
		{
			bool num = parms.symmetryClear ?? true;
			int origin = parms.symmetryOrigin ?? 0;
			bool flag = parms.symmetryVertical ?? false;
			bool flag2 = parms.requireFloor ?? false;
			bool originIncluded = parms.symmetryOriginIncluded ?? false;
			if (num)
			{
				Clear(parms.sketch, origin, flag, originIncluded);
			}
			foreach (SketchBuildable item in parms.sketch.Buildables.ToList())
			{
				if (ShouldKeepAlreadySymmetricalInTheMiddle(item, origin, flag, originIncluded))
				{
					continue;
				}
				SketchBuildable sketchBuildable = (SketchBuildable)item.DeepCopy();
				SketchThing sketchThing = sketchBuildable as SketchThing;
				if (sketchThing != null && sketchThing.def.rotatable)
				{
					if (flag)
					{
						if (!sketchThing.rot.IsHorizontal)
						{
							sketchThing.rot = sketchThing.rot.Opposite;
						}
					}
					else if (sketchThing.rot.IsHorizontal)
					{
						sketchThing.rot = sketchThing.rot.Opposite;
					}
				}
				MoveUntilSymmetrical(sketchBuildable, item.OccupiedRect, origin, flag, originIncluded);
				if (flag2 && sketchBuildable.Buildable != ThingDefOf.Wall && sketchBuildable.Buildable != ThingDefOf.Door)
				{
					bool flag3 = true;
					foreach (IntVec3 item2 in sketchBuildable.OccupiedRect)
					{
						if (!parms.sketch.AnyTerrainAt(item2))
						{
							flag3 = false;
							break;
						}
					}
					if (flag3)
					{
						parms.sketch.Add(sketchBuildable);
					}
				}
				else
				{
					parms.sketch.Add(sketchBuildable);
				}
			}
		}

		protected override bool CanResolveInt(ResolveParams parms)
		{
			return true;
		}

		private void Clear(Sketch sketch, int origin, bool vertical, bool originIncluded)
		{
			foreach (SketchBuildable item in sketch.Buildables.ToList())
			{
				CellRect occupiedRect = item.OccupiedRect;
				if (((occupiedRect.maxX >= origin && !vertical) || (occupiedRect.maxZ >= origin && vertical)) && !ShouldKeepAlreadySymmetricalInTheMiddle(item, origin, vertical, originIncluded))
				{
					sketch.Remove(item);
				}
			}
		}

		private bool ShouldKeepAlreadySymmetricalInTheMiddle(SketchBuildable buildable, int origin, bool vertical, bool originIncluded)
		{
			CellRect occupiedRect = buildable.OccupiedRect;
			if (vertical)
			{
				if (originIncluded)
				{
					return occupiedRect.maxZ - origin == origin - occupiedRect.minZ;
				}
				return occupiedRect.maxZ - origin + 1 == origin - occupiedRect.minZ;
			}
			if (originIncluded)
			{
				return occupiedRect.maxX - origin == origin - occupiedRect.minX;
			}
			return occupiedRect.maxX - origin + 1 == origin - occupiedRect.minX;
		}

		private void MoveUntilSymmetrical(SketchBuildable buildable, CellRect initial, int origin, bool vertical, bool originIncluded)
		{
			if (vertical)
			{
				buildable.pos.x += initial.minX - buildable.OccupiedRect.minX;
				int num = (!originIncluded) ? (origin - initial.maxZ - 1 + origin) : (origin - initial.maxZ + origin);
				buildable.pos.z += num - buildable.OccupiedRect.minZ;
			}
			else
			{
				buildable.pos.z += initial.minZ - buildable.OccupiedRect.minZ;
				int num2 = (!originIncluded) ? (origin - initial.maxX - 1 + origin) : (origin - initial.maxX + origin);
				buildable.pos.x += num2 - buildable.OccupiedRect.minX;
			}
		}
	}
}
