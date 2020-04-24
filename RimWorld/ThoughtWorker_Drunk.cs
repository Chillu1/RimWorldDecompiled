using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Drunk : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
		{
			if (!p.RaceProps.Humanlike)
			{
				return false;
			}
			if (!p.IsTeetotaler())
			{
				return false;
			}
			if (!other.RaceProps.Humanlike)
			{
				return false;
			}
			if (!RelationsUtility.PawnsKnowEachOther(p, other))
			{
				return false;
			}
			Hediff firstHediffOfDef = other.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh);
			if (firstHediffOfDef == null || !firstHediffOfDef.Visible)
			{
				return false;
			}
			return true;
		}
	}
}
