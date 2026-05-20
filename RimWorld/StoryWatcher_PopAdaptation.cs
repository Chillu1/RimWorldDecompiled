using Verse;

namespace RimWorld;

public class StoryWatcher_PopAdaptation : IExposable
{
	private float adaptDays;

	private const int UpdateInterval = 30000;

	public float AdaptDays => adaptDays;

	public void Notify_PawnEvent(Pawn p, PopAdaptationEvent ev)
	{
		if (p.RaceProps.Humanlike)
		{
			if (DebugViewSettings.writeStoryteller)
			{
				Log.Message("PopAdaptation event: " + ev.ToString() + " - " + p);
			}
			if (ev == PopAdaptationEvent.GainedColonist)
			{
				adaptDays = 0f;
			}
		}
	}

	public void PopAdaptationWatcherTick()
	{
		if (Find.TickManager.TicksGame % 30000 == 171)
		{
			float num = 0.5f;
			adaptDays += num;
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref adaptDays, "adaptDays", 0f);
	}

	public void Debug_OffsetAdaptDays(float days)
	{
		adaptDays += days;
	}

	public void ResetAdaptDays()
	{
		adaptDays = 0f;
	}
}
