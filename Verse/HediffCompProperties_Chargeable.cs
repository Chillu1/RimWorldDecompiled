namespace Verse
{
	public class HediffCompProperties_Chargeable : HediffCompProperties
	{
		public int ticksToFullCharge = -1;

		public float initialCharge;

		public float fullChargeAmount = 1f;

		public float minChargeToActivate;

		public string labelInBrackets;

		public HediffCompProperties_Chargeable()
		{
			compClass = typeof(HediffComp_Chargeable);
		}
	}
}
