using Verse;

namespace RimWorld;

public class Designator_ZoneAdd_Growing_Expand : Designator_ZoneAdd_Growing
{
	protected override bool ShowRightClickHideOptions => false;

	public Designator_ZoneAdd_Growing_Expand()
	{
		defaultLabel = "DesignatorZoneExpand".Translate();
		hotKey = KeyBindingDefOf.Misc6;
	}
}
