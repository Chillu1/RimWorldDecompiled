using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GameCondition_NoxiousHaze : GameCondition
{
	private const float MaxSkyLerpFactor = 0.5f;

	private const float SkyGlow = 0.45f;

	private static readonly SkyColorSet NoxiousHazeColors = new SkyColorSet(new ColorInt(216, 255, 0).ToColor, new ColorInt(234, 200, 255).ToColor, new ColorInt(207, 248, 0).ToColor, 0.45f);

	private List<SkyOverlay> overlays = new List<SkyOverlay>
	{
		new WeatherOverlay_NoxiousHaze()
	};

	public const float PlantGrowthRateFactor = 0.5f;

	public override int TransitionTicks => 5000;

	public override string Description
	{
		get
		{
			float num = WorldPollutionUtility.CalculateNearbyPollutionScore(base.SingleMap.Tile);
			float num2 = def.mtbOverNearbyPollutionCurve.Evaluate(num);
			return def.description.Formatted(num.ToStringByStyle(ToStringStyle.FloatTwo).Named("NEARBYPOLLUTION"), num2.Named("MTB"));
		}
	}

	public override void GameConditionTick()
	{
		List<Map> affectedMaps = base.AffectedMaps;
		foreach (SkyOverlay overlay in overlays)
		{
			foreach (Map item in affectedMaps)
			{
				if (!HiddenByOtherCondition(item))
				{
					overlay.TickOverlay(item, 1f);
				}
			}
		}
	}

	public override void GameConditionDraw(Map map)
	{
		if (!HiddenByOtherCondition(map))
		{
			for (int i = 0; i < overlays.Count; i++)
			{
				overlays[i].DrawOverlay(map);
			}
		}
	}

	public override float SkyTargetLerpFactor(Map map)
	{
		return GameConditionUtility.LerpInOutValue(this, TransitionTicks, 0.5f);
	}

	public override SkyTarget? SkyTarget(Map map)
	{
		return new SkyTarget(0.45f, NoxiousHazeColors, 1f, 1f);
	}

	public override float AnimalDensityFactor(Map map)
	{
		return HiddenByOtherCondition(map) ? 1 : 0;
	}

	public override float PlantDensityFactor(Map map)
	{
		return HiddenByOtherCondition(map) ? 1 : 0;
	}

	public override bool AllowEnjoyableOutsideNow(Map map)
	{
		return HiddenByOtherCondition(map);
	}

	public override List<SkyOverlay> SkyOverlays(Map map)
	{
		return overlays;
	}
}
