using RimWorld;

namespace Verse.AI;

public abstract class MentalState_SlaughterThing : MentalState
{
	protected int lastSlaughterTicks = -1;

	[LoadAlias("animalsSlaughtered")]
	protected int thingsSlaughtered;

	protected const int NoThingToSlaughterCheckInterval = 600;

	protected virtual int MinTicksBetweenSlaughter => 3750;

	protected virtual int MaxThingsSlaughtered => 4;

	public bool SlaughteredRecently
	{
		get
		{
			if (lastSlaughterTicks >= 0)
			{
				return Find.TickManager.TicksGame - lastSlaughterTicks < MinTicksBetweenSlaughter;
			}
			return false;
		}
	}

	protected override bool CanEndBeforeMaxDurationNow => lastSlaughterTicks >= 0;

	protected abstract bool SlaughterTargetAvailable { get; }

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastSlaughterTicks, "lastSlaughterTicks", 0);
		Scribe_Values.Look(ref thingsSlaughtered, "thingsSlaughtered", 0);
	}

	public override void MentalStateTick(int delta)
	{
		base.MentalStateTick(delta);
		if (pawn.IsHashIntervalTick(600, delta) && (pawn.CurJob == null || pawn.CurJob.def != JobDefOf.Slaughter) && !SlaughterTargetAvailable)
		{
			RecoverFromState();
		}
	}

	public override void Notify_SlaughteredTarget()
	{
		lastSlaughterTicks = Find.TickManager.TicksGame;
		thingsSlaughtered++;
		if (thingsSlaughtered >= MaxThingsSlaughtered)
		{
			RecoverFromState();
		}
	}
}
