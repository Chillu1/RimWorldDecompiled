using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_UseEffectDestroySelf : CompProperties_UseEffect
{
	public int delayTicks = -1;

	public float orderPriority = -1000f;

	public EffecterDef effecterDef;

	public bool spawnKilledLeavings;

	public List<ThingDefCountClass> leavings;

	public CompProperties_UseEffectDestroySelf()
	{
		compClass = typeof(CompUseEffect_DestroySelf);
	}
}
