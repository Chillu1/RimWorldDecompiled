using Verse;

namespace RimWorld
{
	public class CompProperties_Shield : CompProperties
	{
		public int startingTicksToReset = 3200;

		public float minDrawSize = 1.2f;

		public float maxDrawSize = 1.55f;

		public float energyLossPerDamage = 0.033f;

		public float energyOnReset = 0.2f;

		public bool blocksRangedWeapons = true;

		public CompProperties_Shield()
		{
			compClass = typeof(CompShield);
		}
	}
}
