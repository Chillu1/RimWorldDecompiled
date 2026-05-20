using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_AbilityMoteOnTarget : CompProperties_AbilityEffect
{
	public ThingDef moteDef;

	public List<ThingDef> moteDefs;

	public float scale = 1f;

	public int preCastTicks;

	public CompProperties_AbilityMoteOnTarget()
	{
		compClass = typeof(CompAbilityEffect_MoteOnTarget);
	}
}
