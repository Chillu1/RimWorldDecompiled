namespace Verse.AI.Group;

public class PsychicRitualToil_Wait : PsychicRitualToil
{
	private int durationTicks = 60;

	public PsychicRitualToil_Wait()
	{
	}

	public PsychicRitualToil_Wait(int durationTicks)
	{
		this.durationTicks = durationTicks;
	}

	public override bool Tick(PsychicRitual psychicRitual, PsychicRitualGraph parent)
	{
		durationTicks--;
		return durationTicks <= 0;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref durationTicks, "durationTicks", 0);
	}
}
