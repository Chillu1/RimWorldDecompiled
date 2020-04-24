namespace RimWorld
{
	public class CompProperties_Battery : CompProperties_Power
	{
		public float storedEnergyMax = 1000f;

		public float efficiency = 0.5f;

		public CompProperties_Battery()
		{
			compClass = typeof(CompPowerBattery);
		}
	}
}
