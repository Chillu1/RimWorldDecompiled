using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.SketchGen
{
	public class SketchResolver_Monument : SketchResolver
	{
		private const int MaxOpenSize = 15;

		private const int MinClosedSize = 8;

		protected override void ResolveInt(ResolveParams parms)
		{
			IntVec2 intVec;
			if (parms.monumentSize.HasValue)
			{
				intVec = parms.monumentSize.Value;
			}
			else
			{
				int num = Rand.Range(10, 50);
				intVec = new IntVec2(num, num);
			}
			int width = intVec.x;
			int height = intVec.z;
			bool flag = parms.monumentOpen.HasValue ? parms.monumentOpen.Value : (width < 8 || height < 8 || (width <= 15 && height <= 15 && Rand.Bool));
			Sketch monument = new Sketch();
			bool onlyBuildableByPlayer = parms.onlyBuildableByPlayer ?? false;
			bool filterAllowsAll = parms.allowedMonumentThings == null;
			List<IntVec3> list = new List<IntVec3>();
			bool horizontalSymmetry;
			bool verticalSymmetry;
			if (flag)
			{
				horizontalSymmetry = true;
				verticalSymmetry = true;
				bool[,] array = AbstractShapeGenerator.Generate(width, height, horizontalSymmetry, verticalSymmetry, allTruesMustBeConnected: false, allowEnclosedFalses: false, preferOutlines: true);
				for (int i = 0; i < array.GetLength(0); i++)
				{
					for (int j = 0; j < array.GetLength(1); j++)
					{
						if (array[i, j])
						{
							monument.AddThing(ThingDefOf.Wall, new IntVec3(i, 0, j), Rot4.North, ThingDefOf.WoodLog);
						}
					}
				}
			}
			else
			{
				horizontalSymmetry = Rand.Bool;
				verticalSymmetry = (!horizontalSymmetry || Rand.Bool);
				bool[,] shape = AbstractShapeGenerator.Generate(width - 2, height - 2, horizontalSymmetry, verticalSymmetry, allTruesMustBeConnected: true);
				Func<int, int, bool> func = (int x, int z) => x >= 0 && z >= 0 && x < shape.GetLength(0) && z < shape.GetLength(1) && shape[x, z];
				for (int k = -1; k < shape.GetLength(0) + 1; k++)
				{
					for (int l = -1; l < shape.GetLength(1) + 1; l++)
					{
						if (!func(k, l) && (func(k - 1, l) || func(k, l - 1) || func(k, l + 1) || func(k + 1, l) || func(k - 1, l - 1) || func(k - 1, l + 1) || func(k + 1, l - 1) || func(k + 1, l + 1)))
						{
							int newX = k + 1;
							int newZ = l + 1;
							monument.AddThing(ThingDefOf.Wall, new IntVec3(newX, 0, newZ), Rot4.North, ThingDefOf.WoodLog);
						}
					}
				}
				for (int m = -1; m < shape.GetLength(0) + 1; m++)
				{
					for (int n = -1; n < shape.GetLength(1) + 1; n++)
					{
						if (!func(m, n) && (func(m - 1, n) || func(m, n - 1) || func(m, n + 1) || func(m + 1, n)))
						{
							int num2 = m + 1;
							int num3 = n + 1;
							if ((!func(m - 1, n) && monument.Passable(new IntVec3(num2 - 1, 0, num3))) || (!func(m, n - 1) && monument.Passable(new IntVec3(num2, 0, num3 - 1))) || (!func(m, n + 1) && monument.Passable(new IntVec3(num2, 0, num3 + 1))) || (!func(m + 1, n) && monument.Passable(new IntVec3(num2 + 1, 0, num3))))
							{
								list.Add(new IntVec3(num2, 0, num3));
							}
						}
					}
				}
			}
			ResolveParams parms2 = parms;
			parms2.sketch = monument;
			parms2.connectedGroupsSameStuff = true;
			parms2.assignRandomStuffTo = ThingDefOf.Wall;
			SketchResolverDefOf.AssignRandomStuff.Resolve(parms2);
			ResolveParams parms3 = parms;
			parms3.sketch = monument;
			parms3.floorFillRoomsOnly = !flag;
			parms3.allowConcrete = (parms.allowConcrete ?? false);
			parms3.rect = new CellRect(0, 0, width, height);
			SketchResolverDefOf.FloorFill.Resolve(parms3);
			if (CanUse(ThingDefOf.Column))
			{
				ResolveParams parms4 = parms;
				parms4.rect = new CellRect(0, 0, width, height);
				parms4.sketch = monument;
				SketchResolverDefOf.AddColumns.Resolve(parms4);
			}
			if (CanUse(ThingDefOf.Urn))
			{
				ResolveParams parms5 = parms;
				parms5.sketch = monument;
				parms5.cornerThing = ThingDefOf.Urn;
				SketchResolverDefOf.AddCornerThings.Resolve(parms5);
			}
			if (CanUse(ThingDefOf.SteleLarge))
			{
				ResolveParams parms6 = parms;
				parms6.sketch = monument;
				parms6.thingCentral = ThingDefOf.SteleLarge;
				SketchResolverDefOf.AddThingsCentral.Resolve(parms6);
			}
			if (CanUse(ThingDefOf.SteleGrand))
			{
				ResolveParams parms7 = parms;
				parms7.sketch = monument;
				parms7.thingCentral = ThingDefOf.SteleGrand;
				SketchResolverDefOf.AddThingsCentral.Resolve(parms7);
			}
			if (CanUse(ThingDefOf.Table1x2c))
			{
				ResolveParams parms8 = parms;
				parms8.sketch = monument;
				parms8.wallEdgeThing = ThingDefOf.Table1x2c;
				parms8.requireFloor = true;
				SketchResolverDefOf.AddWallEdgeThings.Resolve(parms8);
			}
			if (CanUse(ThingDefOf.Sarcophagus))
			{
				ResolveParams parms9 = parms;
				parms9.sketch = monument;
				parms9.wallEdgeThing = ThingDefOf.Sarcophagus;
				parms9.requireFloor = true;
				SketchResolverDefOf.AddWallEdgeThings.Resolve(parms9);
			}
			for (int num4 = 0; num4 < 2; num4++)
			{
				ApplySymmetry(parms, horizontalSymmetry, verticalSymmetry, monument, width, height);
				ResolveParams parms10 = parms;
				parms10.sketch = monument;
				parms10.rect = new CellRect(0, 0, width, height);
				SketchResolverDefOf.AddInnerMonuments.Resolve(parms10);
			}
			if (CanUse(ThingDefOf.Table2x2c))
			{
				ResolveParams parms11 = parms;
				parms11.sketch = monument;
				parms11.thingCentral = ThingDefOf.Table2x2c;
				parms11.requireFloor = true;
				SketchResolverDefOf.AddThingsCentral.Resolve(parms11);
			}
			bool num5 = parms.allowMonumentDoors ?? (filterAllowsAll || parms.allowedMonumentThings.Allows(ThingDefOf.Door));
			if (num5 && list.Where((IntVec3 x) => (!horizontalSymmetry || x.x < width / 2) && (!verticalSymmetry || x.z < height / 2) && monument.ThingsAt(x).Any((SketchThing y) => y.def == ThingDefOf.Wall) && ((!monument.ThingsAt(new IntVec3(x.x - 1, x.y, x.z)).Any() && !monument.ThingsAt(new IntVec3(x.x + 1, x.y, x.z)).Any()) || (!monument.ThingsAt(new IntVec3(x.x, x.y, x.z - 1)).Any() && !monument.ThingsAt(new IntVec3(x.x, x.y, x.z + 1)).Any()))).TryRandomElement(out IntVec3 result))
			{
				SketchThing sketchThing = monument.ThingsAt(result).FirstOrDefault((SketchThing x) => x.def == ThingDefOf.Wall);
				if (sketchThing != null)
				{
					monument.Remove(sketchThing);
					monument.AddThing(ThingDefOf.Door, result, Rot4.North, sketchThing.Stuff);
				}
			}
			ApplySymmetry(parms, horizontalSymmetry, verticalSymmetry, monument, width, height);
			if (num5 && !flag && !monument.Things.Any((SketchThing x) => x.def == ThingDefOf.Door) && monument.Things.Where((SketchThing x) => x.def == ThingDefOf.Wall && ((monument.Passable(x.pos.x - 1, x.pos.z) && monument.Passable(x.pos.x + 1, x.pos.z) && monument.AnyTerrainAt(x.pos.x - 1, x.pos.z) != monument.AnyTerrainAt(x.pos.x + 1, x.pos.z)) || (monument.Passable(x.pos.x, x.pos.z - 1) && monument.Passable(x.pos.x, x.pos.z + 1) && monument.AnyTerrainAt(x.pos.x, x.pos.z - 1) != monument.AnyTerrainAt(x.pos.x, x.pos.z + 1)))).TryRandomElement(out SketchThing result2))
			{
				SketchThing sketchThing2 = monument.ThingsAt(result2.pos).FirstOrDefault((SketchThing x) => x.def == ThingDefOf.Wall);
				if (sketchThing2 != null)
				{
					monument.Remove(sketchThing2);
				}
				monument.AddThing(ThingDefOf.Door, result2.pos, Rot4.North, result2.Stuff);
			}
			List<SketchThing> things = monument.Things;
			for (int num6 = 0; num6 < things.Count; num6++)
			{
				if (things[num6].def == ThingDefOf.Wall)
				{
					monument.RemoveTerrain(things[num6].pos);
				}
			}
			parms.sketch.MergeAt(monument, default(IntVec3), Sketch.SpawnPosType.OccupiedCenter);
			bool CanUse(ThingDef def)
			{
				if (onlyBuildableByPlayer && !SketchGenUtility.PlayerCanBuildNow(def))
				{
					return false;
				}
				if (!filterAllowsAll && !parms.allowedMonumentThings.Allows(def))
				{
					return false;
				}
				return true;
			}
		}

		protected override bool CanResolveInt(ResolveParams parms)
		{
			return true;
		}

		private void ApplySymmetry(ResolveParams parms, bool horizontalSymmetry, bool verticalSymmetry, Sketch monument, int width, int height)
		{
			if (horizontalSymmetry)
			{
				ResolveParams parms2 = parms;
				parms2.sketch = monument;
				parms2.symmetryVertical = false;
				parms2.symmetryOrigin = width / 2;
				parms2.symmetryOriginIncluded = (width % 2 == 1);
				SketchResolverDefOf.Symmetry.Resolve(parms2);
			}
			if (verticalSymmetry)
			{
				ResolveParams parms3 = parms;
				parms3.sketch = monument;
				parms3.symmetryVertical = true;
				parms3.symmetryOrigin = height / 2;
				parms3.symmetryOriginIncluded = (height % 2 == 1);
				SketchResolverDefOf.Symmetry.Resolve(parms3);
			}
		}
	}
}
