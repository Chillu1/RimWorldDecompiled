using Verse;

namespace RimWorld
{
	public static class SkyfallerShrapnelUtility
	{
		private const float ShrapnelDistanceFront = 6f;

		private const float ShrapnelDistanceSide = 4f;

		private const float ShrapnelDistanceBack = 30f;

		private const int MotesPerShrapnel = 2;

		private static readonly SimpleCurve ShrapnelDistanceFromAngle = new SimpleCurve
		{
			new CurvePoint(0f, 6f),
			new CurvePoint(90f, 4f),
			new CurvePoint(135f, 4f),
			new CurvePoint(180f, 30f),
			new CurvePoint(225f, 4f),
			new CurvePoint(270f, 4f),
			new CurvePoint(360f, 6f)
		};

		private static readonly SimpleCurve ShrapnelAngleDistribution = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(0.1f, 90f),
			new CurvePoint(0.25f, 135f),
			new CurvePoint(0.5f, 180f),
			new CurvePoint(0.75f, 225f),
			new CurvePoint(0.9f, 270f),
			new CurvePoint(1f, 360f)
		};

		public static void MakeShrapnel(IntVec3 center, Map map, float angle, float distanceFactor, int metalShrapnelCount, int rubbleShrapnelCount, bool spawnMotes)
		{
			angle -= 90f;
			SpawnShrapnel(ThingDefOf.ChunkSlagSteel, metalShrapnelCount, center, map, angle, distanceFactor);
			SpawnShrapnel(ThingDefOf.Filth_RubbleBuilding, rubbleShrapnelCount, center, map, angle, distanceFactor);
			if (spawnMotes)
			{
				ThrowShrapnelMotes((metalShrapnelCount + rubbleShrapnelCount) * 2, center, map, angle, distanceFactor);
			}
		}

		private static void SpawnShrapnel(ThingDef def, int quantity, IntVec3 center, Map map, float angle, float distanceFactor)
		{
			for (int i = 0; i < quantity; i++)
			{
				IntVec3 intVec = GenerateShrapnelLocation(center, angle, distanceFactor);
				if (IsGoodShrapnelCell(intVec, map) && (def.category != ThingCategory.Item || intVec.GetFirstItem(map) == null) && intVec.GetFirstThing(map, def) == null)
				{
					GenSpawn.Spawn(def, intVec, map);
				}
			}
		}

		private static void ThrowShrapnelMotes(int count, IntVec3 center, Map map, float angle, float distanceFactor)
		{
			for (int i = 0; i < count; i++)
			{
				IntVec3 c = GenerateShrapnelLocation(center, angle, distanceFactor);
				if (IsGoodShrapnelCell(c, map))
				{
					MoteMaker.ThrowDustPuff(c.ToVector3Shifted() + Gen.RandomHorizontalVector(0.5f), map, 2f);
				}
			}
		}

		private static bool IsGoodShrapnelCell(IntVec3 c, Map map)
		{
			if (!c.InBounds(map))
			{
				return false;
			}
			if (c.Impassable(map) || c.Filled(map))
			{
				return false;
			}
			if (map.roofGrid.RoofAt(c) != null)
			{
				return false;
			}
			return true;
		}

		private static IntVec3 GenerateShrapnelLocation(IntVec3 center, float angleOffset, float distanceFactor)
		{
			float num = ShrapnelAngleDistribution.Evaluate(Rand.Value);
			float d = ShrapnelDistanceFromAngle.Evaluate(num) * Rand.Value * distanceFactor;
			return (Vector3Utility.HorizontalVectorFromAngle(num + angleOffset) * d).ToIntVec3() + center;
		}
	}
}
