using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Transmute : CompProperties_AbilityEffect
{
	public class ElementRatio
	{
		public ThingDef sourceStuff;

		public float ratio = 1f;
	}

	public List<ElementRatio> elementRatios = new List<ElementRatio>();

	public List<ThingDef> outcomeItems = new List<ThingDef>();

	public List<ThingDef> outcomeStuff = new List<ThingDef>();

	[MustTranslate]
	public string failedMessage;

	public CompProperties_Transmute()
	{
		compClass = typeof(CompAbilityEffect_Transmute);
	}
}
