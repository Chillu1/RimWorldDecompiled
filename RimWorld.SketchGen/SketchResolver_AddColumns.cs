using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AddColumns : SketchResolver
{
	private List<CellRect> rects = new List<CellRect>();

	private HashSet<IntVec3> processed = new HashSet<IntVec3>();

	private const float Chance = 0.8f;

	protected override void ResolveInt(SketchResolveParams parms)
	{
		CellRect outerRect = parms.rect ?? parms.sketch.OccupiedRect;
		bool allowWood = parms.allowWood ?? true;
		bool valueOrDefault = parms.requireFloor == true;
		rects.Clear();
		processed.Clear();
		foreach (IntVec3 item2 in outerRect.Cells.InRandomOrder())
		{
			CellRect item = SketchGenUtility.FindBiggestRectAt(item2, outerRect, parms.sketch, processed, (IntVec3 x) => !AnyColumnBlockerAt(x, parms.sketch));
			if (!item.IsEmpty)
			{
				rects.Add(item);
			}
		}
		ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(ThingDefOf.Column, null, (ThingDef x) => SketchGenUtility.IsStuffAllowed(x, allowWood, parms.useOnlyStonesAvailableOnMap, allowFlammableWalls: true, ThingDefOf.Column));
		for (int num = 0; num < rects.Count; num++)
		{
			if (rects[num].Width < 3 || rects[num].Height < 3 || !Rand.Chance(0.8f))
			{
				continue;
			}
			CellRect cellRect = rects[num].ContractedBy(1);
			Sketch sketch = new Sketch();
			if (Rand.Bool)
			{
				int newZ = Rand.RangeInclusive(cellRect.minZ, cellRect.CenterCell.z);
				int num2 = ((cellRect.Width >= 4) ? Rand.Element(2, 3) : 2);
				for (int num3 = cellRect.minX; num3 <= cellRect.maxX; num3 += num2)
				{
					if (!valueOrDefault || parms.sketch.AnyTerrainAt(new IntVec3(num3, 0, newZ)))
					{
						sketch.AddThing(ThingDefOf.Column, new IntVec3(num3, 0, newZ), Rot4.North, stuff);
					}
				}
				SketchResolveParams parms2 = parms;
				parms2.sketch = sketch;
				parms2.symmetryOrigin = rects[num].minZ + rects[num].Height / 2;
				parms2.symmetryOriginIncluded = rects[num].Height % 2 == 1;
				SketchResolverDefOf.Symmetry.Resolve(parms2);
			}
			else
			{
				int newX = Rand.RangeInclusive(cellRect.minX, cellRect.CenterCell.x);
				int num4 = ((cellRect.Height >= 4) ? Rand.Element(2, 3) : 2);
				for (int num5 = cellRect.minZ; num5 <= cellRect.maxZ; num5 += num4)
				{
					if (!valueOrDefault || parms.sketch.AnyTerrainAt(new IntVec3(newX, 0, num5)))
					{
						sketch.AddThing(ThingDefOf.Column, new IntVec3(newX, 0, num5), Rot4.North, stuff);
					}
				}
				SketchResolveParams parms3 = parms;
				parms3.sketch = sketch;
				parms3.symmetryOrigin = rects[num].minX + rects[num].Width / 2;
				parms3.symmetryOriginIncluded = rects[num].Width % 2 == 1;
				SketchResolverDefOf.Symmetry.Resolve(parms3);
			}
			parms.sketch.Merge(sketch, wipeIfCollides: false);
		}
		rects.Clear();
		processed.Clear();
	}

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return true;
	}

	private bool AnyColumnBlockerAt(IntVec3 c, Sketch sketch)
	{
		return sketch.ThingsAt(c).Any((SketchThing x) => x.def.passability == Traversability.Impassable);
	}
}
