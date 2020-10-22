using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.SketchGen
{
	public class SketchResolver_AddColumns : SketchResolver
	{
		private List<CellRect> rects = new List<CellRect>();

		private HashSet<IntVec3> processed = new HashSet<IntVec3>();

		private const float Chance = 0.8f;

		protected override void ResolveInt(ResolveParams parms)
		{
			CellRect outerRect = parms.rect ?? parms.sketch.OccupiedRect;
			bool allowWood = parms.allowWood ?? true;
			bool flag = parms.requireFloor ?? false;
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
			for (int i = 0; i < rects.Count; i++)
			{
				if (rects[i].Width < 3 || rects[i].Height < 3 || !Rand.Chance(0.8f))
				{
					continue;
				}
				CellRect cellRect = rects[i].ContractedBy(1);
				Sketch sketch = new Sketch();
				if (Rand.Bool)
				{
					int newZ = Rand.RangeInclusive(cellRect.minZ, cellRect.CenterCell.z);
					int num = ((cellRect.Width >= 4) ? Rand.Element(2, 3) : 2);
					for (int j = cellRect.minX; j <= cellRect.maxX; j += num)
					{
						if (!flag || parms.sketch.AnyTerrainAt(new IntVec3(j, 0, newZ)))
						{
							sketch.AddThing(ThingDefOf.Column, new IntVec3(j, 0, newZ), Rot4.North, stuff);
						}
					}
					ResolveParams parms2 = parms;
					parms2.sketch = sketch;
					parms2.symmetryOrigin = rects[i].minZ + rects[i].Height / 2;
					parms2.symmetryOriginIncluded = rects[i].Height % 2 == 1;
					SketchResolverDefOf.Symmetry.Resolve(parms2);
				}
				else
				{
					int newX = Rand.RangeInclusive(cellRect.minX, cellRect.CenterCell.x);
					int num2 = ((cellRect.Height >= 4) ? Rand.Element(2, 3) : 2);
					for (int k = cellRect.minZ; k <= cellRect.maxZ; k += num2)
					{
						if (!flag || parms.sketch.AnyTerrainAt(new IntVec3(newX, 0, k)))
						{
							sketch.AddThing(ThingDefOf.Column, new IntVec3(newX, 0, k), Rot4.North, stuff);
						}
					}
					ResolveParams parms3 = parms;
					parms3.sketch = sketch;
					parms3.symmetryOrigin = rects[i].minX + rects[i].Width / 2;
					parms3.symmetryOriginIncluded = rects[i].Width % 2 == 1;
					SketchResolverDefOf.Symmetry.Resolve(parms3);
				}
				parms.sketch.Merge(sketch, wipeIfCollides: false);
			}
			rects.Clear();
			processed.Clear();
		}

		protected override bool CanResolveInt(ResolveParams parms)
		{
			return true;
		}

		private bool AnyColumnBlockerAt(IntVec3 c, Sketch sketch)
		{
			return sketch.ThingsAt(c).Any((SketchThing x) => x.def.passability == Traversability.Impassable);
		}
	}
}
