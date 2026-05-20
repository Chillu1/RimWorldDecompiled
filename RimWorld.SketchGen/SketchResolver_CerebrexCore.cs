using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_CerebrexCore : SketchResolver
{
	public const int Size = 61;

	private const int DiamondRadius = 19;

	private const int MiddlePathWidth = 3;

	private const int BuildingWidth = 7;

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		return parms.sketch != null;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (!ModLister.CheckOdyssey("Cerebrex core"))
		{
			return;
		}
		Sketch sketch = new Sketch();
		sketch.AddThing(ThingDefOf.CerebrexCore, new IntVec3(0, 0, 0), Rot4.North);
		for (int i = 0; i < 61; i++)
		{
			for (int j = 0; j < 61; j++)
			{
				IntVec3 pos = new IntVec3(i - 30, 0, j - 30);
				if (pos.LengthManhattan <= 19)
				{
					sketch.AddTerrain(TerrainDefOf.AncientTile, pos);
				}
			}
		}
		for (int k = 0; k < 61; k++)
		{
			sketch.AddTerrain(TerrainDefOf.AncientTile, new IntVec3(k - 30, 0, -1));
			sketch.AddTerrain(TerrainDefOf.AncientTile, new IntVec3(k - 30, 0, 0));
			sketch.AddTerrain(TerrainDefOf.AncientTile, new IntVec3(k - 30, 0, 1));
			sketch.AddTerrain(TerrainDefOf.AncientTile, new IntVec3(-1, 0, k - 30));
			sketch.AddTerrain(TerrainDefOf.AncientTile, new IntVec3(0, 0, k - 30));
			sketch.AddTerrain(TerrainDefOf.AncientTile, new IntVec3(1, 0, k - 30));
		}
		sketch.AddPrefab(PrefabDefOf.CerebrexCore, new IntVec3(0, 0, 0), Rot4.North);
		foreach (Rot4 allRotation in Rot4.AllRotations)
		{
			GenerateBuilding(sketch, allRotation);
			AddBuildingPathway(sketch, allRotation);
		}
		parms.sketch.Merge(sketch);
	}

	private static int GenerateBuilding(Sketch sketch, Rot4 dir)
	{
		int num = Mathf.FloorToInt(30.5f);
		int num2 = num - Mathf.CeilToInt(1.5f);
		for (int i = 0; i < num2; i++)
		{
			sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - i, 0, num).RotatedBy(dir), Rot4.North);
			sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num, 0, num - i).RotatedBy(dir), Rot4.North);
			if (i < 7)
			{
				sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - i, 0, 2).RotatedBy(dir), Rot4.North);
				sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(2, 0, num - i).RotatedBy(dir), Rot4.North);
			}
			if (i >= 6)
			{
				sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - i, 0, num - 7 + 1).RotatedBy(dir), Rot4.North);
				sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - 7 + 1, 0, num - i).RotatedBy(dir), Rot4.North);
			}
			for (int j = 1; j < 6; j++)
			{
				sketch.AddTerrain(TerrainDefOf.Space, new IntVec3(num - i, 0, num - j).RotatedBy(dir));
				sketch.AddTerrain(TerrainDefOf.Space, new IntVec3(num - j, 0, num - i).RotatedBy(dir));
			}
		}
		for (int k = -2; k <= 2; k++)
		{
			sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(k, 0, num + 4).RotatedBy(dir), Rot4.North);
		}
		return num;
	}

	private static void AddBuildingPathway(Sketch sketch, Rot4 dir)
	{
		int num = Mathf.FloorToInt(30.5f);
		int num2 = 0;
		sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - 2, 0, num - 2).RotatedBy(dir), Rot4.North);
		int num3 = Rand.Range(14, 21);
		Rot4 rot = dir.Rotated(RotationDirection.Counterclockwise);
		for (int i = 0; i < num3; i++)
		{
			sketch.AddTerrain(TerrainDefOf.MechanoidPlatform, new IntVec3(num - 3 - i, 0, num - 3).RotatedBy(dir));
			sketch.AddTerrain(TerrainDefOf.MechanoidPlatform, new IntVec3(num - 3 - i, 0, num - 2).RotatedBy(dir));
			sketch.AddTerrain(TerrainDefOf.MechanoidPlatform, new IntVec3(num - 3 - i, 0, num - 4).RotatedBy(dir));
			sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - 3 - i, 0, num - 2).RotatedBy(dir), Rot4.North);
			if (i != 0)
			{
				sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - 3 - i, 0, num - 4).RotatedBy(dir), Rot4.North);
				if (i - num2 > 5 && i < num3 - 5 && Rand.Chance(0.05f))
				{
					num2 = i;
					AddConnection(sketch, new IntVec3(num - 3 - i, 0, num - 3).RotatedBy(dir), rot.Rotated(Rand.Bool ? RotationDirection.Clockwise : RotationDirection.Counterclockwise));
				}
			}
		}
		sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - 3 - num3 + 1, 0, num - 3).RotatedBy(dir), Rot4.North);
		Rot4 direction = rot.Rotated(Rand.Bool ? RotationDirection.Clockwise : RotationDirection.Counterclockwise);
		AddConnection(sketch, new IntVec3(num - 3 - num3 + 2, 0, num - 3).RotatedBy(dir), direction);
		if (Rand.Chance(0.25f))
		{
			AddConnection(sketch, new IntVec3(num - 3 - num3 + 2, 0, num - 3).RotatedBy(dir), direction.Opposite);
		}
		num3 = Rand.Range(14, 21);
		rot = dir.Rotated(RotationDirection.Opposite);
		for (int j = 0; j < num3; j++)
		{
			sketch.AddTerrain(TerrainDefOf.MechanoidPlatform, new IntVec3(num - 3, 0, num - 3 - j).RotatedBy(dir));
			sketch.AddTerrain(TerrainDefOf.MechanoidPlatform, new IntVec3(num - 2, 0, num - 3 - j).RotatedBy(dir));
			sketch.AddTerrain(TerrainDefOf.MechanoidPlatform, new IntVec3(num - 4, 0, num - 3 - j).RotatedBy(dir));
			sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - 2, 0, num - 3 - j).RotatedBy(dir), Rot4.North);
			if (j != 0)
			{
				sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - 4, 0, num - 3 - j).RotatedBy(dir), Rot4.North);
				if (j - num2 > 5 && j < num3 - 5 && Rand.Chance(0.05f))
				{
					num2 = j;
					AddConnection(sketch, new IntVec3(num - 3, 0, num - 3 - j).RotatedBy(dir), rot.Rotated(Rand.Bool ? RotationDirection.Clockwise : RotationDirection.Counterclockwise));
				}
			}
		}
		sketch.AddThing(ThingDefOf.AncientFortifiedWall, new IntVec3(num - 3, 0, num - 3 - num3 + 1).RotatedBy(dir), Rot4.North);
		direction = rot.Rotated(Rand.Bool ? RotationDirection.Clockwise : RotationDirection.Counterclockwise);
		AddConnection(sketch, new IntVec3(num - 3, 0, num - 3 - num3 + 2).RotatedBy(dir), direction);
		if (Rand.Chance(0.25f))
		{
			AddConnection(sketch, new IntVec3(num - 3, 0, num - 3 - num3 + 2).RotatedBy(dir), direction.Opposite);
		}
	}

	private static void AddConnection(Sketch sketch, IntVec3 rootCell, Rot4 direction)
	{
		for (int i = 1; i < 4; i++)
		{
			IntVec3 intVec = rootCell + direction.AsIntVec3 * i;
			sketch.Remove(sketch.ThingsAt(intVec).FirstOrDefault());
			sketch.AddTerrain(TerrainDefOf.MechanoidPlatform, intVec);
			sketch.AddTerrain(TerrainDefOf.MechanoidPlatform, intVec + direction.Rotated(RotationDirection.Clockwise).AsIntVec3);
			sketch.AddTerrain(TerrainDefOf.MechanoidPlatform, intVec + direction.Rotated(RotationDirection.Counterclockwise).AsIntVec3);
			sketch.AddThing(ThingDefOf.AncientFortifiedWall, intVec + direction.Rotated(RotationDirection.Clockwise).AsIntVec3, Rot4.North);
			sketch.AddThing(ThingDefOf.AncientFortifiedWall, intVec + direction.Rotated(RotationDirection.Counterclockwise).AsIntVec3, Rot4.North);
		}
		sketch.AddThing(ThingDefOf.AncientBlastDoor, rootCell + direction.AsIntVec3 * 3, direction);
	}
}
