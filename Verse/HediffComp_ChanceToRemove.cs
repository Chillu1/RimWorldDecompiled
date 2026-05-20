namespace Verse;

public class HediffComp_ChanceToRemove : HediffComp
{
	public int currentInterval;

	public bool removeNextInterval;

	public HediffCompProperties_ChanceToRemove Props => (HediffCompProperties_ChanceToRemove)props;

	public override bool CompShouldRemove
	{
		get
		{
			if (!base.CompShouldRemove)
			{
				if (removeNextInterval)
				{
					return currentInterval <= 0;
				}
				return false;
			}
			return true;
		}
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (CompShouldRemove)
		{
			return;
		}
		if (currentInterval <= 0)
		{
			if (Rand.Chance(Props.chance))
			{
				removeNextInterval = true;
				currentInterval = Rand.Range(0, Props.intervalTicks);
			}
			else
			{
				currentInterval = Props.intervalTicks;
			}
		}
		else
		{
			currentInterval -= delta;
		}
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref currentInterval, "currentInterval", 0);
		Scribe_Values.Look(ref removeNextInterval, "removeNextInterval", defaultValue: false);
	}

	public override string CompDebugString()
	{
		return $"currentInterval: {currentInterval}\nremove: {removeNextInterval}";
	}
}
