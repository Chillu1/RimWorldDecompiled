using Verse;

namespace RimWorld;

public class CompProperties_InspectStringStatValue : CompProperties_InspectString
{
	public StatDef stat;

	public ToStringNumberSense? numberSense;

	public CompProperties_InspectStringStatValue()
	{
		compClass = typeof(CompInspectStringStatValue);
	}
}
