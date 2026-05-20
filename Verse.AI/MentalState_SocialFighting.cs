using RimWorld;

namespace Verse.AI;

public class MentalState_SocialFighting : MentalState
{
	public Pawn otherPawn;

	private bool ShouldStop
	{
		get
		{
			if (!otherPawn.Spawned || otherPawn.Dead || otherPawn.Downed)
			{
				return true;
			}
			if (!IsOtherPawnSocialFightingWithMe)
			{
				return true;
			}
			return false;
		}
	}

	private bool IsOtherPawnSocialFightingWithMe
	{
		get
		{
			if (!otherPawn.InMentalState)
			{
				return false;
			}
			if (!(otherPawn.MentalState is MentalState_SocialFighting mentalState_SocialFighting))
			{
				return false;
			}
			if (mentalState_SocialFighting.otherPawn != pawn)
			{
				return false;
			}
			return true;
		}
	}

	public override void MentalStateTick(int delta)
	{
		if (ShouldStop)
		{
			RecoverFromState();
		}
		else
		{
			base.MentalStateTick(delta);
		}
	}

	public override void PostEnd()
	{
		base.PostEnd();
		if (pawn.jobs != null)
		{
			pawn.jobs.StopAll();
		}
		pawn.mindState.meleeThreat = null;
		if (IsOtherPawnSocialFightingWithMe)
		{
			otherPawn.MentalState.RecoverFromState();
		}
		if ((PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(otherPawn)) && pawn.thingIDNumber < otherPawn.thingIDNumber)
		{
			Messages.Message("MessageNoLongerSocialFighting".Translate(pawn.LabelShort, otherPawn.LabelShort, pawn.Named("PAWN1"), otherPawn.Named("PAWN2")), pawn, MessageTypeDefOf.SituationResolved);
		}
		if (!pawn.Dead && pawn.needs.mood != null && !otherPawn.Dead)
		{
			ThoughtDef thoughtDef = ((!(Rand.Value < 0.5f)) ? ThoughtDefOf.HadCatharticFight : ThoughtDefOf.HadAngeringFight);
			pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, otherPawn);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref otherPawn, "otherPawn");
	}

	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Off;
	}
}
