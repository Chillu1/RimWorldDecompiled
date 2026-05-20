using RimWorld;

namespace Verse.AI;

public class MentalState_InhumanRambling : MentalState_WanderSad
{
	private int ticksToRamble;

	private const float RambleEffectRadius = 9.9f;

	public override void PostStart(string reason)
	{
		ticksToRamble = def.ticksBetweenRamblingRange.RandomInRange;
		base.PostStart(reason);
	}

	public override void MentalStateTick(int delta)
	{
		if (pawn.Awake() && !pawn.Suspended && pawn.Spawned)
		{
			ticksToRamble--;
			if (ticksToRamble <= 0)
			{
				ticksToRamble = def.ticksBetweenRamblingRange.RandomInRange;
				DoInhumanRambling(pawn);
			}
		}
		base.MentalStateTick(delta);
	}

	public static void DoInhumanRambling(Pawn pawn)
	{
		SocialInteractionUtility.ImitateInteractionWithNoPawn(pawn, InteractionDefOf.InhumanRambling);
		GenClamor.DoClamor(pawn, 9.9f, delegate(Thing source, Pawn hearer)
		{
			if (hearer != source && hearer.needs.mood != null)
			{
				hearer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.HeardInhumanRambling, pawn);
			}
		});
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticksToRamble, "ticksToRamble", 0);
	}
}
