using RimWorld;
using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalState_TargetedInsultingSpree : MentalState_InsultingSpree
	{
		private static List<Pawn> candidates = new List<Pawn>();

		public override string InspectLine => string.Format(def.baseInspectLine, target.LabelShort);

		protected override bool CanEndBeforeMaxDurationNow => insultedTargetAtLeastOnce;

		public override void MentalStateTick()
		{
			if (base.target != null && (!base.target.Spawned || !pawn.CanReach(base.target, PathEndMode.Touch, Danger.Deadly) || !base.target.Awake()))
			{
				Pawn target = base.target;
				if (!TryFindNewTarget())
				{
					RecoverFromState();
					return;
				}
				Messages.Message("MessageTargetedInsultingSpreeChangedTarget".Translate(pawn.LabelShort, target.Label, base.target.Label, pawn.Named("PAWN"), target.Named("OLDTARGET"), base.target.Named("TARGET")).AdjustedFor(pawn), pawn, MessageTypeDefOf.NegativeEvent);
				base.MentalStateTick();
			}
			else if (base.target == null || !InsultingSpreeMentalStateUtility.CanChaseAndInsult(pawn, base.target, skipReachabilityCheck: false, allowPrisoners: false))
			{
				RecoverFromState();
			}
			else
			{
				base.MentalStateTick();
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

		public override string GetBeginLetterText()
		{
			if (target == null)
			{
				Log.Error("No target. This should have been checked in this mental state's worker.");
				return "";
			}
			return def.beginLetter.Formatted(pawn.NameShortColored.Resolve(), target.NameShortColored.Resolve(), pawn.Named("PAWN"), target.Named("TARGET")).AdjustedFor(pawn).CapitalizeFirst();
		}
	}
}
