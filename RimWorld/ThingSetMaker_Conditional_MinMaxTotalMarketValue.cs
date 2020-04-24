namespace RimWorld
{
	public class ThingSetMaker_Conditional_MinMaxTotalMarketValue : ThingSetMaker_Conditional
	{
		public float minMaxTotalMarketValue;

		protected override bool Condition(ThingSetMakerParams parms)
		{
			if (parms.totalMarketValueRange.HasValue)
			{
				return parms.totalMarketValueRange.Value.max >= minMaxTotalMarketValue;
			}
			return false;
		}
	}
}
