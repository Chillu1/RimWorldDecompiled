using Verse;

namespace RimWorld
{
	public class RitualObligationTrigger_MemberDied : RitualObligationTrigger
	{
		public override void Notify_MemberDied(Pawn p)
		{
			if ((!mustBePlayerIdeo || Faction.OfPlayer.ideos.Has(ritual.ideo)) && (p.HomeFaction == Faction.OfPlayer || p.IsSlave) && p.IsFreeColonist && !p.IsKidnapped())
			{
				ritual.AddObligation(new RitualObligation(ritual, p)
				{
					sendLetter = !p.IsSlave
				});
			}
		}
	}
}
