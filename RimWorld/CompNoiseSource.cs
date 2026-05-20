using Verse;

namespace RimWorld;

public class CompNoiseSource : ThingComp
{
	private CompPowerTrader powerComp;

	public CompProperties_NoiseSource Props => (CompProperties_NoiseSource)props;

	public bool Active
	{
		get
		{
			if (PowerComp != null && (!powerComp.PowerOn || !(PowerComp.PowerOutput < 0f)))
			{
				return powerComp.PowerOutput > 0f;
			}
			return true;
		}
	}

	private CompPowerTrader PowerComp => powerComp ?? (powerComp = parent.GetComp<CompPowerTrader>());

	public override void PostDrawExtraSelectionOverlays()
	{
		GenDraw.DrawRadiusRing(parent.Position, Props.radius);
	}
}
