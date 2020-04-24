using RimWorld;

namespace Verse
{
	public class CompTemperatureDamaged : ThingComp
	{
		public CompProperties_TemperatureDamaged Props => (CompProperties_TemperatureDamaged)props;

		public override void CompTick()
		{
			if (Find.TickManager.TicksGame % 250 == 0)
			{
				CheckTakeDamage();
			}
		}

		public override void CompTickRare()
		{
			CheckTakeDamage();
		}

		private void CheckTakeDamage()
		{
			if (!Props.safeTemperatureRange.Includes(parent.AmbientTemperature))
			{
				parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, Props.damagePerTickRare));
			}
		}
	}
}
