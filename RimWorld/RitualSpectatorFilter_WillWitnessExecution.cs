using Verse;

namespace RimWorld;

public class RitualSpectatorFilter_WillWitnessExecution : RitualSpectatorFilter
{
	public override bool Allowed(Pawn p)
	{
		if (p.IsSlave || p.Ideo == null)
		{
			return true;
		}
		return p.Ideo.MemberWillingToDo(new HistoryEvent(HistoryEventDefOf.ExecutedPrisoner, p.Named(HistoryEventArgsNames.Doer)));
	}
}
