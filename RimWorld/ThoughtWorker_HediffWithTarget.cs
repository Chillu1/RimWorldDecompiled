using Verse;

namespace RimWorld;

public class ThoughtWorker_HediffWithTarget : ThoughtWorker_Hediff
{
	protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
	{
		HediffWithTarget hediffWithTarget = (HediffWithTarget)p.health.hediffSet.GetFirstHediffOfDef(def.hediff);
		if (hediffWithTarget == null || hediffWithTarget.target != other)
		{
			return ThoughtState.Inactive;
		}
		return CurrentStateInternal(p);
	}
}
