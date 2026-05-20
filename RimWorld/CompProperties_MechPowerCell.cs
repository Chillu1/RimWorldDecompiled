using Verse;

namespace RimWorld;

public class CompProperties_MechPowerCell : CompProperties
{
	public int totalPowerTicks = 2500;

	public bool killWhenDepleted = true;

	public bool showGizmoOnNonPlayerControlled;

	[MustTranslate]
	public string labelOverride;

	[MustTranslate]
	public string tooltipOverride;

	public CompProperties_MechPowerCell()
	{
		compClass = typeof(CompMechPowerCell);
	}
}
