using Verse;

namespace RimWorld;

public class GameCondition_GrayPall : GameCondition_ForceWeather
{
	public override int TransitionTicks => 180;

	public override void Init()
	{
		if (!ModLister.CheckAnomaly("Grey pall"))
		{
			End();
		}
		else
		{
			base.Init();
		}
	}

	public override void End()
	{
		base.End();
		base.SingleMap.weatherDecider.StartNextWeather();
	}
}
