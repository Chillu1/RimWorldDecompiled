using RimWorld;

namespace Verse.AI;

public class MentalState_MurderousRage : MentalState
{
	public Pawn target;

	private const int NoLongerValidTargetCheckInterval = 120;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref target, "target");
	}

	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Off;
	}

	public override void PreStart()
	{
		base.PreStart();
		TryFindNewTarget();
	}

	public override void MentalStateTick(int delta)
	{
		base.MentalStateTick(delta);
		if (target != null && target.Dead)
		{
			RecoverFromState();
		}
		if (pawn.IsHashIntervalTick(120, delta) && !IsTargetStillValidAndReachable())
		{
			if (!TryFindNewTarget())
			{
				RecoverFromState();
				return;
			}
			Messages.Message("MessageMurderousRageChangedTarget".Translate(pawn.NameShortColored, target.Label, pawn.Named("PAWN"), target.Named("TARGET")).Resolve().AdjustedFor(pawn), pawn, MessageTypeDefOf.NegativeEvent);
			base.MentalStateTick(delta);
		}
	}

	public override TaggedString GetBeginLetterText()
	{
		if (target == null)
		{
			Log.Error("No target. This should have been checked in this mental state's worker.");
			return "";
		}
		return def.beginLetter.Formatted(pawn.NameShortColored, target.NameShortColored, pawn.Named("PAWN"), target.Named("TARGET")).AdjustedFor(pawn).Resolve()
			.CapitalizeFirst();
	}

	private bool TryFindNewTarget()
	{
		target = MurderousRageMentalStateUtility.FindPawnToKill(pawn);
		return target != null;
	}

	public bool IsTargetStillValidAndReachable()
	{
		if (target != null && target.SpawnedParentOrMe != null && (!(target.SpawnedParentOrMe is Pawn) || target.SpawnedParentOrMe == target))
		{
			return pawn.CanReach(target.SpawnedParentOrMe, PathEndMode.Touch, Danger.Deadly, canBashDoors: true);
		}
		return false;
	}
}
