using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class SpaceGenUtility
	{
		private static readonly IntRange ExtrusionCountRange = new IntRange(4, 8);

		private static readonly IntRange ExtrusionLengthRange = new IntRange(8, 15);

		private static readonly IntRange ExtrusionWidthRange = new IntRange(-1, 3);

		private static readonly IntRange ExtrusionGapRange = new IntRange(0, 2);

		private static readonly IntRange EdgeHoleHalfLength = new IntRange(3, 5);

		private static readonly IntRange EdgeHoleDepth = new IntRange(5, 10);

		private static readonly IntRange LargeHoleRange = new IntRange(4, 6);

		private static readonly IntRange HoleLumpLengthRange = new IntRange(3, 5);

		private static readonly IntRange HoleLumpOffsetRange = new IntRange(-1, 2);

		private static readonly IntRange HoleSizeRange = new IntRange(6, 12);

		private static readonly FloatRange HolesPerHundredCellsRange = new FloatRange(0.05f, 0.05f);

		private static readonly FloatRange PrefabsPerHundredCellsRange = new FloatRange(0.01f, 0.15f);

		private static readonly IntRange DefaultPadBorderLumpLengthRange = new IntRange(6, 10);

		private static readonly IntRange DefaultPadBorderLumpOffsetRange = new IntRange(0, 0);

		private static readonly IntRange DefaultConnectorHeightRange = new IntRange(3, 3);

		private static readonly IntRange DefaultConnectorGapRange = new IntRange(2, 3);

		private static readonly IntRange DefaultLumpOffsetRange = new IntRange(0, 0);

		private static readonly IntRange DefaultLumpLengthRange = new IntRange(3, 6);

		public static void ChunksSetSparsley(Chunks chunks, FloatRange coverageRange)
		{
			IntVec3 item = chunks.rect.ContractedBy(1).Cells.RandomElement();
			HashSet<IntVec3> hashSet = new HashSet<IntVec3> { item };
			Stack<IntVec3> stack = new Stack<IntVec3>();
			stack.Push(item);
			int num = Mathf.RoundToInt(coverageRange.RandomInRange * (float)chunks.rect.Area);
			IntVec3 result;
			while (stack.TryPop(out result) && num > 0)
			{
				num--;
				chunks.SetUsed(result, set: true);
				int num2 = Rand.Range(0, 3);
				for (int i = 0; i < 4; i++)
				{
					IntVec3 intVec = result + GenAdj.CardinalDirections[(i + num2) % 4];
					if (chunks.Contains(intVec) && hashSet.Add(intVec))
					{
						stack.Push(intVec);
					}
				}
			}
		}

		public static void ScatterExtrusions(Map map, Chunks chunks, IntRange? countRange = null, IntRange? lengthRange = null, IntRange? widthRange = null, IntRange? gapRange = null, List<CellRect> spaces = null)
		{
			IntRange intRange = countRange ?? ExtrusionCountRange;
			IntRange intRange2 = lengthRange ?? ExtrusionLengthRange;
			IntRange intRange3 = widthRange ?? ExtrusionWidthRange;
			IntRange intRange4 = gapRange ?? ExtrusionGapRange;
			HashSet<IntVec3> used = new HashSet<IntVec3>();
			int randomInRange = intRange.RandomInRange;
			CellRect otherRect = map.BoundsRect(6);
			for (int i = 0; i < randomInRange; i++)
			{
				if (!chunks.TryGetFreeEdge(out (IntVec3, Rot4) edge, inBounds: true, 1, Validator))
				{
					break;
				}
				CellRect cellRect = chunks[edge.Item1];
				Rot4 item = edge.Item2;
				used.Add(edge.Item1);
				used.Add(edge.Item1 + item.FacingCell);
				Rot4 rot = item.Rotated(RotationDirection.Clockwise);
				IntVec3 centerCellOnEdge = cellRect.GetCenterCellOnEdge(item);
				int num = chunks.regionSize / 2 - 4;
				CellRect cellRect2 = CellRect.FromLimits(centerCellOnEdge - rot.FacingCell * num - item.FacingCell * 10, centerCellOnEdge + rot.FacingCell * num);
				IntVec3 intVec = centerCellOnEdge - rot.FacingCell * num - item.FacingCell * 10;
				int num2 = 0;
				int num3;
				for (; cellRect2.Contains(intVec); intVec += rot.FacingCell * (intRange4.RandomInRange + num3 + 1))
				{
					num3 = Mathf.Max(intRange3.RandomInRange, 0);
					int num4 = Mathf.Max(intRange2.RandomInRange, 1);
					if (num4 == num2)
					{
						num4 = Mathf.Max(intRange2.RandomInRange, 1);
					}
					IntVec3 second = intVec + rot.FacingCell * num3 + item.FacingCell * (num4 + 10);
					CellRect item2 = CellRect.FromLimits(intVec, second).ClipInsideRect(otherRect);
					foreach (IntVec3 cell in item2.Cells)
					{
						map.terrainGrid.SetTerrain(cell, TerrainDefOf.OrbitalPlatform);
					}
					spaces?.Add(item2);
				}
			}
			bool Validator(IntVec3 index)
			{
				return !used.Contains(index);
			}
		}

		public static bool NoIntersection(CellRect rect, List<CellRect> spaces, IntRange holeRange)
		{
			rect = rect.ExpandedBy(holeRange.max * 2);
			foreach (CellRect space in spaces)
			{
				if (space.Overlaps(rect))
				{
					return false;
				}
			}
			return true;
		}

		public static void GenerateHole(Map map, CellRect rect)
		{
			foreach (IntVec3 cell in rect.Cells)
			{
				List<Thing> thingList = cell.GetThingList(map);
				for (int num = thingList.Count - 1; num >= 0; num--)
				{
					thingList[num].Destroy();
				}
				map.terrainGrid.SetTerrain(cell, TerrainDefOf.Space);
				map.roofGrid.SetRoof(cell, null);
			}
		}

		public static void DamageEdges(Map map, Chunks chunks, int maximum, ref int cells)
		{
			int num = 4;
			while (cells > maximum && num-- > 0)
			{
				GenerateEdgeCut(map, chunks, chunks.RandomUsedChunk, EdgeHoleHalfLength, EdgeHoleDepth, ref cells);
			}
		}

		public static void GenerateEdgeCut(Map map, Chunks chunks, IntVec3 chunk, IntRange halfLengthRange, IntRange depthRange, ref int cells, Predicate<CellRect> validator = null, List<CellRect> spaces = null, IntRange? lengthRange = null, IntRange? offsetRange = null, float extendChance = 1f, int borderPadding = 20)
		{
			if (!chunks.TryGetFreeEdge(out (IntVec3, Rot4) edge))
			{
				return;
			}
			CellRect cellRect = chunks[chunk];
			Rot4 item = edge.Item2;
			Rot4 rot = item.Rotated(RotationDirection.Clockwise);
			IntVec3 intVec = cellRect.GetCellsOnEdge(item).RandomElement();
			IntVec3 intVec2 = intVec + rot.FacingCell * halfLengthRange.RandomInRange;
			IntVec3 intVec3 = intVec - rot.FacingCell * halfLengthRange.RandomInRange;
			IntVec3 intVec4 = -item.FacingCell * depthRange.RandomInRange;
			IntVec3 intVec5 = -item.FacingCell * borderPadding;
			CellRect rect = CellRect.FromLimits(intVec2 - intVec5, intVec3 + intVec4).ClipInsideMap(map);
			if (validator == null || validator(rect))
			{
				GenerateHole(map, rect);
				IntRange lengthRange2 = lengthRange ?? HoleLumpLengthRange;
				IntRange offsetRange2 = offsetRange ?? HoleLumpOffsetRange;
				cells -= MapGenUtility.DoRectEdgeLumps(map, TerrainDefOf.Space, ref rect, lengthRange2, offsetRange2);
				cells -= rect.Area;
				if (Rand.Chance(extendChance))
				{
					GenerateExtraDamage(map, ref rect, LargeHoleRange, ref cells, validator, spaces);
				}
				spaces?.Add(rect);
			}
		}

		public static void GenerateExtraDamage(Map map, ref CellRect rect, IntRange sizeRange, ref int cells, Predicate<CellRect> validator = null, List<CellRect> spaces = null)
		{
			if (!rect.IsEmpty)
			{
				IntVec3 intVec = rect.Corners.MaxBy((IntVec3 c) => c.DistanceToEdge(map));
				Rot4 rot;
				Rot4 rot2;
				if (rect.Width >= rect.Height)
				{
					rot = ((intVec.z >= rect.CenterCell.z) ? Rot4.North : Rot4.South);
					rot2 = ((intVec.x >= rect.CenterCell.x) ? Rot4.West : Rot4.East);
				}
				else
				{
					rot = ((intVec.x >= rect.CenterCell.x) ? Rot4.East : Rot4.West);
					rot2 = ((intVec.z >= rect.CenterCell.z) ? Rot4.South : Rot4.North);
				}
				IntVec3 first = intVec + rot2.FacingCell * sizeRange.RandomInRange;
				IntVec3 second = intVec + rot.FacingCell * sizeRange.RandomInRange;
				CellRect rect2 = CellRect.FromLimits(first, second).ClipInsideMap(map);
				if (validator == null || validator(rect2.ExpandedBy(HoleLumpOffsetRange.max)))
				{
					GenerateHole(map, rect2);
					cells -= MapGenUtility.DoRectEdgeLumps(map, TerrainDefOf.Space, ref rect2, HoleLumpLengthRange, HoleLumpOffsetRange);
					cells -= rect2.Area;
					spaces?.Add(rect2);
				}
			}
		}

		public static void DamageHoles(Chunks chunks, Map map, int maximum, ref int cells, IntRange? sizeRange = null, FloatRange? per100Range = null, float extendChance = 1f, Predicate<CellRect> validator = null, List<CellRect> spaces = null)
		{
			IntRange intRange = sizeRange ?? HoleSizeRange;
			FloatRange floatRange = per100Range ?? HolesPerHundredCellsRange;
			int num = Mathf.RoundToInt(floatRange.RandomInRange * (float)cells / 100f);
			for (int i = 0; i < num; i++)
			{
				if (cells <= maximum)
				{
					break;
				}
				IntVec2 size = new IntVec2(intRange.RandomInRange, intRange.RandomInRange);
				if (!TryGetRect(chunks, size, out var rect, (CellRect r) => validator?.Invoke(r.ExpandedBy(HoleLumpOffsetRange.max)) ?? true))
				{
					break;
				}
				GenerateHole(map, rect);
				cells -= MapGenUtility.DoRectEdgeLumps(map, TerrainDefOf.Space, ref rect, HoleLumpLengthRange, HoleLumpOffsetRange);
				cells -= rect.Area;
				spaces?.Add(rect);
			}
			int num2 = Mathf.RoundToInt(floatRange.RandomInRange * (float)cells / 100f);
			for (int num3 = 0; num3 < num2; num3++)
			{
				if (cells <= maximum)
				{
					break;
				}
				IntVec2 size2 = new IntVec2(LargeHoleRange.RandomInRange, LargeHoleRange.RandomInRange);
				if (TryGetRect(chunks, size2, out var rect2, (CellRect r) => validator?.Invoke(r.ExpandedBy(HoleLumpOffsetRange.max)) ?? true))
				{
					GenerateHole(map, rect2);
					cells -= MapGenUtility.DoRectEdgeLumps(map, TerrainDefOf.Space, ref rect2, HoleLumpLengthRange, HoleLumpOffsetRange);
					cells -= rect2.Area;
					if (Rand.Chance(extendChance))
					{
						GenerateExtraDamage(map, ref rect2, LargeHoleRange, ref cells, validator, spaces);
					}
					spaces?.Add(rect2);
					continue;
				}
				break;
			}
		}

		public static bool TryGetRect(Chunks chunks, IntVec2 size, out CellRect rect, Predicate<CellRect> validator = null)
		{
			foreach (CellRect item in chunks.EnumeratedRects.InRandomOrder())
			{
				if (item.Size.x >= size.x && item.Size.z >= size.z && item.TryFindRandomInnerRect(size, out rect, validator))
				{
					return true;
				}
			}
			rect = default(CellRect);
			return false;
		}

		public static void ScatterPrefabs(Map map, Chunks chunks, Faction faction, List<PrefabParms> prefabs, Predicate<CellRect> validator = null, FloatRange? prefabsPer100K = null)
		{
			FloatRange floatRange = prefabsPer100K ?? PrefabsPerHundredCellsRange;
			float num = Mathf.Round((float)chunks.ApproximateArea / 100f * floatRange.RandomInRange);
			Rot4 random = Rot4.Random;
			for (int i = 0; (float)i < num; i++)
			{
				PrefabParms prefab = prefabs.RandomElementByWeight((PrefabParms w) => w.weight);
				int num2 = Mathf.Max(prefab.def.size.x, prefab.def.size.z);
				IntVec2 size = new IntVec2(num2, num2);
				Rot4 lRot = random;
				if (TryGetRect(chunks, size, out var rect, Validator))
				{
					PrefabUtility.SpawnPrefab(prefab.def, map, rect.CenterCell, random, faction, null, null, OnSpawned);
				}
				random = Rot4.Random;
				bool Validator(CellRect r)
				{
					if (validator == null || validator(r))
					{
						return PrefabUtility.CanSpawnPrefab(prefab.def, map, r.CenterCell, lRot, canWipeEdifices: false);
					}
					return false;
				}
			}
			static void OnSpawned(Thing thing)
			{
				if (thing.TryGetComp(out CompPowerBattery comp))
				{
					comp.SetStoredEnergyPct(1f);
				}
			}
		}

		public static void GenerateRing(Map map, CellRect rect, TerrainDef terrain, int distance, int width, float ringWidth = 4.9f, float ringExtraOffset = 0.5f, float ringExtraChance = 0f)
		{
			IntVec3 centerCell = rect.CenterCell;
			Vector3 vector = centerCell.ToVector3();
			int num = distance + width;
			Vector3 vector2 = new Vector3((float)(rect.minX - num) - ringWidth, 0f, centerCell.z);
			Vector3 vector3 = new Vector3((float)(rect.minX - num) - ringWidth - ringExtraOffset, 0f, centerCell.z);
			Vector3 vector4 = new Vector3(rect.minX - num, 0f, centerCell.z);
			Vector3 vector5 = new Vector3((float)(rect.minX - num) + ringExtraOffset, 0f, centerCell.z);
			float sqrMagnitude = (vector2 - vector).sqrMagnitude;
			float sqrMagnitude2 = (vector4 - vector).sqrMagnitude;
			float sqrMagnitude3 = (vector3 - vector).sqrMagnitude;
			float sqrMagnitude4 = (vector5 - vector).sqrMagnitude;
			foreach (IntVec3 allCell in map.AllCells)
			{
				float sqrMagnitude5 = (allCell - map.Center).SqrMagnitude;
				if (allCell.GetTerrain(map) == TerrainDefOf.Space && ((sqrMagnitude5 >= sqrMagnitude2 && sqrMagnitude5 <= sqrMagnitude) || (Rand.Chance(ringExtraChance) && sqrMagnitude5 >= sqrMagnitude4 && sqrMagnitude5 <= sqrMagnitude3)))
				{
					map.terrainGrid.SetTerrain(allCell, terrain);
				}
			}
		}

		public static CellRect GenerateConnectedPlatform(Map map, TerrainDef terrain, CellRect platformRect, IntRange widthRange, IntRange heightRange, Rot4 dir, int distance = 14, float lumpChance = 0.2f, int minHeight = 2, IntRange? connectorHeightRange = null, IntRange? connectorGapRange = null, IntRange? lumpOffsetRange = null, IntRange? lumpLengthRange = null)
		{
			Rot4 rot = dir.Rotated(RotationDirection.Clockwise);
			IntVec3 corner = platformRect.GetCorner(dir);
			int num = platformRect.GetSideLength(dir) - heightRange.max;
			IntVec3 intVec = CellRect.FromLimits(corner - rot.FacingCell * num, corner - rot.FacingCell * 10).RandomCell + dir.FacingCell * distance;
			IntVec3 second = intVec - rot.FacingCell * heightRange.RandomInRange + dir.FacingCell * widthRange.RandomInRange;
			CellRect rect = CellRect.FromLimits(intVec, second).ClipInsideMap(map);
			SpawnWalkways(map, terrain, rect, dir.Opposite, platformRect.GetSideLength(rot) / 2 + distance, lumpChance, minHeight, connectorHeightRange, connectorGapRange, lumpOffsetRange, lumpLengthRange);
			foreach (IntVec3 item in rect)
			{
				if (item.GetTerrain(map) == TerrainDefOf.Space)
				{
					map.terrainGrid.SetTerrain(item, terrain);
				}
			}
			foreach (IntVec3 corner2 in rect.ContractedBy(2).Corners)
			{
				GenSpawn.Spawn(ThingDefOf.AncientShipBeacon, corner2, map);
			}
			MapGenUtility.DoRectEdgeLumps(map, terrain, ref rect, DefaultPadBorderLumpLengthRange, DefaultPadBorderLumpOffsetRange);
			return rect;
		}

		public static void SpawnWalkways(Map map, TerrainDef terrain, CellRect rect, Rot4 dir, int maxDistance, float lumpChance = 0.2f, int minHeight = 2, IntRange? connectorHeightRange = null, IntRange? connectorGapRange = null, IntRange? lumpOffsetRange = null, IntRange? lumpLengthRange = null)
		{
			IntVec3 facingCell = dir.Rotated(RotationDirection.Counterclockwise).FacingCell;
			IntRange intRange = connectorHeightRange ?? DefaultConnectorHeightRange;
			IntRange intRange2 = connectorGapRange ?? DefaultConnectorGapRange;
			IntRange intRange3 = lumpOffsetRange ?? DefaultLumpOffsetRange;
			IntRange intRange4 = lumpLengthRange ?? DefaultLumpLengthRange;
			CellRect cellRect = rect.GetEdgeRect(dir).ContractedBy(dir.IsVertical ? intRange.max : 0, dir.IsHorizontal ? intRange.max : 0);
			int randomInRange;
			int num3;
			for (IntVec3 intVec = cellRect.GetCorner(dir) + facingCell; cellRect.Contains(intVec); intVec += facingCell * (randomInRange + intRange2.RandomInRange + num3))
			{
				randomInRange = intRange.RandomInRange;
				int num = 0;
				int num2 = 0;
				num3 = 0;
				IntVec3 intVec2 = intVec;
				for (CellRect cellRect2 = CellRect.FromLimits(intVec, intVec + dir.FacingCell * (maxDistance + 2)); cellRect2.Contains(intVec2); intVec2 += dir.FacingCell)
				{
					if (num <= 0 && Rand.Chance(lumpChance))
					{
						num = intRange4.RandomInRange;
						num2 = intRange3.RandomInRange;
						if (randomInRange + num2 < minHeight)
						{
							num2 = 0;
						}
						if (num2 > num3)
						{
							num3 = num2;
						}
					}
					bool flag = false;
					for (int i = -num2; i < randomInRange + num2; i++)
					{
						IntVec3 c = intVec2 + facingCell * i;
						if (c.InBounds(map) && c.GetTerrain(map) == TerrainDefOf.Space)
						{
							map.terrainGrid.SetTerrain(c, terrain);
							flag = true;
						}
					}
					if (!flag)
					{
						break;
					}
					if (num-- <= 0)
					{
						num2 = 0;
					}
				}
			}
		}
	}
}
