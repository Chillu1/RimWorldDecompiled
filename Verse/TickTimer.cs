using System;

namespace Verse;

public class TickTimer : IExposable
{
	private int start;

	private int elapsed;

	private int duration;

	public Action OnFinish;

	public bool Finished => elapsed >= duration;

	public void ExposeData()
	{
		Scribe_Values.Look(ref elapsed, "elapsed", 0);
		Scribe_Values.Look(ref duration, "duration", 0);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && !Finished)
		{
			start = GenTicks.TicksGame;
		}
	}

	public void Start(int startTicks, int durationTicks, Action finishAction)
	{
		start = startTicks;
		duration = durationTicks;
		OnFinish = finishAction;
		elapsed = 0;
	}

	public void TickIntervalDelta()
	{
		elapsed = GenTicks.TicksGame - start;
		if (Finished && OnFinish != null)
		{
			OnFinish();
			OnFinish = null;
		}
	}
}
