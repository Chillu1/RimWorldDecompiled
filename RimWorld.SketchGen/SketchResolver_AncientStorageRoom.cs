using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientStorageRoom : SketchResolver
{
	private static float WallChance = 0.8f;

	private const float OilSmearChance = 0.15f;

	private static List<IntVec3> tmpCells = new List<IntVec3>();

	private static IEnumerable<ThingDef> PossibleBuildings
	{
		get
		{
			yield return ThingDefOf.AncientCrate;
			yield return ThingDefOf.AncientBarrel;
		}
	}

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
		if (!ModLister.CheckIdeology("Ancient storage room"))
		{
			return;
		}
		CellRect rect = parms.rect.Value;
		CellRect cellRect = SketchGenUtility.FindBiggestRect(parms.sketch, (IntVec3 p) => rect.Contains(p) && !parms.sketch.ThingsAt(p).Any());
		if (cellRect == CellRect.Empty)
		{
			return;
		}
		tmpCells.Clear();
		Rot4 dir = ((cellRect.Width > cellRect.Height) ? Rot4.North : Rot4.East);
		tmpCells.AddRange(cellRect.GetEdgeCells(dir));
		tmpCells.AddRange(cellRect.GetEdgeCells(dir.Opposite));
		ThingDef def = PossibleBuildings.RandomElement();
		foreach (IntVec3 tmpCell in tmpCells)
		{
			if (Rand.Chance(WallChance) && CanPlaceWallAdjacentAt(tmpCell, parms.sketch))
			{
				parms.sketch.AddThing(def, tmpCell, Rot4.North);
			}
		}
		tmpCells.Clear();
		foreach (IntVec3 cell in rect.Cells)
		{
			CellRect rect2 = CellRect.CenteredOn(cell, 2, 2);
			if (!CanPlaceAt(rect2, parms.sketch))
			{
				continue;
			}
			foreach (IntVec3 item in rect2)
			{
				ThingDef thingDef = PossibleBuildings.RandomElement();
				parms.sketch.AddThing(thingDef, item, Rot4.North);
				ScatterDebrisUtility.ScatterAround(item, thingDef.size, Rot4.North, parms.sketch, ThingDefOf.Filth_OilSmear, 0.15f);
			}
		}
	}

	private bool CanPlaceAt(CellRect rect, Sketch sketch)
	{
		foreach (IntVec3 cell in rect.Cells)
		{
			if (sketch.ThingsAt(cell).Any())
			{
				return false;
			}
		}
		foreach (IntVec3 edgeCell in rect.ExpandedBy(1).EdgeCells)
		{
			if (sketch.ThingsAt(edgeCell).Any((SketchThing t) => t.def != ThingDefOf.Wall && t.def != ThingDefOf.Door))
			{
				return false;
			}
		}
		return true;
	}

	private bool CanPlaceWallAdjacentAt(IntVec3 position, Sketch sketch)
	{
		bool result = false;
		IntVec3[] cardinalDirectionsAndInside = GenAdj.CardinalDirectionsAndInside;
		for (int i = 0; i < cardinalDirectionsAndInside.Length; i++)
		{
			IntVec3 pos = cardinalDirectionsAndInside[i] + position;
			foreach (SketchThing item in sketch.ThingsAt(pos))
			{
				if (item.def == ThingDefOf.Wall)
				{
					result = true;
				}
				else if (item.def == ThingDefOf.Door)
				{
					return false;
				}
			}
		}
		return result;
	}
}
