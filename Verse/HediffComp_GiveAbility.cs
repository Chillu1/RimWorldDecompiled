namespace Verse
{
	public class HediffComp_GiveAbility : HediffComp
	{
		private HediffCompProperties_GiveAbility Props => (HediffCompProperties_GiveAbility)props;

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			if (Props.abilityDef != null)
			{
				parent.pawn.abilities.GainAbility(Props.abilityDef);
			}
			if (!Props.abilityDefs.NullOrEmpty())
			{
				for (int i = 0; i < Props.abilityDefs.Count; i++)
				{
					parent.pawn.abilities.GainAbility(Props.abilityDefs[i]);
				}
			}
		}

		public override void CompPostPostRemoved()
		{
			if (Props.abilityDef != null)
			{
				parent.pawn.abilities.RemoveAbility(Props.abilityDef);
			}
			if (!Props.abilityDefs.NullOrEmpty())
			{
				for (int i = 0; i < Props.abilityDefs.Count; i++)
				{
					parent.pawn.abilities.RemoveAbility(Props.abilityDefs[i]);
				}
			}
		}
	}
}
