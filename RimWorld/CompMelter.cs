using Verse;

namespace RimWorld
{
	public class CompMelter : ThingComp
	{
		private const float MeltPerIntervalPer10Degrees = 0.15f;

		public override void CompTickRare()
		{
			float ambientTemperature = parent.AmbientTemperature;
			if (!(ambientTemperature < 0f))
			{
				int num = GenMath.RoundRandom(0.15f * (ambientTemperature / 10f));
				if (num > 0)
				{
					parent.TakeDamage(new DamageInfo(DamageDefOf.Rotting, num));
				}
			}
		}
	}
}
