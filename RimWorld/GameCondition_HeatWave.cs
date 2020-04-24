namespace RimWorld
{
	public class GameCondition_HeatWave : GameCondition
	{
		private const float MaxTempOffset = 17f;

		public override int TransitionTicks => 12000;

		public override float TemperatureOffset()
		{
			return GameConditionUtility.LerpInOutValue(this, TransitionTicks, 17f);
		}
	}
}
