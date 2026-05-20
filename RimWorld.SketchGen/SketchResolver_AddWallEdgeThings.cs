using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AddWallEdgeThings : SketchResolver
{
	private HashSet<IntVec3> processed = new HashSet<IntVec3>();

	private const float Chance = 0.2f;

	protected override void ResolveInt(SketchResolveParams parms)
	{
		CellRect outerRect = parms.rect ?? parms.sketch.OccupiedRect;
		bool allowWood = parms.allowWood ?? true;
		ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(parms.wallEdgeThing, null, (ThingDef x) => SketchGenUtility.IsStuffAllowed(x, allowWood, parms.useOnlyStonesAvailableOnMap, allowFlammableWalls: true, parms.wallEdgeThing));
		Rot4 rot = ((parms.wallEdgeThing.size.z > parms.wallEdgeThing.size.x) ? Rot4.North : Rot4.East);
		Rot4 rot2 = ((parms.wallEdgeThing.size.z > parms.wallEdgeThing.size.x) ? Rot4.East : Rot4.North);
		CellRect cellRect = GenAdj.OccupiedRect(default(IntVec3), rot, parms.wallEdgeThing.size);
		CellRect cellRect2 = GenAdj.OccupiedRect(default(IntVec3), rot2, parms.wallEdgeThing.size);
		bool requireFloor = parms.requireFloor == true;
		processed.Clear();
		try
		{
			foreach (IntVec3 item in outerRect.Cells.InRandomOrder())
			{
				CellRect cellRect3 = SketchGenUtility.FindBiggestRectAt(item, outerRect, parms.sketch, processed, (IntVec3 x) => !parms.sketch.ThingsAt(x).Any() && (!requireFloor || (parms.sketch.TerrainAt(x) != null && parms.sketch.TerrainAt(x).layerable)));
				if (cellRect3.Width < cellRect.Width || cellRect3.Height < cellRect.Height || cellRect3.Width < cellRect2.Width || cellRect3.Height < cellRect2.Height || !Rand.Chance(parms.chance ?? 0.2f))
				{
					continue;
				}
				CellRect rect = new CellRect(cellRect3.minX, cellRect3.CenterCell.z - cellRect.Height / 2, cellRect.Width, cellRect.Height);
				CellRect rect2 = new CellRect(cellRect3.maxX - (cellRect.Width - 1), cellRect3.CenterCell.z - cellRect.Height / 2, cellRect.Width, cellRect.Height);
				CellRect rect3 = new CellRect(cellRect3.CenterCell.x - cellRect2.Width / 2, cellRect3.maxZ - (cellRect2.Height - 1), cellRect2.Width, cellRect2.Height);
				CellRect rect4 = new CellRect(cellRect3.CenterCell.x - cellRect2.Width / 2, cellRect3.minZ, cellRect2.Width, cellRect2.Height);
				if ((Rand.Bool && CanPlaceAt(rect, Rot4.West, parms.sketch)) || CanPlaceAt(rect2, Rot4.East, parms.sketch))
				{
					if (Rand.Bool && CanPlaceAt(rect, Rot4.West, parms.sketch))
					{
						parms.sketch.AddThing(parms.wallEdgeThing, new IntVec3(rect.minX - cellRect.minX, 0, rect.minZ - cellRect.minZ), rot, stuff, 1, null, null, wipeIfCollides: false);
					}
					else if (CanPlaceAt(rect2, Rot4.East, parms.sketch))
					{
						parms.sketch.AddThing(parms.wallEdgeThing, new IntVec3(rect2.minX - cellRect.minX, 0, rect2.minZ - cellRect.minZ), rot.Opposite, stuff, 1, null, null, wipeIfCollides: false);
					}
				}
				else if (Rand.Bool && CanPlaceAt(rect3, Rot4.North, parms.sketch))
				{
					parms.sketch.AddThing(parms.wallEdgeThing, new IntVec3(rect3.minX - cellRect2.minX, 0, rect3.minZ - cellRect2.minZ), rot2.Opposite, stuff, 1, null, null, wipeIfCollides: false);
				}
				else if (CanPlaceAt(rect4, Rot4.South, parms.sketch))
				{
					parms.sketch.AddThing(parms.wallEdgeThing, new IntVec3(rect4.minX - cellRect2.minX, 0, rect4.minZ - cellRect2.minZ), rot2, stuff, 1, null, null, wipeIfCollides: false);
				}
			}
		}
		finally
		{
			processed.Clear();
		}
	}

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return true;
	}

	private bool CanPlaceAt(CellRect rect, Rot4 dir, Sketch sketch)
	{
		foreach (IntVec3 edgeCell in rect.GetEdgeCells(dir))
		{
			IntVec3 current = edgeCell;
			if (dir == Rot4.North)
			{
				current.z++;
			}
			else if (dir == Rot4.South)
			{
				current.z++;
			}
			else if (dir == Rot4.East)
			{
				current.x++;
			}
			else
			{
				current.x--;
			}
			if (!sketch.ThingsAt(current).Any((SketchThing x) => x.def == ThingDefOf.Wall))
			{
				return false;
			}
		}
		return true;
	}
}
