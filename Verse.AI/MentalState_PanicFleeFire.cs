using RimWorld;

namespace Verse.AI;

public class MentalState_PanicFleeFire : MentalState
{
	private int lastFireSeenTick = -1;

	protected override bool CanEndBeforeMaxDurationNow => false;

	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Off;
	}

	public override void MentalStateTick(int delta)
	{
		base.MentalStateTick(delta);
		if (pawn.IsHashIntervalTick(30, delta))
		{
			if (lastFireSeenTick < 0 || ThoughtWorker_Pyrophobia.NearFire(pawn))
			{
				lastFireSeenTick = Find.TickManager.TicksGame;
			}
			if (lastFireSeenTick >= 0 && Find.TickManager.TicksGame >= lastFireSeenTick + def.minTicksBeforeRecovery)
			{
				RecoverFromState();
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastFireSeenTick, "lastFireSeenTick", -1);
	}
}
