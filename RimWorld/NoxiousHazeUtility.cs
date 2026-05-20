using RimWorld.Planet;
using Verse;

namespace RimWorld;

public static class NoxiousHazeUtility
{
	public static bool IsExposedToNoxiousHaze(Thing thing)
	{
		if (!thing.SpawnedOrAnyParentSpawned)
		{
			return false;
		}
		return IsExposedToNoxiousHaze(thing, thing.PositionHeld, thing.MapHeld);
	}

	public static bool IsExposedToNoxiousHaze(Thing thing, IntVec3 cell, Map map)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		GameCondition activeCondition = map.gameConditionManager.GetActiveCondition(GameConditionDefOf.NoxiousHaze);
		if (activeCondition == null || activeCondition.HiddenByOtherCondition(map))
		{
			return false;
		}
		if (thing is Pawn pawn && pawn.kindDef.immuneToGameConditionEffects)
		{
			return false;
		}
		if (thing.def.category == ThingCategory.Item)
		{
			return !cell.Roofed(map);
		}
		if (cell.Roofed(map))
		{
			return cell.GetRoom(map).PsychologicallyOutdoors;
		}
		return true;
	}

	public static bool TryGetNoxiousHazeMTB(PlanetTile tile, out float mtb)
	{
		float num = WorldPollutionUtility.CalculateNearbyPollutionScore(tile);
		if (num > 0f && num >= GameConditionDefOf.NoxiousHaze.minNearbyPollution)
		{
			mtb = GameConditionDefOf.NoxiousHaze.mtbOverNearbyPollutionCurve.Evaluate(num);
			return true;
		}
		mtb = 0f;
		return false;
	}
}
