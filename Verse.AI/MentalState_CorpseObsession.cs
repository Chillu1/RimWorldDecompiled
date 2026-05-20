namespace Verse.AI;

public class MentalState_CorpseObsession : MentalState
{
	public Corpse corpse;

	public bool alreadyHauledCorpse;

	private const int AnyCorpseStillValidCheckInterval = 500;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref corpse, "corpse");
		Scribe_Values.Look(ref alreadyHauledCorpse, "alreadyHauledCorpse", defaultValue: false);
	}

	public override void MentalStateTick(int delta)
	{
		if (alreadyHauledCorpse)
		{
			base.MentalStateTick(delta);
			return;
		}
		bool flag = false;
		if (pawn.IsHashIntervalTick(500, delta) && !CorpseObsessionMentalStateUtility.IsCorpseValid(corpse, pawn))
		{
			corpse = CorpseObsessionMentalStateUtility.GetClosestCorpseToDigUp(pawn);
			if (corpse == null)
			{
				RecoverFromState();
				flag = true;
			}
		}
		if (!flag)
		{
			base.MentalStateTick(delta);
		}
	}

	public override void PostStart(string reason)
	{
		base.PostStart(reason);
		corpse = CorpseObsessionMentalStateUtility.GetClosestCorpseToDigUp(pawn);
	}

	public void Notify_CorpseHauled()
	{
		alreadyHauledCorpse = true;
	}
}
