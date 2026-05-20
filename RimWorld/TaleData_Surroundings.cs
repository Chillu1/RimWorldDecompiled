using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class TaleData_Surroundings : TaleData
{
	public PlanetTile tile;

	public BiomeDef biome;

	public float temperature;

	public float snowDepth;

	public WeatherDef weather;

	public RoomRoleDef roomRole;

	public float roomImpressiveness;

	public float roomBeauty;

	public float roomCleanliness;

	public bool Outdoors => weather != null;

	public override void ExposeData()
	{
		Scribe_Values.Look(ref tile, "tile");
		Scribe_Values.Look(ref temperature, "temperature", 0f);
		Scribe_Values.Look(ref snowDepth, "snowDepth", 0f);
		Scribe_Defs.Look(ref weather, "weather");
		Scribe_Defs.Look(ref roomRole, "roomRole");
		Scribe_Defs.Look(ref biome, "biome");
		Scribe_Values.Look(ref roomImpressiveness, "roomImpressiveness", 0f);
		Scribe_Values.Look(ref roomBeauty, "roomBeauty", 0f);
		Scribe_Values.Look(ref roomCleanliness, "roomCleanliness", 0f);
	}

	public override IEnumerable<Rule> GetRules(Dictionary<string, string> constants = null)
	{
		if (biome == null)
		{
			yield return new Rule_String("BIOME", Find.WorldGrid[tile].PrimaryBiome.label);
		}
		else
		{
			yield return new Rule_String("BIOME", biome.label);
		}
		if (roomRole != null && roomRole != RoomRoleDefOf.None)
		{
			yield return new Rule_String("ROOM_role", roomRole.label);
			yield return new Rule_String("ROOM_roleDefinite", Find.ActiveLanguageWorker.WithDefiniteArticle(roomRole.label));
			yield return new Rule_String("ROOM_roleIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(roomRole.label));
			RoomStatScoreStage impressiveness = RoomStatDefOf.Impressiveness.GetScoreStage(roomImpressiveness);
			RoomStatScoreStage beauty = RoomStatDefOf.Beauty.GetScoreStage(roomBeauty);
			RoomStatScoreStage cleanliness = RoomStatDefOf.Cleanliness.GetScoreStage(roomCleanliness);
			yield return new Rule_String("ROOM_impressiveness", impressiveness.label);
			yield return new Rule_String("ROOM_impressivenessIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(impressiveness.label));
			yield return new Rule_String("ROOM_beauty", beauty.label);
			yield return new Rule_String("ROOM_beautyIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(beauty.label));
			yield return new Rule_String("ROOM_cleanliness", cleanliness.label);
			yield return new Rule_String("ROOM_cleanlinessIndefinite", Find.ActiveLanguageWorker.WithIndefiniteArticle(cleanliness.label));
		}
	}

	public static TaleData_Surroundings GenerateFrom(IntVec3 c, Map map)
	{
		TaleData_Surroundings taleData_Surroundings = new TaleData_Surroundings
		{
			tile = map.Tile,
			biome = map.Biome
		};
		Room roomOrAdjacent = c.GetRoomOrAdjacent(map, RegionType.Set_All);
		if (roomOrAdjacent != null)
		{
			if (roomOrAdjacent.PsychologicallyOutdoors)
			{
				taleData_Surroundings.weather = map.weatherManager.CurWeatherPerceived;
			}
			taleData_Surroundings.roomRole = roomOrAdjacent.Role;
			taleData_Surroundings.roomImpressiveness = roomOrAdjacent.GetStat(RoomStatDefOf.Impressiveness);
			taleData_Surroundings.roomBeauty = roomOrAdjacent.GetStat(RoomStatDefOf.Beauty);
			taleData_Surroundings.roomCleanliness = roomOrAdjacent.GetStat(RoomStatDefOf.Cleanliness);
		}
		if (!GenTemperature.TryGetTemperatureForCell(c, map, out taleData_Surroundings.temperature))
		{
			taleData_Surroundings.temperature = 21f;
		}
		taleData_Surroundings.snowDepth = map.snowGrid.GetDepth(c);
		return taleData_Surroundings;
	}

	public static TaleData_Surroundings GenerateRandom(Map map)
	{
		return GenerateFrom(CellFinder.RandomCell(map), map);
	}
}
