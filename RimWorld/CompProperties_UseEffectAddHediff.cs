using Verse;

namespace RimWorld;

public class CompProperties_UseEffectAddHediff : CompProperties_UseEffect
{
	public HediffDef hediffDef;

	public bool allowRepeatedUse;

	public CompProperties_UseEffectAddHediff()
	{
		compClass = typeof(CompUseEffect_AddHediff);
	}
}
