using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RitualObligationTrigger_Blinding : RitualObligationTrigger_EveryMember
{
	private static List<Pawn> existingObligations = new List<Pawn>();

	public override string TriggerExtraDesc => "RitualBlindingTriggerExtraDesc".Translate(ritual.ideo.memberName.Named("IDEOMEMBER"));

	protected override void Recache()
	{
		try
		{
			if (ritual.activeObligations != null)
			{
				ritual.activeObligations.RemoveAll((RitualObligation o) => o.targetA.Thing is Pawn pawn && pawn.Ideo != ritual.ideo);
				foreach (RitualObligation activeObligation in ritual.activeObligations)
				{
					existingObligations.Add(activeObligation.targetA.Thing as Pawn);
				}
			}
			foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
			{
				if (!existingObligations.Contains(allMapsCaravansAndTravellingTransporters_Alive_Colonist) && allMapsCaravansAndTravellingTransporters_Alive_Colonist.Ideo != null && allMapsCaravansAndTravellingTransporters_Alive_Colonist.Ideo == ritual.ideo && !allMapsCaravansAndTravellingTransporters_Alive_Colonist.IsPrisoner && allMapsCaravansAndTravellingTransporters_Alive_Colonist.health.hediffSet.GetNotMissingParts().Any((BodyPartRecord p) => p.def == BodyPartDefOf.Eye))
				{
					ritual.AddObligation(new RitualObligation(ritual, allMapsCaravansAndTravellingTransporters_Alive_Colonist, expires: false));
				}
			}
		}
		finally
		{
			existingObligations.Clear();
		}
	}
}
