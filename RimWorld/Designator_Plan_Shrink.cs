using Verse;

namespace RimWorld;

public class Designator_Plan_Shrink : Designator_Plan_Remove
{
	public Designator_Plan_Shrink()
	{
		defaultLabel = "DesignatorPlanShrinkSingular".Translate();
		defaultDesc = "DesignatorPlanShrinkDesc".Translate();
		hotKey = KeyBindingDefOf.Misc6;
	}
}
