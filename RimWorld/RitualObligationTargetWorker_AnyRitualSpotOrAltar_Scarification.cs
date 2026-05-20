using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_AnyRitualSpotOrAltar_Scarification : RitualObligationTargetWorker_AnyRitualSpotOrAltar
{
	public RitualObligationTargetWorker_AnyRitualSpotOrAltar_Scarification()
	{
	}

	public RitualObligationTargetWorker_AnyRitualSpotOrAltar_Scarification(RitualObligationTargetFilterDef def)
		: base(def)
	{
	}

	public override bool ObligationTargetsValid(RitualObligation obligation)
	{
		if (obligation.targetA.Thing is Pawn pawn)
		{
			if (pawn.Dead)
			{
				return false;
			}
			int hediffCount = pawn.health.hediffSet.GetHediffCount(HediffDefOf.Scarification);
			if (pawn.Ideo != null)
			{
				return pawn.Ideo.RequiredScars > hediffCount;
			}
			return false;
		}
		return false;
	}

	public override string LabelExtraPart(RitualObligation obligation)
	{
		return obligation.targetA.Thing.LabelShort;
	}
}
