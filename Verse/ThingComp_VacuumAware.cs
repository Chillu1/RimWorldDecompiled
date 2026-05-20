namespace Verse;

public abstract class ThingComp_VacuumAware : ThingComp
{
	protected bool InVacuum => parent.PositionHeld.GetVacuum(parent.MapHeld) > 0f;

	protected abstract bool FunctionsInVacuum { get; }

	public override string CompInspectStringExtra()
	{
		if (InVacuum && !FunctionsInVacuum)
		{
			return "CannotFunctionInVacuum".Translate().CapitalizeFirst().Colorize(ColoredText.WarningColor);
		}
		return null;
	}
}
