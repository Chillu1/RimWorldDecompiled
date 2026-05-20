using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_Monument : SketchResolver
{
	private static readonly SimpleCurve OpenChancePerSizeCurve = new SimpleCurve
	{
		{ 0f, 1f },
		{ 3f, 0.85f },
		{ 6f, 0.2f },
		{ 8f, 0f }
	};

	private static HashSet<IntVec3> tmpSeen = new HashSet<IntVec3>();

	private static List<IntVec3> tmpCellsToCheck = new List<IntVec3>();

	private static List<IntVec3> extraRoots = new List<IntVec3>();

	protected override void ResolveInt(SketchResolveParams parms)
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
		bool flag = ((!parms.monumentOpen.HasValue) ? Rand.Chance(OpenChancePerSizeCurve.Evaluate(Mathf.Max(width, height))) : parms.monumentOpen.Value);
		Sketch monument = new Sketch();
		bool onlyBuildableByPlayer = parms.onlyBuildableByPlayer == true;
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
			verticalSymmetry = !horizontalSymmetry || Rand.Bool;
			bool[,] shape = AbstractShapeGenerator.Generate(width - 2, height - 2, horizontalSymmetry, verticalSymmetry, allTruesMustBeConnected: true);
			Func<int, int, bool> func = (int x, int z) => x >= 0 && z >= 0 && x < shape.GetLength(0) && z < shape.GetLength(1) && shape[x, z];
			for (int num2 = -1; num2 < shape.GetLength(0) + 1; num2++)
			{
				for (int num3 = -1; num3 < shape.GetLength(1) + 1; num3++)
				{
					if (!func(num2, num3) && (func(num2 - 1, num3) || func(num2, num3 - 1) || func(num2, num3 + 1) || func(num2 + 1, num3) || func(num2 - 1, num3 - 1) || func(num2 - 1, num3 + 1) || func(num2 + 1, num3 - 1) || func(num2 + 1, num3 + 1)))
					{
						int newX = num2 + 1;
						int newZ = num3 + 1;
						monument.AddThing(ThingDefOf.Wall, new IntVec3(newX, 0, newZ), Rot4.North, ThingDefOf.WoodLog);
					}
				}
			}
			for (int num4 = -1; num4 < shape.GetLength(0) + 1; num4++)
			{
				for (int num5 = -1; num5 < shape.GetLength(1) + 1; num5++)
				{
					if (!func(num4, num5) && (func(num4 - 1, num5) || func(num4, num5 - 1) || func(num4, num5 + 1) || func(num4 + 1, num5)))
					{
						int num6 = num4 + 1;
						int num7 = num5 + 1;
						if ((!func(num4 - 1, num5) && monument.Passable(new IntVec3(num6 - 1, 0, num7))) || (!func(num4, num5 - 1) && monument.Passable(new IntVec3(num6, 0, num7 - 1))) || (!func(num4, num5 + 1) && monument.Passable(new IntVec3(num6, 0, num7 + 1))) || (!func(num4 + 1, num5) && monument.Passable(new IntVec3(num6 + 1, 0, num7))))
						{
							list.Add(new IntVec3(num6, 0, num7));
						}
					}
				}
			}
		}
		SketchResolveParams parms2 = parms;
		parms2.sketch = monument;
		parms2.connectedGroupsSameStuff = true;
		parms2.assignRandomStuffTo = ThingDefOf.Wall;
		SketchResolverDefOf.AssignRandomStuff.Resolve(parms2);
		if (parms.addFloors ?? true)
		{
			SketchResolveParams parms3 = parms;
			parms3.singleFloorType = true;
			parms3.sketch = monument;
			parms3.floorFillRoomsOnly = !flag;
			parms3.onlyStoneFloors = parms.onlyStoneFloors ?? true;
			parms3.allowConcrete = parms.allowConcrete == true;
			parms3.rect = new CellRect(0, 0, width, height);
			SketchResolverDefOf.FloorFill.Resolve(parms3);
		}
		if (CanUse(ThingDefOf.Column))
		{
			SketchResolveParams parms4 = parms;
			parms4.rect = new CellRect(0, 0, width, height);
			parms4.sketch = monument;
			parms4.requireFloor = true;
			SketchResolverDefOf.AddColumns.Resolve(parms4);
		}
		TryPlaceFurniture(parms, monument, CanUse);
		for (int num8 = 0; num8 < 2; num8++)
		{
			SketchResolveParams parms5 = parms;
			parms5.addFloors = false;
			parms5.sketch = monument;
			parms5.rect = new CellRect(0, 0, width, height);
			SketchResolverDefOf.AddInnerMonuments.Resolve(parms5);
		}
		bool? allowMonumentDoors = parms.allowMonumentDoors;
		int num9;
		if (!allowMonumentDoors.HasValue)
		{
			if (filterAllowsAll)
			{
				num9 = 1;
				goto IL_06a9;
			}
			num9 = (parms.allowedMonumentThings.Allows(ThingDefOf.Door) ? 1 : 0);
		}
		else
		{
			num9 = ((allowMonumentDoors == true) ? 1 : 0);
		}
		if (num9 != 0)
		{
			goto IL_06a9;
		}
		goto IL_0743;
		IL_0743:
		TryPlaceFurniture(parms, monument, CanUse);
		ApplySymmetry(parms, horizontalSymmetry, verticalSymmetry, monument, width, height);
		if (num9 != 0 && !flag && !monument.Things.Any((SketchThing x) => x.def == ThingDefOf.Door) && monument.Things.Where((SketchThing t) => IsWallBorderingEdge(monument, t)).TryRandomElement(out var result))
		{
			SketchThing sketchThing = monument.ThingsAt(result.pos).FirstOrDefault((SketchThing x) => x.def == ThingDefOf.Wall);
			if (sketchThing != null)
			{
				monument.Remove(sketchThing);
			}
			monument.AddThing(ThingDefOf.Door, result.pos, Rot4.North, result.Stuff);
		}
		TryAddDoorsToClosedRooms(monument);
		List<SketchThing> things = monument.Things;
		for (int num10 = 0; num10 < things.Count; num10++)
		{
			if (things[num10].def == ThingDefOf.Wall)
			{
				monument.RemoveTerrain(things[num10].pos);
			}
		}
		parms.sketch.MergeAt(monument, default(IntVec3), Sketch.SpawnPosType.OccupiedCenter);
		return;
		IL_06a9:
		if (list.Where((IntVec3 x) => (!horizontalSymmetry || x.x < width / 2) && (!verticalSymmetry || x.z < height / 2) && monument.ThingsAt(x).Any((SketchThing y) => y.def == ThingDefOf.Wall) && ((!monument.ThingsAt(new IntVec3(x.x - 1, x.y, x.z)).Any() && !monument.ThingsAt(new IntVec3(x.x + 1, x.y, x.z)).Any()) || (!monument.ThingsAt(new IntVec3(x.x, x.y, x.z - 1)).Any() && !monument.ThingsAt(new IntVec3(x.x, x.y, x.z + 1)).Any()))).TryRandomElement(out var result2))
		{
			SketchThing sketchThing2 = monument.ThingsAt(result2).FirstOrDefault((SketchThing x) => x.def == ThingDefOf.Wall);
			if (sketchThing2 != null)
			{
				monument.Remove(sketchThing2);
				monument.AddThing(ThingDefOf.Door, result2, Rot4.North, sketchThing2.Stuff);
			}
		}
		goto IL_0743;
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

	private bool IsWallBorderingEdge(Sketch monument, SketchThing sketchThing)
	{
		if (sketchThing.def == ThingDefOf.Wall)
		{
			if (!monument.Passable(sketchThing.pos.x - 1, sketchThing.pos.z) || !monument.Passable(sketchThing.pos.x + 1, sketchThing.pos.z) || monument.AnyTerrainAt(sketchThing.pos.x - 1, sketchThing.pos.z) == monument.AnyTerrainAt(sketchThing.pos.x + 1, sketchThing.pos.z))
			{
				if (monument.Passable(sketchThing.pos.x, sketchThing.pos.z - 1) && monument.Passable(sketchThing.pos.x, sketchThing.pos.z + 1))
				{
					return monument.AnyTerrainAt(sketchThing.pos.x, sketchThing.pos.z - 1) != monument.AnyTerrainAt(sketchThing.pos.x, sketchThing.pos.z + 1);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return true;
	}

	private void ApplySymmetry(SketchResolveParams parms, bool horizontalSymmetry, bool verticalSymmetry, Sketch monument, int width, int height)
	{
		if (horizontalSymmetry)
		{
			SketchResolveParams parms2 = parms;
			parms2.sketch = monument;
			parms2.symmetryVertical = false;
			parms2.symmetryOrigin = width / 2;
			parms2.symmetryOriginIncluded = width % 2 == 1;
			SketchResolverDefOf.Symmetry.Resolve(parms2);
		}
		if (verticalSymmetry)
		{
			SketchResolveParams parms3 = parms;
			parms3.sketch = monument;
			parms3.symmetryVertical = true;
			parms3.symmetryOrigin = height / 2;
			parms3.symmetryOriginIncluded = height % 2 == 1;
			SketchResolverDefOf.Symmetry.Resolve(parms3);
		}
	}

	private void TryPlaceFurniture(SketchResolveParams parms, Sketch monument, Func<ThingDef, bool> canUseValidator)
	{
		if (canUseValidator == null || canUseValidator(ThingDefOf.Urn))
		{
			SketchResolveParams parms2 = parms;
			parms2.sketch = monument;
			parms2.cornerThing = ThingDefOf.Urn;
			parms2.requireFloor = true;
			SketchResolverDefOf.AddCornerThings.Resolve(parms2);
		}
		if (canUseValidator == null || canUseValidator(ThingDefOf.SteleLarge))
		{
			SketchResolveParams parms3 = parms;
			parms3.sketch = monument;
			parms3.thingCentral = ThingDefOf.SteleLarge;
			parms3.requireFloor = true;
			SketchResolverDefOf.AddThingsCentral.Resolve(parms3);
		}
		if (canUseValidator == null || canUseValidator(ThingDefOf.SteleGrand))
		{
			SketchResolveParams parms4 = parms;
			parms4.sketch = monument;
			parms4.thingCentral = ThingDefOf.SteleGrand;
			parms4.requireFloor = true;
			SketchResolverDefOf.AddThingsCentral.Resolve(parms4);
		}
		if (canUseValidator == null || canUseValidator(ThingDefOf.Table1x2c))
		{
			SketchResolveParams parms5 = parms;
			parms5.sketch = monument;
			parms5.wallEdgeThing = ThingDefOf.Table1x2c;
			parms5.requireFloor = true;
			SketchResolverDefOf.AddWallEdgeThings.Resolve(parms5);
		}
		if (canUseValidator == null || canUseValidator(ThingDefOf.Table2x2c))
		{
			SketchResolveParams parms6 = parms;
			parms6.sketch = monument;
			parms6.thingCentral = ThingDefOf.Table2x2c;
			parms6.requireFloor = true;
			SketchResolverDefOf.AddThingsCentral.Resolve(parms6);
		}
		if (canUseValidator == null || canUseValidator(ThingDefOf.Sarcophagus))
		{
			SketchResolveParams parms7 = parms;
			parms7.sketch = monument;
			parms7.wallEdgeThing = ThingDefOf.Sarcophagus;
			parms7.requireFloor = true;
			parms7.thingCentral = ThingDefOf.Sarcophagus;
			SketchResolverDefOf.AddWallEdgeThings.Resolve(parms7);
			SketchResolverDefOf.AddThingsCentral.Resolve(parms7);
		}
	}

	private void TryAddDoorsToClosedRooms(Sketch sketch)
	{
		SketchThing sketchThing = sketch.Things.Where((SketchThing t) => t.def == ThingDefOf.Wall && IsWallBorderingEdge(sketch, t)).FirstOrDefault();
		if (sketchThing == null)
		{
			return;
		}
		tmpSeen.Clear();
		FloodFillFrom(sketchThing.pos);
		tmpCellsToCheck.Clear();
		tmpCellsToCheck.AddRange(sketch.OccupiedRect.Cells);
		foreach (IntVec3 item in tmpCellsToCheck)
		{
			if (!tmpSeen.Contains(item))
			{
				SketchThing sketchThing2 = (from t in sketch.ThingsAt(item)
					where t.def == ThingDefOf.Wall
					select t).FirstOrDefault();
				if (sketchThing2 != null && ((IsConnectedRoomCell(item + IntVec3.North) && IsClosedRoomCell(item + IntVec3.South)) || (IsConnectedRoomCell(item + IntVec3.South) && IsClosedRoomCell(item + IntVec3.North)) || (IsConnectedRoomCell(item + IntVec3.East) && IsClosedRoomCell(item + IntVec3.West)) || (IsConnectedRoomCell(item + IntVec3.West) && IsClosedRoomCell(item + IntVec3.East))))
				{
					sketch.AddThing(ThingDefOf.Door, sketchThing2.pos, Rot4.North, sketchThing2.Stuff);
					FloodFillFrom(sketchThing2.pos);
				}
			}
		}
		tmpCellsToCheck.Clear();
		tmpSeen.Clear();
		extraRoots.Clear();
		void FloodFillFrom(IntVec3 pos)
		{
			extraRoots.Clear();
			IntVec3[] cardinalDirectionsAndInside = GenAdj.CardinalDirectionsAndInside;
			foreach (IntVec3 intVec in cardinalDirectionsAndInside)
			{
				IntVec3 intVec2 = pos + intVec;
				if (sketch.Passable(intVec2))
				{
					extraRoots.Add(intVec2);
				}
			}
			Sketch sketch2 = sketch;
			Predicate<IntVec3> passCheck = (IntVec3 p) => !tmpSeen.Contains(p) && sketch.AnyTerrainAt(p) && sketch.Passable(p);
			Func<IntVec3, int, bool> processor = delegate(IntVec3 p, int d)
			{
				tmpSeen.Add(p);
				return false;
			};
			IEnumerable<IntVec3> enumerable = extraRoots;
			sketch2.FloodFill(pos, passCheck, processor, int.MaxValue, null, enumerable);
		}
		bool IsClosedRoomCell(IntVec3 cell)
		{
			if (!tmpSeen.Contains(cell) && sketch.AnyTerrainAt(cell))
			{
				return sketch.Passable(cell);
			}
			return false;
		}
		bool IsConnectedRoomCell(IntVec3 cell)
		{
			if (tmpSeen.Contains(cell))
			{
				return sketch.Passable(cell);
			}
			return false;
		}
	}
}
