using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientBarracks : SketchResolver
{
	private static List<IntVec3> tmpCells = new List<IntVec3>();

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		if (parms.rect.HasValue)
		{
			return parms.sketch != null;
		}
		return false;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (!ModLister.CheckIdeology("Ancient barracks"))
		{
			return;
		}
		CellRect rect = parms.rect.Value;
		SketchResolveParams parms2 = parms;
		parms2.cornerThing = ThingDefOf.AncientLamp;
		parms2.requireFloor = true;
		SketchResolverDefOf.AddCornerThings.Resolve(parms2);
		ThingDef ancientBed = ThingDefOf.AncientBed;
		CellRect cellRect = SketchGenUtility.FindBiggestRect(parms.sketch, (IntVec3 p) => rect.Contains(p) && !parms.sketch.ThingsAt(p).Any());
		if (cellRect == CellRect.Empty)
		{
			return;
		}
		SketchResolveParams parms3 = parms;
		parms3.chance = 1f;
		parms3.wallEdgeThing = ThingDefOf.AncientLockerBank;
		SketchResolverDefOf.AddWallEdgeThings.Resolve(parms3);
		tmpCells.Clear();
		if (cellRect.Width > cellRect.Height)
		{
			tmpCells.AddRange(cellRect.GetEdgeCells(Rot4.North));
			tmpCells.AddRange(cellRect.GetEdgeCells(Rot4.South));
			foreach (IntVec3 tmpCell in tmpCells)
			{
				if (CanPlaceBedAt(ancientBed, tmpCell, Rot4.North, parms.sketch))
				{
					parms.sketch.AddThing(ancientBed, tmpCell, Rot4.North, null, 1, null, null, wipeIfCollides: false);
				}
				if (CanPlaceBedAt(ancientBed, tmpCell, Rot4.South, parms.sketch))
				{
					parms.sketch.AddThing(ancientBed, tmpCell, Rot4.South, null, 1, null, null, wipeIfCollides: false);
				}
			}
		}
		else
		{
			tmpCells.AddRange(cellRect.GetEdgeCells(Rot4.East));
			tmpCells.AddRange(cellRect.GetEdgeCells(Rot4.West));
			foreach (IntVec3 tmpCell2 in tmpCells)
			{
				if (CanPlaceBedAt(ancientBed, tmpCell2, Rot4.East, parms.sketch))
				{
					parms.sketch.AddThing(ancientBed, tmpCell2, Rot4.East, null, 1, null, null, wipeIfCollides: false);
				}
				if (CanPlaceBedAt(ancientBed, tmpCell2, Rot4.West, parms.sketch))
				{
					parms.sketch.AddThing(ancientBed, tmpCell2, Rot4.West, null, 1, null, null, wipeIfCollides: false);
				}
			}
		}
		tmpCells.Clear();
	}

	private bool CanPlaceBedAt(ThingDef def, IntVec3 position, Rot4 rot, Sketch sketch)
	{
		CellRect cellRect = GenAdj.OccupiedRect(position, rot, def.size);
		foreach (IntVec3 cell in cellRect.Cells)
		{
			if (sketch.ThingsAt(cell).Any((SketchThing x) => x.def == ThingDefOf.Wall))
			{
				return false;
			}
		}
		bool result = false;
		foreach (IntVec3 edgeCell in cellRect.ExpandedBy(1).EdgeCells)
		{
			foreach (SketchThing item in sketch.ThingsAt(edgeCell))
			{
				if (item.def == ThingDefOf.Wall)
				{
					result = true;
					continue;
				}
				return false;
			}
		}
		return result;
	}
}
