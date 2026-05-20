using RimWorld;

namespace Verse.AI;

public class MentalState_Roaming : MentalState
{
	public const int WanderDistance = 12;

	private const int MaxTicksToRoamBeforeExit = 60000;

	private static readonly IntRange WaitAtEdgeBeforeExitingTicks = new IntRange(7000, 8000);

	public IntVec3 exitDest = IntVec3.Invalid;

	public int waitAtEdgeUntilTick = -1;

	public override bool AllowRestingInBed => false;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref exitDest, "exitDest");
		Scribe_Values.Look(ref waitAtEdgeUntilTick, "waitAtEdgeUntilTick", 0);
	}

	public override void PreStart()
	{
		base.PreStart();
		pawn.caller?.DoCall();
		Messages.Message("MessageRoamerLeaving".Translate(pawn.Named("PAWN")).CapitalizeFirst(), pawn, MessageTypeDefOf.ThreatSmall);
	}

	public override void MentalStateTick(int delta)
	{
		base.MentalStateTick(delta);
		if (!pawn.Spawned)
		{
			return;
		}
		if (!exitDest.IsValid || !pawn.CanReach(exitDest, PathEndMode.OnCell, Danger.Deadly))
		{
			waitAtEdgeUntilTick = -1;
			exitDest = IntVec3.Invalid;
			if (!RCellFinder.TryFindRandomExitSpot(pawn, out exitDest))
			{
				RecoverFromState();
				return;
			}
		}
		if (pawn.roping.IsRoped)
		{
			RecoverFromState();
		}
		if (waitAtEdgeUntilTick < 0 && pawn.Position.InHorDistOf(exitDest, 12f))
		{
			waitAtEdgeUntilTick = Find.TickManager.TicksGame + WaitAtEdgeBeforeExitingTicks.RandomInRange;
		}
	}

	public override void PostEnd()
	{
		base.PostEnd();
		pawn.mindState.lastStartRoamCooldownTick = Find.TickManager.TicksGame;
	}

	public bool ShouldExitMapNow()
	{
		if (waitAtEdgeUntilTick > 0 && Find.TickManager.TicksGame > waitAtEdgeUntilTick)
		{
			return true;
		}
		if (base.Age > 60000)
		{
			return true;
		}
		return false;
	}
}
