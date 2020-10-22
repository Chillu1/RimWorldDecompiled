using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class GenStep_ScatterThings : GenStep_Scatterer
	{
		public ThingDef thingDef;

		public ThingDef stuff;

		public int clearSpaceSize;

		public int clusterSize = 1;

		public float terrainValidationRadius;

		[NoTranslate]
		private List<string> terrainValidationDisallowed;

		[Unsaved(false)]
		private IntVec3 clusterCenter;

		[Unsaved(false)]
		private int leftInCluster;

		private const int ClusterRadius = 4;

		private List<Rot4> possibleRotationsInt;

		private static List<Rot4> tmpRotations = new List<Rot4>();

		public override int SeedPart => 1158116095;

		private List<Rot4> PossibleRotations
		{
			get
			{
				if (possibleRotationsInt == null)
				{
					possibleRotationsInt = new List<Rot4>();
					if (thingDef.rotatable)
					{
						possibleRotationsInt.Add(Rot4.North);
						possibleRotationsInt.Add(Rot4.East);
						possibleRotationsInt.Add(Rot4.South);
						possibleRotationsInt.Add(Rot4.West);
					}
					else
					{
						possibleRotationsInt.Add(Rot4.North);
					}
				}
				return possibleRotationsInt;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			if (!allowInWaterBiome && map.TileInfo.WaterCovered)
			{
				return;
			}
			int count = CalculateFinalCount(map);
			IntRange stackSizeRange = ((thingDef.ingestible != null && thingDef.ingestible.IsMeal && thingDef.stackLimit <= 10) ? IntRange.one : ((thingDef.stackLimit > 5) ? new IntRange(Mathf.RoundToInt((float)thingDef.stackLimit * 0.5f), thingDef.stackLimit) : new IntRange(thingDef.stackLimit, thingDef.stackLimit)));
			List<int> list = CountDividedIntoStacks(count, stackSizeRange);
			for (int i = 0; i < list.Count; i++)
			{
				if (!TryFindScatterCell(map, out var result))
				{
					return;
				}
				ScatterAt(result, map, parms, list[i]);
				usedSpots.Add(result);
			}
			usedSpots.Clear();
			clusterCenter = IntVec3.Invalid;
			leftInCluster = 0;
		}

		protected override bool TryFindScatterCell(Map map, out IntVec3 result)
		{
			if (clusterSize > 1)
			{
				if (leftInCluster <= 0)
				{
					if (!base.TryFindScatterCell(map, out clusterCenter))
					{
						Log.Error("Could not find cluster center to scatter " + thingDef);
					}
					leftInCluster = clusterSize;
				}
				leftInCluster--;
				result = CellFinder.RandomClosewalkCellNear(clusterCenter, map, 4, (IntVec3 x) => TryGetRandomValidRotation(x, map, out var _));
				return result.IsValid;
			}
			return base.TryFindScatterCell(map, out result);
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1)
		{
			if (!TryGetRandomValidRotation(loc, map, out var rot))
			{
				Log.Warning("Could not find any valid rotation for " + thingDef);
				return;
			}
			if (clearSpaceSize > 0)
			{
				foreach (IntVec3 item in GridShapeMaker.IrregularLump(loc, map, clearSpaceSize))
				{
					item.GetEdifice(map)?.Destroy();
				}
			}
			Thing thing = ThingMaker.MakeThing(thingDef, stuff);
			if (thingDef.Minifiable)
			{
				thing = thing.MakeMinified();
			}
			if (thing.def.category == ThingCategory.Item)
			{
				thing.stackCount = stackCount;
				thing.SetForbidden(value: true, warnOnFail: false);
				GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Near, out var lastResultingThing);
				if (nearPlayerStart && lastResultingThing != null && lastResultingThing.def.category == ThingCategory.Item && TutorSystem.TutorialMode)
				{
					Find.TutorialState.AddStartingItem(lastResultingThing);
				}
			}
			else
			{
				GenSpawn.Spawn(thing, loc, map, rot);
			}
		}

		protected override bool CanScatterAt(IntVec3 loc, Map map)
		{
			if (!base.CanScatterAt(loc, map))
			{
				return false;
			}
			if (!TryGetRandomValidRotation(loc, map, out var _))
			{
				return false;
			}
			if (terrainValidationRadius > 0f)
			{
				foreach (IntVec3 item in GenRadial.RadialCellsAround(loc, terrainValidationRadius, useCenter: true))
				{
					if (!item.InBounds(map))
					{
						continue;
					}
					TerrainDef terrain = item.GetTerrain(map);
					for (int i = 0; i < terrainValidationDisallowed.Count; i++)
					{
						if (terrain.HasTag(terrainValidationDisallowed[i]))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private bool TryGetRandomValidRotation(IntVec3 loc, Map map, out Rot4 rot)
		{
			List<Rot4> possibleRotations = PossibleRotations;
			for (int i = 0; i < possibleRotations.Count; i++)
			{
				if (IsRotationValid(loc, possibleRotations[i], map))
				{
					tmpRotations.Add(possibleRotations[i]);
				}
			}
			if (tmpRotations.TryRandomElement(out rot))
			{
				tmpRotations.Clear();
				return true;
			}
			rot = Rot4.Invalid;
			return false;
		}

		private bool IsRotationValid(IntVec3 loc, Rot4 rot, Map map)
		{
			if (!GenAdj.OccupiedRect(loc, rot, thingDef.size).InBounds(map))
			{
				return false;
			}
			if (GenSpawn.WouldWipeAnythingWith(loc, rot, thingDef, map, (Thing x) => x.def == thingDef || (x.def.category != ThingCategory.Plant && x.def.category != ThingCategory.Filth)))
			{
				return false;
			}
			return true;
		}

		public static List<int> CountDividedIntoStacks(int count, IntRange stackSizeRange)
		{
			List<int> list = new List<int>();
			while (count > 0)
			{
				int num = Mathf.Min(count, stackSizeRange.RandomInRange);
				count -= num;
				list.Add(num);
			}
			if (stackSizeRange.max > 2)
			{
				for (int i = 0; i < list.Count * 4; i++)
				{
					int num2 = Rand.RangeInclusive(0, list.Count - 1);
					int num3 = Rand.RangeInclusive(0, list.Count - 1);
					if (num2 != num3 && list[num2] > list[num3])
					{
						int num4 = (int)((float)(list[num2] - list[num3]) * Rand.Value);
						list[num2] -= num4;
						list[num3] += num4;
					}
				}
			}
			return list;
		}
	}
}
