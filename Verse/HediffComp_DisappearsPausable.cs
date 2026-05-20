namespace Verse;

public class HediffComp_DisappearsPausable : HediffComp_Disappears
{
	protected virtual bool Paused => false;

	public override string CompLabelInBracketsExtra
	{
		get
		{
			if (!Paused)
			{
				return base.CompLabelInBracketsExtra;
			}
			return null;
		}
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (!Paused)
		{
			ticksToDisappear -= TicksLostPerTick;
		}
	}
}
