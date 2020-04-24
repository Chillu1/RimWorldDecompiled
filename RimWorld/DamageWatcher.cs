using Verse;

namespace RimWorld
{
	public class DamageWatcher : IExposable
	{
		private float everDamage;

		public float DamageTakenEver => everDamage;

		public void Notify_DamageTaken(Thing damagee, float amount)
		{
			if (damagee.Faction == Faction.OfPlayer)
			{
				everDamage += amount;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref everDamage, "everDamage", 0f);
		}
	}
}
