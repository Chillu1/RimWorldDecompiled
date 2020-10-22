using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class GenStep_Roads : GenStep
	{
		private struct NeededRoad
		{
			public float angle;

			public RoadDef road;
		}

		private struct DrawCommand
		{
			public RoadDef roadDef;

			public Action action;
		}

		public struct DistanceElement
		{
			public float fromRoad;

			public float alongPath;

			public bool touched;

			public IntVec3 origin;
		}

		private const float CurveControlPointDistance = 4f;

		private const int CurveSampleMultiplier = 4;

		private readonly float[] endcapSamples = new float[5]
		{
			0.75f,
			0.8f,
			0.85f,
			0.9f,
			0.95f
		};

		public override int SeedPart => 1187464702;

		public override void Generate(Map map, GenStepParams parms)
		{
			List<NeededRoad> neededRoads = CalculateNeededRoads(map);
			if (neededRoads.Count == 0)
			{
				return;
			}
			List<DrawCommand> list = new List<DrawCommand>();
			DeepProfiler.Start("RebuildAllRegions");
			map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
			DeepProfiler.End();
			TerrainDef rockDef = BaseGenUtility.RegionalRockTerrainDef(map.Tile, beautiful: false);
			IntVec3 centerpoint = CellFinderLoose.TryFindCentralCell(map, 3, 10);
			RoadDef bestRoadType = DefDatabase<RoadDef>.AllDefs.Where((RoadDef rd) => neededRoads.Count((NeededRoad nr) => nr.road == rd) >= 2).MaxByWithFallback((RoadDef rd) => rd.priority);
			DrawCommand item;
			if (bestRoadType != null)
			{
				NeededRoad neededRoad = neededRoads[neededRoads.FindIndex((NeededRoad nr) => nr.road == bestRoadType)];
				neededRoads.RemoveAt(neededRoads.FindIndex((NeededRoad nr) => nr.road == bestRoadType));
				NeededRoad neededRoad2 = neededRoads[neededRoads.FindIndex((NeededRoad nr) => nr.road == bestRoadType)];
				neededRoads.RemoveAt(neededRoads.FindIndex((NeededRoad nr) => nr.road == bestRoadType));
				RoadPathingDef pathingDef = neededRoad.road.pathingMode;
				IntVec3 intVec = FindRoadExitCell(map, neededRoad.angle, centerpoint, ref pathingDef);
				IntVec3 end = FindRoadExitCell(map, neededRoad2.angle, intVec, ref pathingDef);
				Action action = PrepDrawRoad(map, rockDef, intVec, end, neededRoad.road, pathingDef, out centerpoint);
				item = new DrawCommand
				{
					action = action,
					roadDef = bestRoadType
				};
				list.Add(item);
			}
			foreach (NeededRoad item2 in neededRoads)
			{
				RoadPathingDef pathingDef2 = item2.road.pathingMode;
				IntVec3 intVec2 = FindRoadExitCell(map, item2.angle, centerpoint, ref pathingDef2);
				if (!(intVec2 == IntVec3.Invalid))
				{
					item = new DrawCommand
					{
						action = PrepDrawRoad(map, rockDef, centerpoint, intVec2, item2.road, pathingDef2),
						roadDef = item2.road
					};
					list.Add(item);
				}
			}
			foreach (DrawCommand item3 in list.OrderBy((DrawCommand dc) => dc.roadDef.priority))
			{
				if (item3.action != null)
				{
					item3.action();
				}
			}
		}

		private List<NeededRoad> CalculateNeededRoads(Map map)
		{
			List<int> list = new List<int>();
			Find.WorldGrid.GetTileNeighbors(map.Tile, list);
			List<NeededRoad> list2 = new List<NeededRoad>();
			NeededRoad item;
			foreach (int item2 in list)
			{
				RoadDef roadDef = Find.WorldGrid.GetRoadDef(map.Tile, item2);
				if (roadDef != null)
				{
					item = new NeededRoad
					{
						angle = Find.WorldGrid.GetHeadingFromTo(map.Tile, item2),
						road = roadDef
					};
					list2.Add(item);
				}
			}
			if (list2.Count > 1)
			{
				Vector3 zero = Vector3.zero;
				foreach (NeededRoad item3 in list2)
				{
					zero += Vector3Utility.HorizontalVectorFromAngle(item3.angle);
				}
				zero /= (float)(-list2.Count);
				zero += Rand.UnitVector3 * 1f / 6f;
				zero.y = 0f;
				for (int i = 0; i < list2.Count; i++)
				{
					item = (list2[i] = new NeededRoad
					{
						angle = (Vector3Utility.HorizontalVectorFromAngle(list2[i].angle) + zero).AngleFlat(),
						road = list2[i].road
					});
				}
			}
			return list2;
		}

		private IntVec3 FindRoadExitCell(Map map, float angle, IntVec3 crossroads, ref RoadPathingDef pathingDef)
		{
			Predicate<IntVec3> tileValidator = delegate(IntVec3 pos)
			{
				foreach (IntVec3 item in GenRadial.RadialCellsAround(pos, 8f, useCenter: true))
				{
					if (item.InBounds(map) && item.GetTerrain(map).IsWater)
					{
						return false;
					}
				}
				return true;
			};
			IntVec3 result;
			for (float validAngleSpan2 = 10f; validAngleSpan2 < 90f; validAngleSpan2 += 10f)
			{
				Predicate<IntVec3> angleValidator2 = (IntVec3 pos) => GenGeo.AngleDifferenceBetween((pos - map.Center).AngleFlat, angle) < validAngleSpan2;
				if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => angleValidator2(x) && tileValidator(x) && map.reachability.CanReach(crossroads, x, PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors)), map, 0f, out result))
				{
					return result;
				}
			}
			if (pathingDef == RoadPathingDefOf.Avoid)
			{
				pathingDef = RoadPathingDefOf.Bulldoze;
			}
			for (float validAngleSpan = 10f; validAngleSpan < 90f; validAngleSpan += 10f)
			{
				Predicate<IntVec3> angleValidator = (IntVec3 pos) => GenGeo.AngleDifferenceBetween((pos - map.Center).AngleFlat, angle) < validAngleSpan;
				if (CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => angleValidator(x) && tileValidator(x) && map.reachability.CanReach(crossroads, x, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassAllDestroyableThings)), map, 0f, out result))
				{
					return result;
				}
			}
			Log.Error($"Can't find exit from map from {crossroads} to angle {angle}");
			return IntVec3.Invalid;
		}

		private Action PrepDrawRoad(Map map, TerrainDef rockDef, IntVec3 start, IntVec3 end, RoadDef roadDef, RoadPathingDef pathingDef)
		{
			IntVec3 centerpoint;
			return PrepDrawRoad(map, rockDef, start, end, roadDef, pathingDef, out centerpoint);
		}

		private Action PrepDrawRoad(Map map, TerrainDef rockDef, IntVec3 start, IntVec3 end, RoadDef roadDef, RoadPathingDef pathingDef, out IntVec3 centerpoint)
		{
			centerpoint = IntVec3.Invalid;
			PawnPath pawnPath = map.pathFinder.FindPath(start, end, TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater));
			if (pawnPath == PawnPath.NotFound)
			{
				pawnPath = map.pathFinder.FindPath(start, end, TraverseParms.For(TraverseMode.NoPassClosedDoors));
			}
			if (pawnPath == PawnPath.NotFound)
			{
				pawnPath = map.pathFinder.FindPath(start, end, TraverseParms.For(TraverseMode.PassAllDestroyableThingsNotWater));
			}
			if (pawnPath == PawnPath.NotFound)
			{
				pawnPath = map.pathFinder.FindPath(start, end, TraverseParms.For(TraverseMode.PassAllDestroyableThings));
			}
			if (pawnPath == PawnPath.NotFound)
			{
				return null;
			}
			List<IntVec3> list = RefinePath(pawnPath.NodesReversed, map);
			pawnPath.ReleaseToPool();
			DistanceElement[,] distance = new DistanceElement[map.Size.x, map.Size.z];
			for (int i = 0; i < distance.GetLength(0); i++)
			{
				for (int j = 0; j < distance.GetLength(1); j++)
				{
					distance[i, j].origin = IntVec3.Invalid;
				}
			}
			int count = list.Count;
			int centerpointIndex = Mathf.RoundToInt(Rand.Range(0.3f, 0.7f) * (float)count);
			int num = Mathf.Max(1, GenMath.RoundRandom((float)count / (float)roadDef.tilesPerSegment));
			for (int k = 0; k < num; k++)
			{
				int pathStartIndex = Mathf.RoundToInt((float)(count - 1) / (float)num * (float)k);
				int pathEndIndex = Mathf.RoundToInt((float)(count - 1) / (float)num * (float)(k + 1));
				DrawCurveSegment(distance, list, pathStartIndex, pathEndIndex, pathingDef, map, centerpointIndex, ref centerpoint);
			}
			return delegate
			{
				ApplyDistanceField(distance, map, rockDef, roadDef, pathingDef);
			};
		}

		private void DrawCurveSegment(DistanceElement[,] distance, List<IntVec3> path, int pathStartIndex, int pathEndIndex, RoadPathingDef pathing, Map map, int centerpointIndex, ref IntVec3 centerpoint)
		{
			if (pathStartIndex == pathEndIndex)
			{
				Log.ErrorOnce("Zero-length segment drawn in road routine", 78187971);
				return;
			}
			GenMath.BezierCubicControls bcc = GenerateBezierControls(path, pathStartIndex, pathEndIndex);
			List<Vector3> list = new List<Vector3>();
			int num = (pathEndIndex - pathStartIndex) * 4;
			for (int i = 0; i <= num; i++)
			{
				list.Add(GenMath.BezierCubicEvaluate((float)i / (float)num, bcc));
			}
			int num2 = 0;
			for (int j = pathStartIndex; j <= pathEndIndex; j++)
			{
				if (j > 0 && j < path.Count && path[j].InBounds(map) && path[j].GetTerrain(map).IsWater)
				{
					num2++;
				}
			}
			if (pathStartIndex + 1 < pathEndIndex)
			{
				for (int k = 0; k < list.Count; k++)
				{
					IntVec3 intVec = list[k].ToIntVec3();
					bool flag = intVec.InBounds(map) && intVec.Impassable(map);
					int num3 = 0;
					for (int l = 0; l < GenAdj.CardinalDirections.Length; l++)
					{
						if (flag)
						{
							break;
						}
						IntVec3 c = intVec + GenAdj.CardinalDirections[l];
						if (c.InBounds(map))
						{
							flag |= pathing == RoadPathingDefOf.Avoid && c.Impassable(map);
							if (c.GetTerrain(map).IsWater)
							{
								num3++;
							}
							if (flag)
							{
								break;
							}
						}
					}
					if (flag || (float)num3 > (float)num2 * 1.5f + 2f)
					{
						DrawCurveSegment(distance, path, pathStartIndex, (pathStartIndex + pathEndIndex) / 2, pathing, map, centerpointIndex, ref centerpoint);
						DrawCurveSegment(distance, path, (pathStartIndex + pathEndIndex) / 2, pathEndIndex, pathing, map, centerpointIndex, ref centerpoint);
						return;
					}
				}
			}
			for (int m = 0; m < list.Count; m++)
			{
				FillDistanceField(distance, list[m].x, list[m].z, GenMath.LerpDouble(0f, list.Count - 1, pathStartIndex, pathEndIndex, m), 10f, map);
			}
			if (centerpointIndex >= pathStartIndex && centerpointIndex < pathEndIndex)
			{
				centerpointIndex = Mathf.Clamp(Mathf.RoundToInt(GenMath.LerpDouble(pathStartIndex, pathEndIndex, 0f, list.Count, centerpointIndex)), 0, list.Count - 1);
				centerpoint = list[centerpointIndex].ToIntVec3();
			}
		}

		private GenMath.BezierCubicControls GenerateBezierControls(List<IntVec3> path, int pathStartIndex, int pathEndIndex)
		{
			int index = Mathf.Max(0, pathStartIndex - (pathEndIndex - pathStartIndex));
			int index2 = Mathf.Min(path.Count - 1, pathEndIndex - (pathStartIndex - pathEndIndex));
			GenMath.BezierCubicControls result = default(GenMath.BezierCubicControls);
			result.w0 = path[pathStartIndex].ToVector3Shifted();
			result.w1 = path[pathStartIndex].ToVector3Shifted() + (path[pathEndIndex] - path[index]).ToVector3().normalized * 4f;
			result.w2 = path[pathEndIndex].ToVector3Shifted() + (path[pathStartIndex] - path[index2]).ToVector3().normalized * 4f;
			result.w3 = path[pathEndIndex].ToVector3Shifted();
			return result;
		}

		private void ApplyDistanceField(DistanceElement[,] distance, Map map, TerrainDef rockDef, RoadDef roadDef, RoadPathingDef pathingDef)
		{
			for (int i = 0; i < map.Size.x; i++)
			{
				for (int j = 0; j < map.Size.z; j++)
				{
					DistanceElement distanceElement = distance[i, j];
					if (!distanceElement.touched)
					{
						continue;
					}
					float b = Mathf.Abs(distanceElement.fromRoad + Rand.Value - 0.5f);
					for (int k = 0; k < roadDef.roadGenSteps.Count; k++)
					{
						RoadDefGenStep roadDefGenStep = roadDef.roadGenSteps[k];
						float x = Mathf.LerpUnclamped(distanceElement.fromRoad, b, roadDefGenStep.antialiasingMultiplier);
						float num = roadDefGenStep.chancePerPositionCurve.Evaluate(x);
						if (!(num <= 0f) && (roadDefGenStep.periodicSpacing == 0 || !(distanceElement.alongPath / (float)roadDefGenStep.periodicSpacing % 1f * (float)roadDefGenStep.periodicSpacing >= 1f)))
						{
							IntVec3 position = new IntVec3(i, 0, j);
							if (Rand.Value < num)
							{
								roadDefGenStep.Place(map, position, rockDef, distanceElement.origin, distance);
							}
						}
					}
				}
			}
		}

		private void FillDistanceField(DistanceElement[,] field, float cx, float cz, float alongPath, float radius, Map map)
		{
			int num = Mathf.Clamp(Mathf.FloorToInt(cx - radius), 0, field.GetLength(0) - 1);
			int num2 = Mathf.Clamp(Mathf.FloorToInt(cx + radius), 0, field.GetLength(0) - 1);
			int num3 = Mathf.Clamp(Mathf.FloorToInt(cz - radius), 0, field.GetLength(1) - 1);
			int num4 = Mathf.Clamp(Mathf.FloorToInt(cz + radius), 0, field.GetLength(1) - 1);
			IntVec3 origin = new Vector3(cx, 0f, cz).ToIntVec3().ClampInsideMap(map);
			for (int i = num; i <= num2; i++)
			{
				float num5 = ((float)i + 0.5f - cx) * ((float)i + 0.5f - cx);
				for (int j = num3; j <= num4; j++)
				{
					float num6 = ((float)j + 0.5f - cz) * ((float)j + 0.5f - cz);
					float num7 = Mathf.Sqrt(num5 + num6);
					float fromRoad = field[i, j].fromRoad;
					if (!field[i, j].touched || num7 < fromRoad)
					{
						field[i, j].fromRoad = num7;
						field[i, j].alongPath = alongPath;
						field[i, j].origin = origin;
					}
					field[i, j].touched = true;
				}
			}
		}

		private List<IntVec3> RefinePath(List<IntVec3> input, Map map)
		{
			List<IntVec3> list = RefineEndcap(input, map);
			list.Reverse();
			return RefineEndcap(list, map);
		}

		private List<IntVec3> RefineEndcap(List<IntVec3> input, Map map)
		{
			float[] array = new float[endcapSamples.Length];
			for (int i = 0; i < endcapSamples.Length; i++)
			{
				int index = Mathf.RoundToInt((float)input.Count * endcapSamples[i]);
				PawnPath pawnPath = map.pathFinder.FindPath(input[index], input[input.Count - 1], TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater));
				if (pawnPath == PawnPath.NotFound)
				{
					pawnPath = map.pathFinder.FindPath(input[index], input[input.Count - 1], TraverseParms.For(TraverseMode.NoPassClosedDoors));
				}
				if (pawnPath == PawnPath.NotFound)
				{
					pawnPath = map.pathFinder.FindPath(input[index], input[input.Count - 1], TraverseParms.For(TraverseMode.PassAllDestroyableThingsNotWater));
				}
				if (pawnPath == PawnPath.NotFound)
				{
					pawnPath = map.pathFinder.FindPath(input[index], input[input.Count - 1], TraverseParms.For(TraverseMode.PassAllDestroyableThings));
				}
				if (pawnPath != null && pawnPath != PawnPath.NotFound)
				{
					array[i] = pawnPath.TotalCost;
				}
				pawnPath.ReleaseToPool();
			}
			float num = 0f;
			int num2 = 0;
			IntVec3 start = IntVec3.Invalid;
			for (int j = 0; j < 2; j++)
			{
				IntVec3 facingCell = new Rot4(j).FacingCell;
				IntVec3 intVec = input[input.Count - 1];
				bool flag = true;
				if (Mathf.Abs(intVec.x * facingCell.x) > 5 && Mathf.Abs(intVec.x * facingCell.x - map.Size.x) > 5)
				{
					flag = false;
				}
				if (Mathf.Abs(intVec.z * facingCell.z) > 5 && Mathf.Abs(intVec.z * facingCell.z - map.Size.z) > 5)
				{
					flag = false;
				}
				if (!flag)
				{
					continue;
				}
				for (int k = 0; k < endcapSamples.Length; k++)
				{
					if (array[k] == 0f)
					{
						continue;
					}
					int num3 = Mathf.RoundToInt((float)input.Count * endcapSamples[k]);
					IntVec3 intVec2 = input[num3];
					if (facingCell.x != 0)
					{
						intVec2.x = intVec.x;
					}
					else if (facingCell.z != 0)
					{
						intVec2.z = intVec.z;
					}
					PawnPath pawnPath2 = map.pathFinder.FindPath(input[num3], input[input.Count - 1], TraverseParms.For(TraverseMode.NoPassClosedDoors));
					if (pawnPath2 == PawnPath.NotFound)
					{
						pawnPath2 = map.pathFinder.FindPath(input[num3], input[input.Count - 1], TraverseParms.For(TraverseMode.PassAllDestroyableThings));
					}
					if (pawnPath2 != PawnPath.NotFound)
					{
						float num4 = array[k] / pawnPath2.TotalCost;
						if (num4 > num)
						{
							num = num4;
							num2 = num3;
							start = intVec2;
						}
						pawnPath2.ReleaseToPool();
					}
				}
			}
			input = new List<IntVec3>(input);
			if ((double)num > 1.75)
			{
				using (PawnPath pawnPath3 = map.pathFinder.FindPath(start, input[num2], TraverseParms.For(TraverseMode.NoPassClosedDoors)))
				{
					input.RemoveRange(num2, input.Count - num2);
					input.AddRange(pawnPath3.NodesReversed);
					return input;
				}
			}
			return input;
		}
	}
}
