using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_DamageOnInterval : CompProperties
	{
		public DamageDef damageDef;

		public int ticksBetweenDamage;

		public float damage;

		public List<DamageEffectStage> effectStages = new List<DamageEffectStage>();

		public float startHitPointsPercent = -1f;

		public CompProperties_DamageOnInterval()
		{
			compClass = typeof(CompDamageOnInterval);
		}
	}
}
