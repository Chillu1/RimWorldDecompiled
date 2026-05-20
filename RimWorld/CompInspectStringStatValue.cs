using Verse;

namespace RimWorld;

public class CompInspectStringStatValue : CompInspectString
{
	private new CompProperties_InspectStringStatValue Props => (CompProperties_InspectStringStatValue)props;

	public override string CompInspectStringExtra()
	{
		if (Props.stat == null || Props.stat.Worker.IsDisabledFor(parent))
		{
			return null;
		}
		return Props.inspectString.Formatted(Props.stat.LabelCap, Props.stat.Worker.ValueToString(parent.GetStatValue(Props.stat), finalized: true, Props.numberSense ?? Props.stat.toStringNumberSense));
	}
}
