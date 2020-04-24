namespace Verse
{
	public class CompProperties_TemperatureDamaged : CompProperties
	{
		public FloatRange safeTemperatureRange = new FloatRange(-30f, 30f);

		public int damagePerTickRare = 1;

		public CompProperties_TemperatureDamaged()
		{
			compClass = typeof(CompTemperatureDamaged);
		}
	}
}
