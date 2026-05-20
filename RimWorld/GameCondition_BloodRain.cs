using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GameCondition_BloodRain : GameCondition_ForceWeather
{
	private List<SkyOverlay> overlays = new List<SkyOverlay>();

	public override int TransitionTicks => 300;

	public override float SkyTargetLerpFactor(Map map)
	{
		return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
	}

	public override void Init()
	{
		if (!ModLister.CheckAnomaly("Blood rain"))
		{
			End();
		}
		else
		{
			base.Init();
		}
	}

	public override void GameConditionTick()
	{
		base.GameConditionTick();
		List<Map> affectedMaps = base.AffectedMaps;
		for (int i = 0; i < overlays.Count; i++)
		{
			for (int j = 0; j < affectedMaps.Count; j++)
			{
				overlays[i].TickOverlay(affectedMaps[j], 1f);
			}
		}
	}

	public override void GameConditionDraw(Map map)
	{
		for (int i = 0; i < overlays.Count; i++)
		{
			overlays[i].DrawOverlay(map);
		}
	}

	public override void End()
	{
		base.End();
		base.SingleMap.weatherDecider.StartNextWeather();
	}
}
