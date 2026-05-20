using Verse;

namespace RimWorld;

public abstract class CompRitualEffectSpawner : ThingComp
{
	protected LordJob_Ritual ritual;

	private const int RitualCheckInterval = 30;

	public override void CompTick()
	{
		if (parent.IsHashIntervalTick(30))
		{
			ritual = parent.TargetOfRitual();
		}
		if (ritual != null)
		{
			Tick_InRitualInterval(ritual);
		}
		else
		{
			Tick_OutOfRitualInterval();
		}
	}

	protected abstract void Tick_InRitual(LordJob_Ritual ritual);

	protected abstract void Tick_InRitualInterval(LordJob_Ritual ritual);

	protected abstract void Tick_OutOfRitualInterval();
}
