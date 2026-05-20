using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IncidentWorker_MeteoriteImpact : IncidentWorker
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		IntVec3 cell;
		return TryFindCell(out cell, map);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		List<Thing> contents;
		Skyfaller skyfaller = SpawnMeteoriteIncoming(parms, out contents);
		if (skyfaller == null)
		{
			return false;
		}
		skyfaller.impactLetter = MakeLetter(skyfaller, contents);
		return true;
	}

	protected virtual List<Thing> GenerateMeteorContents(IncidentParms parms)
	{
		return ThingSetMakerDefOf.Meteorite.root.Generate();
	}

	protected Skyfaller SpawnMeteoriteIncoming(IncidentParms parms, out List<Thing> contents)
	{
		Map map = (Map)parms.target;
		if (!TryFindCell(out var cell, map))
		{
			contents = null;
			return null;
		}
		contents = GenerateMeteorContents(parms);
		return SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, contents, cell, map);
	}

	protected virtual Letter MakeLetter(Skyfaller meteorite, List<Thing> contents)
	{
		Thing thing = contents?[0];
		if (thing == null)
		{
			return null;
		}
		LetterDef letterDef = (thing.def.building.isResourceRock ? LetterDefOf.PositiveEvent : LetterDefOf.NeutralEvent);
		string text = def.letterText.Formatted(thing.def.label).CapitalizeFirst();
		return LetterMaker.MakeLetter(def.letterLabel + ": " + thing.def.LabelCap, text, letterDef, new TargetInfo(meteorite.Position, meteorite.Map));
	}

	private static bool TryFindCell(out IntVec3 cell, Map map)
	{
		int maxMineables = ThingSetMaker_Meteorite.MineablesCountRange.max;
		return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, TerrainAffordanceDefOf.Light, out cell, 10, default(IntVec3), -1, allowRoofedCells: true, allowCellsWithItems: false, allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: true, alwaysAvoidColonists: true, delegate(IntVec3 x)
		{
			int num = Mathf.CeilToInt(Mathf.Sqrt(maxMineables)) + 2;
			CellRect other = CellRect.CenteredOn(x, num, num);
			int num2 = 0;
			foreach (IntVec3 item in other)
			{
				if (item.InBounds(map) && item.Standable(map))
				{
					num2++;
				}
			}
			if (ModsConfig.RoyaltyActive)
			{
				foreach (Thing item2 in map.listerThings.ThingsOfDef(ThingDefOf.MonumentMarker))
				{
					MonumentMarker monumentMarker = item2 as MonumentMarker;
					if (monumentMarker.AllDone && monumentMarker.sketch.OccupiedRect.ExpandedBy(3).MovedBy(monumentMarker.Position).Overlaps(other))
					{
						return false;
					}
				}
			}
			return num2 >= maxMineables;
		});
	}
}
