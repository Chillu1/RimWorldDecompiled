using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_AnimalFilth : Alert
{
	private List<GlobalTargetInfo> targets = new List<GlobalTargetInfo>();

	private List<string> pawnEntries = new List<string>();

	private const float MinFilthRate = 4f;

	private const float MinFilthMultiplier = 0.5f;

	public Alert_AnimalFilth()
	{
		defaultLabel = "AlertAnimalFilth".Translate();
	}

	private void CalculateTargets()
	{
		targets.Clear();
		pawnEntries.Clear();
		foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
		{
			if (item.HostFaction != null || item.RaceProps.Humanlike || (ModsConfig.AnomalyActive && item.IsShambler) || !(item.GetStatValue(StatDefOf.FilthRate) >= 4f))
			{
				continue;
			}
			IntVec3 position = item.Position;
			Map map = item.Map;
			TerrainDef terrain = position.GetTerrain(map);
			if (!(terrain.GetStatValueAbstract(StatDefOf.FilthMultiplier) <= 0.5f) && FilthMaker.TerrainAcceptsFilth(terrain, ThingDefOf.Filth_AnimalFilth, FilthSourceFlags.Pawn) && position.GetRoof(map) != null)
			{
				Room room = position.GetRoom(map);
				if (room != null && !room.Fogged && !room.TouchesMapEdge && !room.IsDoorway && item.Map.areaManager.Home[item.Position])
				{
					targets.Add(item);
					pawnEntries.Add(item.NameShortColored.Resolve());
				}
			}
		}
	}

	public override TaggedString GetExplanation()
	{
		return "AlertAnimalFilthDesc".Translate(pawnEntries.ToLineList("  - "));
	}

	public override AlertReport GetReport()
	{
		CalculateTargets();
		return AlertReport.CulpritsAre(targets);
	}
}
