using RimWorld;
using RimWorld.Planet;

namespace Verse.AI;

public class MentalFitGenerator : IExposable
{
	private Pawn pawn;

	private int ticksUntilCanDoMentalFit;

	private const int CheckInterval = 150;

	private const int MinTicksSinceRecoveryToHaveFit = 600;

	public bool CanDoRandomMentalFits
	{
		get
		{
			if (pawn.RaceProps.Humanlike)
			{
				return !pawn.IsWorldPawn();
			}
			return false;
		}
	}

	public MentalFitGenerator()
	{
	}

	public MentalFitGenerator(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void Reset()
	{
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref ticksUntilCanDoMentalFit, "ticksUntilCanDoMentalFit", 0);
	}

	public void TickInterval(int delta)
	{
		if (ticksUntilCanDoMentalFit > 0)
		{
			if (pawn.Awake())
			{
				ticksUntilCanDoMentalFit -= delta;
			}
		}
		else
		{
			if (!pawn.Awake() || !pawn.IsHashIntervalTick(150, delta) || !CanDoRandomMentalFits || pawn.InMentalState)
			{
				return;
			}
			foreach (MentalFitDef item in DefDatabase<MentalFitDef>.AllDefsListForReading)
			{
				if (Rand.MTBEventOccurs(item.CalculateMTBDays(pawn), 60000f, 150f) && pawn.mindState.mentalStateHandler.TryStartMentalState(item.mentalState, null, forced: false, forceWake: false, causedByMood: false, null, transitionSilently: true))
				{
					break;
				}
			}
		}
	}

	public void Notify_RecoveredFromMentalState()
	{
		ticksUntilCanDoMentalFit = 600;
	}
}
