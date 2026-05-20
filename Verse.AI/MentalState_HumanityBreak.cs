using RimWorld;

namespace Verse.AI;

public class MentalState_HumanityBreak : MentalState_WanderOwnRoom
{
	private int ticksToRamble;

	private static readonly IntRange RecoveryDurationTicksRange = new IntRange(10000, 20000);

	private static readonly IntRange RambleTicksRange = new IntRange(1200, 6000);

	public override void PostStart(string reason)
	{
		forceRecoverAfterTicks = RecoveryDurationTicksRange.RandomInRange;
		ticksToRamble = RambleTicksRange.RandomInRange;
		base.PostStart(reason);
	}

	public override void MentalStateTick(int delta)
	{
		if (!pawn.Awake() || pawn.Suspended)
		{
			return;
		}
		if (pawn.IsHashIntervalTick(30, delta))
		{
			age += 30;
			if (age >= forceRecoverAfterTicks)
			{
				RecoverFromState();
				return;
			}
		}
		if (pawn.Spawned)
		{
			ticksToRamble--;
			if (ticksToRamble <= 0)
			{
				ticksToRamble = RambleTicksRange.RandomInRange;
				SocialInteractionUtility.ImitateInteractionWithNoPawn(pawn, InteractionDefOf.InhumanRambling);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksToRamble, "ticksToRamble", 0);
	}
}
