using Verse;

namespace RimWorld;

public class Designator_Plan_Expand : Designator_Plan_Add
{
	protected override bool CanSelectColor => false;

	public override bool CanRightClickToggleVisibility => false;

	public Designator_Plan_Expand()
	{
		defaultLabel = "DesignatorExpandPlan".Translate();
		hotKey = KeyBindingDefOf.Misc6;
	}

	public void Initialize(ColorDef color)
	{
		colorDef = color;
	}
}
