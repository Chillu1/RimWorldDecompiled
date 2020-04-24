namespace RimWorld
{
	public class GameCondition_ColdSnap : GameCondition
	{
		private const float MaxTempOffset = -20f;

		public override int TransitionTicks => 12000;

		public override float TemperatureOffset()
		{
			return GameConditionUtility.LerpInOutValue(this, TransitionTicks, -20f);
		}
	}
}
