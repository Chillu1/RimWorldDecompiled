using Verse;

namespace RimWorld;

public class Designator_ZoneAdd_Fishing_Expand : Designator_ZoneAdd_Fishing
{
	protected override bool ShowRightClickHideOptions => false;

	public Designator_ZoneAdd_Fishing_Expand()
	{
		defaultLabel = "DesignatorZoneExpand".Translate();
		hotKey = KeyBindingDefOf.Misc6;
	}
}
