namespace Verse;

public class SubEffecter_SprayerTriggeredDelayed : SubEffecter_SprayerTriggered
{
	private int ticksLeft = -1;

	public SubEffecter_SprayerTriggeredDelayed(SubEffecterDef def, Effecter parent)
		: base(def, parent)
	{
	}

	public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
	{
		ticksLeft = def.initialDelayTicks;
	}

	public override void SubEffectTick(TargetInfo A, TargetInfo B)
	{
		bool flag = ticksLeft > 0;
		if (flag)
		{
			ticksLeft--;
		}
		if (ticksLeft <= 0 && flag)
		{
			MakeMote(A, B);
		}
		base.SubEffectTick(A, B);
	}
}
