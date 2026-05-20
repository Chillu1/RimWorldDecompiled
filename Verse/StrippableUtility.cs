using RimWorld;

namespace Verse;

public static class StrippableUtility
{
	public static bool CanBeStrippedByColony(Thing th)
	{
		if (!(th is IStrippable strippable))
		{
			return false;
		}
		if (!strippable.AnythingToStrip())
		{
			return false;
		}
		if (!(th is Pawn pawn))
		{
			return true;
		}
		if (!pawn.kindDef.canStrip)
		{
			return false;
		}
		if (pawn.CarriedBy != null)
		{
			return false;
		}
		if (pawn.Downed)
		{
			return true;
		}
		if (pawn.IsPrisonerOfColony && pawn.guest.PrisonerIsSecure)
		{
			return true;
		}
		pawn.IsQuestLodger();
		return false;
	}

	public static void CheckSendStrippingImpactsGoodwillMessage(Thing th)
	{
		if (th is Pawn { Dead: false, Faction: not null } pawn && pawn.Faction != Faction.OfPlayer && !pawn.Faction.HostileTo(Faction.OfPlayer) && !pawn.Faction.Hidden)
		{
			Messages.Message("MessageStrippingWillAngerFaction".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.CautionInput, historical: false);
		}
	}
}
