using Verse;

namespace RimWorld
{
	public class SmokepopBelt : Apparel
	{
		private float ApparelScorePerBeltRadius = 0.046f;

		public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			if (!dinfo.Def.isExplosive && dinfo.Def.harmsHealth && dinfo.Def.ExternalViolenceFor(this) && dinfo.Def.isRanged && base.Wearer.Spawned)
			{
				GenExplosion.DoExplosion(base.Wearer.Position, base.Wearer.Map, this.GetStatValue(StatDefOf.SmokepopBeltRadius), DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, ThingDefOf.Gas_Smoke, 1f);
				Destroy();
			}
			return false;
		}

		public override float GetSpecialApparelScoreOffset()
		{
			return this.GetStatValue(StatDefOf.SmokepopBeltRadius) * ApparelScorePerBeltRadius;
		}
	}
}
