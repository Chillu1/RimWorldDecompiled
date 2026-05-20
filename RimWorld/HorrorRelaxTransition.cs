using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class HorrorRelaxTransition : MusicTransition
{
	public override bool IsTransitionSatisfied()
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!base.IsTransitionSatisfied())
		{
			return false;
		}
		foreach (Map map in Find.Maps)
		{
			if (map.weatherManager.curWeather == WeatherDefOf.GrayPall || map.gameConditionManager.ConditionIsActive(GameConditionDefOf.GrayPall))
			{
				return true;
			}
		}
		foreach (PocketMapParent pocketMap in Find.World.pocketMaps)
		{
			if (pocketMap.Map != null && IsValidPocketMap(pocketMap))
			{
				return true;
			}
		}
		foreach (Map map2 in Find.Maps)
		{
			if (IsValidMap(map2))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsValidPocketMap(PocketMapParent pocketMap)
	{
		if (pocketMap.Map.generatorDef != MapGeneratorDefOf.Labyrinth)
		{
			return pocketMap.Map.generatorDef == MapGeneratorDefOf.Undercave;
		}
		return true;
	}

	private static bool IsValidMap(Map map)
	{
		if (!map.listerThings.AnyThingWithDef(ThingDefOf.PitGate) && !map.listerThings.AnyThingWithDef(ThingDefOf.FleshmassHeart))
		{
			return map.listerThings.AnyThingWithDef(ThingDefOf.Noctolith);
		}
		return true;
	}
}
