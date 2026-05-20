using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AddThingsCentral : SketchResolver
{
	private HashSet<IntVec3> processed = new HashSet<IntVec3>();

	private const float Chance = 0.4f;

	protected override void ResolveInt(SketchResolveParams parms)
	{
		CellRect outerRect = parms.rect ?? parms.sketch.OccupiedRect;
		bool allowWood = parms.allowWood ?? true;
		ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(parms.thingCentral, null, (ThingDef x) => SketchGenUtility.IsStuffAllowed(x, allowWood, parms.useOnlyStonesAvailableOnMap, allowFlammableWalls: true, parms.thingCentral));
		bool requireFloor = parms.requireFloor == true;
		processed.Clear();
		try
		{
			foreach (IntVec3 item in outerRect.Cells.InRandomOrder())
			{
				CellRect cellRect = SketchGenUtility.FindBiggestRectAt(item, outerRect, parms.sketch, processed, (IntVec3 x) => !parms.sketch.ThingsAt(x).Any() && (!requireFloor || (parms.sketch.TerrainAt(x) != null && parms.sketch.TerrainAt(x).layerable)));
				if (cellRect.Width >= parms.thingCentral.size.x + 2 && cellRect.Height >= parms.thingCentral.size.z + 2)
				{
					IntVec3 intVec = new IntVec3(cellRect.CenterCell.x - parms.thingCentral.size.x / 2, 0, cellRect.CenterCell.z - parms.thingCentral.size.z / 2);
					if (Rand.Chance(0.4f) && CanPlaceAt(parms.thingCentral, intVec, Rot4.North, parms.sketch))
					{
						parms.sketch.AddThing(parms.thingCentral, intVec, Rot4.North, stuff, 1, null, null, wipeIfCollides: false);
					}
				}
			}
		}
		finally
		{
			processed.Clear();
		}
	}

	private bool CanPlaceAt(ThingDef def, IntVec3 position, Rot4 rot, Sketch sketch)
	{
		foreach (IntVec3 item in GenAdj.OccupiedRect(position, Rot4.North, def.size).AdjacentCellsCardinal)
		{
			if (sketch.GetDoor(item) != null)
			{
				return false;
			}
		}
		return true;
	}

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return true;
	}
}
