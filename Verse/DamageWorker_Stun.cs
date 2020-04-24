namespace Verse
{
	public class DamageWorker_Stun : DamageWorker
	{
		public override DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			DamageResult damageResult = base.Apply(dinfo, victim);
			damageResult.stunned = true;
			return damageResult;
		}
	}
}
