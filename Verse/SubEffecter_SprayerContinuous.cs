namespace Verse;

public class SubEffecter_SprayerContinuous : SubEffecter_Sprayer
{
	private int ticksUntilMote;

	private int moteCount;

	public SubEffecter_SprayerContinuous(SubEffecterDef def, Effecter parent)
		: base(def, parent)
	{
		ticksUntilMote = def.initialDelayTicks;
	}

	public override void SubEffectTick(TargetInfo A, TargetInfo B)
	{
		if (moteCount < def.maxMoteCount)
		{
			ticksUntilMote--;
			if (ticksUntilMote <= 0)
			{
				MakeMote(A, B);
				ticksUntilMote = def.ticksBetweenMotes;
				moteCount++;
			}
		}
	}
}
