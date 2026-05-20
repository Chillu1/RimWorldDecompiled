using Verse;

namespace RimWorld;

public class CompProperties_SpreadSludge : CompProperties
{
	public int mtbTicks = 2500;

	public AbilityDef abilityDef;

	public bool tryAvoidSludge;

	public CompProperties_SpreadSludge()
	{
		compClass = typeof(CompSpreadSludge);
	}
}
