using Verse;

namespace RimWorld;

public class CompLeaveFilthOnDestroyed : ThingComp
{
	public CompProperties_LeaveFilthOnDestroyed Props => (CompProperties_LeaveFilthOnDestroyed)props;

	private float Radius => (float)parent.def.size.x / 2f;

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		if (mode == DestroyMode.Vanish)
		{
			return;
		}
		foreach (IntVec3 item in GenRadial.RadialPatternInRadius(Radius - 0.1f))
		{
			FilthMaker.TryMakeFilth(parent.Position + item, previousMap, Props.filthDef, Props.thickness);
		}
	}
}
