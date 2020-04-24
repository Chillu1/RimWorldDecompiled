using RimWorld;

namespace Verse
{
	public class DamageWorker_Extinguish : DamageWorker
	{
		private const float DamageAmountToFireSizeRatio = 0.01f;

		public override DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			DamageResult result = new DamageResult();
			Fire fire = victim as Fire;
			if (fire == null || fire.Destroyed)
			{
				return result;
			}
			base.Apply(dinfo, victim);
			fire.fireSize -= dinfo.Amount * 0.01f;
			if (fire.fireSize <= 0.1f)
			{
				fire.Destroy();
			}
			return result;
		}
	}
}
