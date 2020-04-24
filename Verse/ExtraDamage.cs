namespace Verse
{
	public class ExtraDamage
	{
		public DamageDef def;

		public float amount;

		public float armorPenetration = -1f;

		public float chance = 1f;

		public float AdjustedDamageAmount(Verb verb, Pawn caster)
		{
			return amount * verb.verbProps.GetDamageFactorFor(verb, caster);
		}

		public float AdjustedArmorPenetration(Verb verb, Pawn caster)
		{
			if (armorPenetration < 0f)
			{
				return AdjustedDamageAmount(verb, caster) * 0.015f;
			}
			return armorPenetration;
		}

		public float AdjustedArmorPenetration()
		{
			if (armorPenetration < 0f)
			{
				return amount * 0.015f;
			}
			return armorPenetration;
		}
	}
}
