using Verse;

namespace RimWorld;

public class Pawn_GuiltTracker : IExposable
{
	private Pawn pawn;

	public bool awaitingExecution;

	private int guiltyTicksLeft;

	private const int DefaultGuiltyDuration = 60000;

	public bool IsGuilty
	{
		get
		{
			if (guiltyTicksLeft <= 0)
			{
				if (pawn.InAggroMentalState)
				{
					return pawn.MentalStateDef.allowGuilty;
				}
				return false;
			}
			return true;
		}
	}

	public int TicksUntilInnocent => guiltyTicksLeft;

	public string Tip => "GuiltyDesc".Translate() + ": " + TicksUntilInnocent.ToStringTicksToPeriod();

	public Pawn_GuiltTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref guiltyTicksLeft, "guiltyTicksLeft", 0);
		Scribe_Values.Look(ref awaitingExecution, "awaitingExecution", defaultValue: false);
	}

	public void Notify_Guilty(int durationTicks = 60000)
	{
		guiltyTicksLeft = durationTicks;
	}

	public void GuiltTrackerTickInterval(int delta)
	{
		if (guiltyTicksLeft > 0)
		{
			guiltyTicksLeft -= delta;
		}
		else if (!IsGuilty)
		{
			awaitingExecution = false;
		}
	}
}
