using Verse;

namespace RimWorld
{
	public class Designator_ZoneAddStockpile_Expand : Designator_ZoneAddStockpile_Resources
	{
		public Designator_ZoneAddStockpile_Expand()
		{
			defaultLabel = "DesignatorZoneExpand".Translate();
			hotKey = KeyBindingDefOf.Misc6;
		}
	}
}
