using Verse;

namespace RimWorld
{
	public class CompStunOnDamage : ThingComp
	{
		private CompProperties_StunOnDamage Props => (CompProperties_StunOnDamage)props;

		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (dinfo.Def == Props.damage)
			{
				((Pawn)parent).stances?.stunner?.StunFor(Props.delayTicks, dinfo.Instigator, addBattleLog: false);
			}
		}
	}
}
