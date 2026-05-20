using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class MentalState_TargetedInsultingSpree : MentalState_InsultingSpree
{
	private static List<Pawn> candidates = new List<Pawn>();

	public override string InspectLine => string.Format(def.baseInspectLine, target.LabelShort);

	protected override bool CanEndBeforeMaxDurationNow => insultedTargetAtLeastOnce;

	public override void MentalStateTick(int delta)
	{
		if (target != null && (!target.Spawned || !base.pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly) || !target.Awake()))
		{
			Pawn pawn = target;
			if (!TryFindNewTarget())
			{
				RecoverFromState();
				return;
			}
			Messages.Message("MessageTargetedInsultingSpreeChangedTarget".Translate(base.pawn.LabelShort, pawn.Label, target.Label, base.pawn.Named("PAWN"), pawn.Named("OLDTARGET"), target.Named("TARGET")).AdjustedFor(base.pawn), base.pawn, MessageTypeDefOf.NegativeEvent);
			base.MentalStateTick(delta);
		}
		else if (target == null || !InsultingSpreeMentalStateUtility.CanChaseAndInsult(base.pawn, target, skipReachabilityCheck: false, allowPrisoners: false))
		{
			RecoverFromState();
		}
		else
		{
			base.MentalStateTick(delta);
		}
	}

	public override void PreStart()
	{
		base.PreStart();
		TryFindNewTarget();
	}

	private bool TryFindNewTarget()
	{
		InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(pawn, candidates, allowPrisoners: false);
		bool result = candidates.TryRandomElement(out target);
		candidates.Clear();
		return result;
	}

	public override void PostEnd()
	{
		base.PostEnd();
		if (target != null && PawnUtility.ShouldSendNotificationAbout(pawn))
		{
			Messages.Message("MessageNoLongerOnTargetedInsultingSpree".Translate(pawn.LabelShort, target.Label, pawn.Named("PAWN"), target.Named("TARGET")), pawn, MessageTypeDefOf.SituationResolved);
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
}
