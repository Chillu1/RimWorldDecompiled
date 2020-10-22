using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class MentalState_TargetedTantrum : MentalState_Tantrum
	{
		public const int MinMarketValue = 300;

		private static List<Thing> tmpThings = new List<Thing>();

		public override void MentalStateTick()
		{
			if (base.target == null || base.target.Destroyed)
			{
				RecoverFromState();
			}
			else if (!base.target.Spawned || !pawn.CanReach(base.target, PathEndMode.Touch, Danger.Deadly))
			{
				Thing target = base.target;
				if (!TryFindNewTarget())
				{
					RecoverFromState();
					return;
				}
				Messages.Message("MessageTargetedTantrumChangedTarget".Translate(pawn.LabelShort, target.Label, base.target.Label, pawn.Named("PAWN"), target.Named("OLDTARGET"), base.target.Named("TARGET")).AdjustedFor(pawn), pawn, MessageTypeDefOf.NegativeEvent);
				base.MentalStateTick();
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
			TantrumMentalStateUtility.GetSmashableThingsNear(pawn, pawn.Position, tmpThings, null, 300);
			bool result = tmpThings.TryRandomElementByWeight((Thing x) => x.MarketValue * (float)x.stackCount, out target);
			tmpThings.Clear();
			return result;
		}

		public override string GetBeginLetterText()
		{
			if (target == null)
			{
				Log.Error("No target. This should have been checked in this mental state's worker.");
				return "";
			}
			return def.beginLetter.Formatted(pawn.NameShortColored, target.Label, pawn.Named("PAWN"), target.Named("TARGET")).AdjustedFor(pawn).Resolve()
				.CapitalizeFirst();
		}
	}
}
