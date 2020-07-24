using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class MechClusterGenerator
	{
		public const string MechClusterMemberTag = "MechClusterMember";

		public const string MechClusterMemberGoodTag = "MechClusterMemberGood";

		public const string MechClusterActivatorTag = "MechClusterActivator";

		public const string MechClusterCombatThreatTag = "MechClusterCombatThreat";

		public const string MechClusterProblemCauserTag = "MechClusterProblemCauser";

		public const float MaxPoints = 10000f;

		public static readonly SimpleCurve PointsToPawnsChanceCurve = new SimpleCurve
		{
			new CurvePoint(400f, 0.75f)
		};

		public static readonly SimpleCurve PawnPointsRandomPercentOfTotalCurve = new SimpleCurve
		{
			new CurvePoint(0.2f, 0f),
			new CurvePoint(0.5f, 1f),
			new CurvePoint(0.8f, 0f)
		};

		private static readonly FloatRange SizeRandomFactorRange = new FloatRange(0.8f, 2f);

		private static readonly SimpleCurve PointsToSizeCurve = new SimpleCurve
		{
			new CurvePoint(400f, 7f),
			new CurvePoint(1000f, 10f),
			new CurvePoint(2000f, 20f),
			new CurvePoint(5000f, 25f)
		};

		private static readonly SimpleCurve ProblemCauserCountCurve = new SimpleCurve
		{
			new CurvePoint(400f, 0.5f),
			new CurvePoint(800f, 0.9f),
			new CurvePoint(1200f, 0.95f)
		};

		private static readonly SimpleCurve WallsChanceCurve = new SimpleCurve
		{
			new CurvePoint(400f, 0.35f),
			new CurvePoint(1000f, 0.5f)
		};

		private const float ActivatorCountdownChance = 0.5f;

		private const float ActivatorProximityChance = 0.5f;

		private static readonly SimpleCurve ActivatorProximitysCountCurve = new SimpleCurve
		{
			new CurvePoint(600f, 1f),
			new CurvePoint(1800f, 2f),
			new CurvePoint(3000f, 3f),
			new CurvePoint(5000f, 4f)
		};

		private static readonly SimpleCurve GoodBuildingChanceCurve = new SimpleCurve
		{
			new CurvePoint(400f, 0.5f)
		};

		private static readonly SimpleCurve GoodBuildingMaxCountCurve = new SimpleCurve
		{
			new CurvePoint(400f, 1f),
			new CurvePoint(700f, 2f),
			new CurvePoint(1000f, 3f),
			new CurvePoint(1300f, 4f),
			new CurvePoint(2000f, 5f),
			new CurvePoint(3000f, 6f),
			new CurvePoint(5000f, 7f)
		};

		private static readonly SimpleCurve BulletShieldChanceCurve = new SimpleCurve
		{
			new CurvePoint(400f, 0.1f),
			new CurvePoint(1000f, 0.4f),
			new CurvePoint(2200f, 0.5f)
		};

		private const float BulletShieldTotalPointsFactor = 0.85f;

		private static readonly SimpleCurve BulletShieldMaxCountCurve = new SimpleCurve
		{
			new CurvePoint(400f, 1f),
			new CurvePoint(3000f, 1.5f)
		};

		private const float MortarShieldTotalPointsFactor = 0.9f;

		private static readonly SimpleCurve MortarShieldChanceCurve = new SimpleCurve
		{
			new CurvePoint(400f, 0.1f),
			new CurvePoint(1000f, 0.4f),
			new CurvePoint(2200f, 0.5f)
		};

		private const float BuildingRechooseWeight = 200f;

		[Obsolete]
		public static MechClusterSketch GenerateClusterSketch(float points, bool startDormant = true)
		{
			return GenerateClusterSketch(points, null, startDormant);
		}

		public static MechClusterSketch GenerateClusterSketch(float points, Map map, bool startDormant = true)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Mech clusters are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 657122);
				return new MechClusterSketch(new Sketch(), new List<MechClusterSketch.Mech>(), startDormant);
			}
			points = Mathf.Min(points, 10000f);
			float num = points;
			List<MechClusterSketch.Mech> list = null;
			if (Rand.Chance(PointsToPawnsChanceCurve.Evaluate(points)))
			{
				List<PawnKindDef> source = DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef def) => def.RaceProps.IsMechanoid).ToList();
				list = new List<MechClusterSketch.Mech>();
				float a = Rand.ByCurve(PawnPointsRandomPercentOfTotalCurve) * num;
				float pawnPointsLeft;
				a = (pawnPointsLeft = Mathf.Max(a, source.Min((PawnKindDef x) => x.combatPower)));
				PawnKindDef result;
				while (pawnPointsLeft > 0f && source.Where((PawnKindDef def) => def.combatPower <= pawnPointsLeft).TryRandomElement(out result))
				{
					pawnPointsLeft -= result.combatPower;
					list.Add(new MechClusterSketch.Mech(result));
				}
				num -= a - pawnPointsLeft;
			}
			Sketch buildingsSketch = RimWorld.SketchGen.SketchGen.Generate(SketchResolverDefOf.MechCluster, new ResolveParams
			{
				points = num,
				totalPoints = points,
				mechClusterDormant = startDormant,
				sketch = new Sketch(),
				mechClusterForMap = map
			});
			if (list != null)
			{
				List<IntVec3> pawnUsedSpots = new List<IntVec3>();
				for (int i = 0; i < list.Count; i++)
				{
					MechClusterSketch.Mech pawn = list[i];
					if (!buildingsSketch.OccupiedRect.Where((IntVec3 c) => !buildingsSketch.ThingsAt(c).Any() && !pawnUsedSpots.Contains(c)).TryRandomElement(out IntVec3 result2))
					{
						CellRect cellRect = buildingsSketch.OccupiedRect;
						do
						{
							cellRect = cellRect.ExpandedBy(1);
						}
						while (!cellRect.Where((IntVec3 x) => !buildingsSketch.WouldCollide(pawn.kindDef.race, x, Rot4.North) && !pawnUsedSpots.Contains(x)).TryRandomElement(out result2));
					}
					pawnUsedSpots.Add(result2);
					pawn.position = result2;
					list[i] = pawn;
				}
			}
			return new MechClusterSketch(buildingsSketch, list, startDormant);
		}

		public static void ResolveSketch(ResolveParams parms)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Mech clusters are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 673321);
				return;
			}
			bool canBeDormant = !parms.mechClusterDormant.HasValue || parms.mechClusterDormant.Value;
			float num;
			if (parms.points.HasValue)
			{
				num = parms.points.Value;
			}
			else
			{
				num = 2000f;
				Log.Error("No points given for mech cluster generation. Default to " + num);
			}
			float value = parms.totalPoints.HasValue ? parms.totalPoints.Value : num;
			IntVec2 intVec;
			if (parms.mechClusterSize.HasValue)
			{
				intVec = parms.mechClusterSize.Value;
			}
			else
			{
				int num2 = GenMath.RoundRandom(PointsToSizeCurve.Evaluate(num) * SizeRandomFactorRange.RandomInRange);
				int num3 = GenMath.RoundRandom(PointsToSizeCurve.Evaluate(num) * SizeRandomFactorRange.RandomInRange);
				if (parms.mechClusterForMap != null)
				{
					CellRect cellRect = LargestAreaFinder.FindLargestRect(parms.mechClusterForMap, (IntVec3 x) => !x.Impassable(parms.mechClusterForMap) && x.GetTerrain(parms.mechClusterForMap).affordances.Contains(TerrainAffordanceDefOf.Heavy), Mathf.Max(num2, num3));
					num2 = Mathf.Min(num2, cellRect.Width);
					num3 = Mathf.Min(num3, cellRect.Height);
				}
				intVec = new IntVec2(num2, num3);
			}
			Sketch sketch = new Sketch();
			if (Rand.Chance(WallsChanceCurve.Evaluate(num)))
			{
				ResolveParams parms2 = parms;
				parms2.sketch = sketch;
				parms2.mechClusterSize = intVec;
				SketchResolverDefOf.MechClusterWalls.Resolve(parms2);
			}
			List<ThingDef> buildingDefsForCluster_NewTemp = GetBuildingDefsForCluster_NewTemp(num, intVec, canBeDormant, value);
			AddBuildingsToSketch(sketch, intVec, buildingDefsForCluster_NewTemp);
			parms.sketch.MergeAt(sketch, default(IntVec3), Sketch.SpawnPosType.OccupiedCenter);
		}

		[Obsolete("Only need this overload to not break mod compatibility.")]
		private static List<ThingDef> GetBuildingDefsForCluster(float points, IntVec2 size, bool canBeDormant)
		{
			return GetBuildingDefsForCluster_NewTemp(points, size, canBeDormant, 0f);
		}

		private static List<ThingDef> GetBuildingDefsForCluster_NewTemp(float points, IntVec2 size, bool canBeDormant, float? totalPoints)
		{
			List<ThingDef> list = new List<ThingDef>();
			List<ThingDef> source = DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef def) => def.building != null && def.building.buildingTags != null && def.building.buildingTags.Contains("MechClusterMember") && (!totalPoints.HasValue || (float)def.building.minMechClusterPoints <= totalPoints)).ToList();
			int num = GenMath.RoundRandom(ProblemCauserCountCurve.Evaluate(points));
			for (int i = 0; i < num; i++)
			{
				if (!source.Where((ThingDef x) => x.building.buildingTags.Contains("MechClusterProblemCauser")).TryRandomElementByWeight((ThingDef t) => t.generateCommonality, out ThingDef result))
				{
					break;
				}
				list.Add(result);
			}
			if (canBeDormant)
			{
				if (Rand.Chance(0.5f))
				{
					list.Add(ThingDefOf.ActivatorCountdown);
				}
				if (Rand.Chance(0.5f))
				{
					int num2 = GenMath.RoundRandom(ActivatorProximitysCountCurve.Evaluate(points));
					for (int j = 0; j < num2; j++)
					{
						list.Add(ThingDefOf.ActivatorProximity);
					}
				}
			}
			if (Rand.Chance(GoodBuildingChanceCurve.Evaluate(points)))
			{
				int num3 = Rand.RangeInclusive(0, GenMath.RoundRandom(GoodBuildingMaxCountCurve.Evaluate(points)));
				for (int k = 0; k < num3; k++)
				{
					if (!source.Where((ThingDef x) => x.building.buildingTags.Contains("MechClusterMemberGood")).TryRandomElement(out ThingDef result2))
					{
						break;
					}
					list.Add(result2);
				}
			}
			if (Rand.Chance(BulletShieldChanceCurve.Evaluate(points)))
			{
				points *= 0.85f;
				int num4 = Rand.RangeInclusive(0, GenMath.RoundRandom(BulletShieldMaxCountCurve.Evaluate(points)));
				for (int l = 0; l < num4; l++)
				{
					list.Add(ThingDefOf.ShieldGeneratorBullets);
				}
			}
			if (Rand.Chance(MortarShieldChanceCurve.Evaluate(points)))
			{
				points *= 0.9f;
				list.Add(ThingDefOf.ShieldGeneratorMortar);
			}
			float pointsLeft = points;
			ThingDef thingDef = source.Where((ThingDef x) => x.building.buildingTags.Contains("MechClusterCombatThreat")).MinBy((ThingDef x) => x.building.combatPower);
			ThingDef result3;
			for (pointsLeft = Mathf.Max(pointsLeft, thingDef.building.combatPower); pointsLeft > 0f && source.Where((ThingDef x) => x.building.combatPower <= pointsLeft && x.building.buildingTags.Contains("MechClusterCombatThreat")).TryRandomElement(out result3); pointsLeft -= result3.building.combatPower)
			{
				list.Add(result3);
			}
			return list;
		}

		private static bool TryRandomBuildingWithTag(string tag, List<ThingDef> allowedBuildings, List<ThingDef> generatedBuildings, IntVec2 size, out ThingDef result)
		{
			return allowedBuildings.Where((ThingDef x) => x.building.buildingTags.Contains(tag)).TryRandomElement(out result);
		}

		private static void AddBuildingsToSketch(Sketch sketch, IntVec2 size, List<ThingDef> buildings)
		{
			List<CellRect> edgeWallRects = new List<CellRect>
			{
				new CellRect(0, 0, size.x, 1),
				new CellRect(0, 0, 1, size.z),
				new CellRect(size.x - 1, 0, 1, size.z),
				new CellRect(0, size.z - 1, size.x, 1)
			};
			foreach (ThingDef item in buildings.OrderBy((ThingDef x) => x.building.IsTurret && !x.building.IsMortar))
			{
				bool flag = item.building.IsTurret && !item.building.IsMortar;
				if (!TryFindRandomPlaceFor(item, sketch, size, out IntVec3 pos, lowerLeftQuarterOnly: false, flag, flag, !flag, edgeWallRects) && !TryFindRandomPlaceFor(item, sketch, size + new IntVec2(6, 6), out pos, lowerLeftQuarterOnly: false, flag, flag, !flag, edgeWallRects))
				{
					continue;
				}
				sketch.AddThing(item, pos, Rot4.North, GenStuff.RandomStuffByCommonalityFor(item));
				if (item != ThingDefOf.Turret_AutoMiniTurret)
				{
					continue;
				}
				if (pos.x < size.x / 2)
				{
					if (pos.z < size.z / 2)
					{
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z - 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z - 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					}
					else
					{
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x - 1, 0, pos.z + 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
						sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z + 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					}
				}
				else if (pos.z < size.z / 2)
				{
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z - 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z - 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
				}
				else
				{
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x + 1, 0, pos.z + 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
					sketch.AddThing(ThingDefOf.Barricade, new IntVec3(pos.x, 0, pos.z + 1), Rot4.North, ThingDefOf.Steel, 1, null, null, wipeIfCollides: false);
				}
			}
		}

		private static bool TryFindRandomPlaceFor(ThingDef thingDef, Sketch sketch, IntVec2 size, out IntVec3 pos, bool lowerLeftQuarterOnly, bool avoidCenter, bool requireLOSToEdge, bool avoidEdge, List<CellRect> edgeWallRects)
		{
			for (int i = 0; i < 200; i++)
			{
				CellRect cellRect = new CellRect(0, 0, size.x, size.z);
				if (lowerLeftQuarterOnly)
				{
					cellRect = new CellRect(cellRect.minX, cellRect.minZ, cellRect.Width / 2, cellRect.Height / 2);
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (avoidCenter)
				{
					CellRect cellRect2 = CellRect.CenteredOn(new CellRect(0, 0, size.x, size.z).CenterCell, size.x / 2, size.z / 2);
					for (int j = 0; j < 5; j++)
					{
						if (!cellRect2.Contains(randomCell))
						{
							break;
						}
						randomCell = cellRect.RandomCell;
					}
				}
				if (avoidEdge)
				{
					CellRect cellRect3 = CellRect.CenteredOn(new CellRect(0, 0, size.x, size.z).CenterCell, Mathf.RoundToInt((float)size.x * 0.75f), Mathf.RoundToInt((float)size.z * 0.75f));
					for (int k = 0; k < 5; k++)
					{
						if (cellRect3.Contains(randomCell))
						{
							break;
						}
						randomCell = cellRect.RandomCell;
					}
				}
				if (requireLOSToEdge)
				{
					IntVec3 end = randomCell;
					end.x += size.x + 1;
					IntVec3 end2 = randomCell;
					end2.x -= size.x + 1;
					IntVec3 end3 = randomCell;
					end3.z -= size.z + 1;
					IntVec3 end4 = randomCell;
					end4.z += size.z + 1;
					if (!sketch.LineOfSight(randomCell, end) && !sketch.LineOfSight(randomCell, end2) && !sketch.LineOfSight(randomCell, end3) && !sketch.LineOfSight(randomCell, end4))
					{
						continue;
					}
				}
				if (thingDef.building.minDistanceToSameTypeOfBuilding > 0)
				{
					bool flag = false;
					for (int l = 0; l < sketch.Things.Count; l++)
					{
						if (sketch.Things[l].def == thingDef && sketch.Things[l].pos.InHorDistOf(randomCell, thingDef.building.minDistanceToSameTypeOfBuilding))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				bool flag2 = false;
				CellRect cellRect4 = GenAdj.OccupiedRect(randomCell, Rot4.North, thingDef.Size);
				for (int m = 0; m < 4; m++)
				{
					if (cellRect4.Overlaps(edgeWallRects[m]))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2 && !sketch.WouldCollide(thingDef, randomCell, Rot4.North))
				{
					pos = randomCell;
					return true;
				}
			}
			pos = IntVec3.Invalid;
			return false;
		}

		[DebugOutput]
		public static void MechClusterBuildingSelection()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (float item in DebugActionsUtility.PointsOptions(extended: false))
			{
				float localPoints = item;
				list.Add(new DebugMenuOption(item.ToString("F0"), DebugMenuOptionMode.Action, delegate
				{
					string text = "";
					for (int i = 0; i < 50; i++)
					{
						int num = Rand.Range(10, 20);
						List<ThingDef> buildingDefsForCluster_NewTemp = GetBuildingDefsForCluster_NewTemp(localPoints, new IntVec2(num, num), canBeDormant: true, localPoints);
						text = text + "points: " + localPoints + " , size: " + num;
						foreach (ThingDef item2 in buildingDefsForCluster_NewTemp)
						{
							text = text + "\n- " + item2.defName;
						}
						text += "\n\n";
					}
					Log.Message(text);
				}));
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}
	}
}
