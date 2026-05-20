using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ComplexThreatWorker_SleepingMechanoids : ComplexThreatWorker_SleepingThreat
	{
		protected override bool CanResolveInt(ComplexResolveParams parms)
		{
			if (base.CanResolveInt(parms))
			{
				if (parms.hostileFaction != null)
				{
					return parms.hostileFaction == Faction.OfMechanoids;
				}
				return true;
			}
			return false;
		}

		private static bool MechKindSuitableForComplex(PawnKindDef def)
		{
			if (def.RaceProps.IsMechanoid && !def.isGoodBreacher && def.isFighter)
			{
				return def.allowInMechClusters;
			}
			return false;
		}

		protected override IEnumerable<PawnKindDef> GetPawnKindsForPoints(float points)
		{
			return PawnUtility.GetCombatPawnKindsForPoints(MechKindSuitableForComplex, points);
		}
	}
}
