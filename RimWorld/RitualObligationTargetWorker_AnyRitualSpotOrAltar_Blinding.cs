using System.Linq;
using Verse;

namespace RimWorld;

public class RitualObligationTargetWorker_AnyRitualSpotOrAltar_Blinding : RitualObligationTargetWorker_AnyRitualSpotOrAltar
{
	public RitualObligationTargetWorker_AnyRitualSpotOrAltar_Blinding()
	{
	}

	public RitualObligationTargetWorker_AnyRitualSpotOrAltar_Blinding(RitualObligationTargetFilterDef def)
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
			return pawn.health.hediffSet.GetNotMissingParts().Any((BodyPartRecord p) => p.def == BodyPartDefOf.Eye);
		}
		return false;
	}
}
