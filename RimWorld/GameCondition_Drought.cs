using Verse;

namespace RimWorld;

public class GameCondition_Drought : GameCondition
{
	public const float PlantGrowthRateFactor = 0.3f;

	public override void GameConditionTick()
	{
		HandleEndDueToRain();
	}

	protected bool HandleEndDueToRain()
	{
		if (GenTicks.IsTickInterval(60) && base.SingleMap.weatherManager.curWeather.rainRate > 0f)
		{
			End();
			return true;
		}
		return false;
	}
}
