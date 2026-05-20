using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class MentalState_TargetedTantrum : MentalState_Tantrum
{
	public const int MinMarketValue = 300;

	private static List<Thing> tmpThings = new List<Thing>();

	public override void MentalStateTick(int delta)
	{
		if (target == null || target.Destroyed)
		{
			RecoverFromState();
		}
		else if (!target.Spawned || !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
		{
			Thing thing = target;
			if (!TryFindNewTarget())
			{
				RecoverFromState();
				return;
			}
			Messages.Message("MessageTargetedTantrumChangedTarget".Translate(pawn.LabelShort, thing.Label, target.Label, pawn.Named("PAWN"), thing.Named("OLDTARGET"), target.Named("TARGET")).AdjustedFor(pawn), pawn, MessageTypeDefOf.NegativeEvent);
			base.MentalStateTick(delta);
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
		TantrumMentalStateUtility.GetSmashableThingsNear(pawn, pawn.Position, tmpThings, null, 300);
		bool result = tmpThings.TryRandomElementByWeight((Thing x) => x.MarketValue * (float)x.stackCount, out target);
		tmpThings.Clear();
		return result;
	}

	public override TaggedString GetBeginLetterText()
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
