using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class HediffCompProperties_GiveAbility : HediffCompProperties
	{
		public AbilityDef abilityDef;

		public List<AbilityDef> abilityDefs;

		public HediffCompProperties_GiveAbility()
		{
			compClass = typeof(HediffComp_GiveAbility);
		}
	}
}
