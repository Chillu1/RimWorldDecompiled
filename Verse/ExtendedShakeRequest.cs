namespace Verse;

internal class ExtendedShakeRequest : IExposable
{
	private float mag;

	private int duration;

	private int startTick;

	public float Mag => mag;

	public int Duration => duration;

	public int StartTick => startTick;

	public ExtendedShakeRequest()
	{
	}

	public ExtendedShakeRequest(float mag, int duration)
	{
		this.mag = mag;
		this.duration = duration;
		startTick = Find.TickManager.TicksGame;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref mag, "mag", 0f);
		Scribe_Values.Look(ref duration, "duration", 0);
		Scribe_Values.Look(ref startTick, "startTick", 0);
	}
}
