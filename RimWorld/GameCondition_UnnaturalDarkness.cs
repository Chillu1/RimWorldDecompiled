using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class GameCondition_UnnaturalDarkness : GameCondition_ForceWeather
{
	public bool anyColonistAttacked;

	private List<SkyOverlay> overlays = new List<SkyOverlay>
	{
		new WeatherOverlay_UnnaturalDarkness()
	};

	public override int TransitionTicks => 300;

	public override void Init()
	{
		if (!ModLister.CheckAnomaly("Unnatural darkness"))
		{
			End();
		}
		else
		{
			base.Init();
		}
	}

	public override float SkyTargetLerpFactor(Map map)
	{
		return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
	}

	public override SkyTarget? SkyTarget(Map map)
	{
		return new SkyTarget(0f, GameCondition_NoSunlight.EclipseSkyColors, 1f, 0f);
	}

	public override WeatherDef ForcedWeather()
	{
		Map map = base.AffectedMaps.FirstOrDefault();
		if (map != null)
		{
			if (map.gameConditionManager.MapBrightness > 0.99f)
			{
				return WeatherDefOf.UnnaturalDarkness_Stage1;
			}
			return WeatherDefOf.UnnaturalDarkness_Stage2;
		}
		return null;
	}

	public override void GameConditionTick()
	{
		base.GameConditionTick();
		List<Map> affectedMaps = base.AffectedMaps;
		for (int i = 0; i < affectedMaps.Count; i++)
		{
			for (int j = 0; j < overlays.Count; j++)
			{
				overlays[j].TickOverlay(affectedMaps[i], 1f);
			}
			if (!GenTicks.IsTickInterval(60))
			{
				continue;
			}
			foreach (Pawn item in affectedMaps[i].mapPawns.AllHumanlikeSpawned)
			{
				if (AffectedByDarkness(item) && item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DarknessExposure) == null)
				{
					item.health.AddHediff(HediffDefOf.DarknessExposure);
				}
			}
		}
	}

	public static bool AffectedByDarkness(Pawn pawn)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (pawn.Spawned && pawn.RaceProps.Humanlike && !pawn.Downed)
		{
			if (!pawn.IsColonistPlayerControlled)
			{
				return pawn.IsColonySubhumanPlayerControlled;
			}
			return true;
		}
		return false;
	}

	public override void End()
	{
		base.End();
		foreach (Map affectedMap in base.AffectedMaps)
		{
			affectedMap.weatherDecider.StartNextWeather();
		}
	}

	public override void GameConditionDraw(Map map)
	{
		if (!(map.GameConditionManager.MapBrightness > 0.5f))
		{
			for (int i = 0; i < overlays.Count; i++)
			{
				overlays[i].DrawOverlay(map);
			}
		}
	}

	public override List<SkyOverlay> SkyOverlays(Map map)
	{
		return overlays;
	}

	public static bool InUnnaturalDarkness(Pawn p)
	{
		if (!p.SpawnedOrAnyParentSpawned)
		{
			return false;
		}
		return UnnaturalDarknessAt(p.PositionHeld, p.MapHeld);
	}

	public static bool UnnaturalDarknessOnMap(Map map)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (map == null)
		{
			return false;
		}
		if (map.gameConditionManager.MapBrightness > 0.01f)
		{
			return false;
		}
		if (!map.gameConditionManager.ConditionIsActive(GameConditionDefOf.UnnaturalDarkness))
		{
			return false;
		}
		return true;
	}

	public static bool UnnaturalDarknessAt(IntVec3 cell, Map map)
	{
		if (!UnnaturalDarknessOnMap(map))
		{
			return false;
		}
		Building_Door door = cell.GetDoor(map);
		if (door != null)
		{
			float num = 0f;
			int num2 = 0;
			foreach (IntVec3 edgeCellsNoCorner in door.OccupiedRect().ExpandedBy(1).EdgeCellsNoCorners)
			{
				if (edgeCellsNoCorner.InBounds(map) && !edgeCellsNoCorner.Filled(map))
				{
					num += map.glowGrid.GroundGlowAt(edgeCellsNoCorner);
					num2++;
				}
			}
			if (num2 > 0)
			{
				return num / (float)num2 <= 0f;
			}
		}
		return map.glowGrid.GroundGlowAt(cell) <= 0f;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref anyColonistAttacked, "anyColonistAttacked", defaultValue: false);
	}
}
