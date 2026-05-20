using Verse;

namespace RimWorld;

public class CompInitiatable : ThingComp
{
	public int initiationDelayTicksOverride;

	public bool Initiated
	{
		get
		{
			if (Delay > 0)
			{
				if (parent.spawnedTick >= 0)
				{
					return Find.TickManager.TicksGame >= parent.spawnedTick + Delay;
				}
				return false;
			}
			return true;
		}
	}

	private int Delay
	{
		get
		{
			if (initiationDelayTicksOverride <= 0)
			{
				return Props.initiationDelayTicks;
			}
			return initiationDelayTicksOverride;
		}
	}

	private CompProperties_Initiatable Props => (CompProperties_Initiatable)props;

	public override string CompInspectStringExtra()
	{
		if (!Initiated)
		{
			return "InitiatesIn".Translate() + ": " + (parent.spawnedTick + Delay - Find.TickManager.TicksGame).ToStringTicksToPeriod();
		}
		return base.CompInspectStringExtra();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref initiationDelayTicksOverride, "initiationDelayTicksOverride", 0);
	}
}
