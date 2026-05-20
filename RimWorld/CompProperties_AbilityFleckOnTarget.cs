using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityFleckOnTarget : CompProperties_AbilityEffect
	{
		public FleckDef fleckDef;

		public List<FleckDef> fleckDefs;

		public float scale = 1f;

		public int preCastTicks;

		public CompProperties_AbilityFleckOnTarget()
		{
			compClass = typeof(CompAbilityEffect_FleckOnTarget);
		}
	}
}
