using System.Linq;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_Symmetry : SketchResolver
{
	protected override void ResolveInt(SketchResolveParams parms)
	{
		bool num = parms.symmetryClear ?? true;
		int valueOrDefault = parms.symmetryOrigin.GetValueOrDefault();
		bool valueOrDefault2 = parms.symmetryVertical == true;
		bool valueOrDefault3 = parms.requireFloor == true;
		bool valueOrDefault4 = parms.symmetryOriginIncluded == true;
		if (num)
		{
			Clear(parms.sketch, valueOrDefault, valueOrDefault2, valueOrDefault4);
		}
		foreach (SketchBuildable item in parms.sketch.Buildables.ToList())
		{
			if (ShouldKeepAlreadySymmetricalInTheMiddle(item, valueOrDefault, valueOrDefault2, valueOrDefault4))
			{
				continue;
			}
			SketchBuildable sketchBuildable = (SketchBuildable)item.DeepCopy();
			if (sketchBuildable is SketchThing sketchThing && sketchThing.def.rotatable)
			{
				if (valueOrDefault2)
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
			MoveUntilSymmetrical(sketchBuildable, item.OccupiedRect, valueOrDefault, valueOrDefault2, valueOrDefault4);
			if (valueOrDefault3 && sketchBuildable.Buildable != ThingDefOf.Wall && sketchBuildable.Buildable != ThingDefOf.Door)
			{
				bool flag = true;
				foreach (IntVec3 item2 in sketchBuildable.OccupiedRect)
				{
					if (!parms.sketch.AnyTerrainAt(item2))
					{
						flag = false;
						break;
					}
				}
				if (flag)
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

	protected override bool CanResolveInt(SketchResolveParams parms)
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
			int num = ((!originIncluded) ? (origin - initial.maxZ - 1 + origin) : (origin - initial.maxZ + origin));
			buildable.pos.z += num - buildable.OccupiedRect.minZ;
		}
		else
		{
			buildable.pos.z += initial.minZ - buildable.OccupiedRect.minZ;
			int num2 = ((!originIncluded) ? (origin - initial.maxX - 1 + origin) : (origin - initial.maxX + origin));
			buildable.pos.x += num2 - buildable.OccupiedRect.minX;
		}
	}
}
