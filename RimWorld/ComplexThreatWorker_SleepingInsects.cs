using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ComplexThreatWorker_SleepingInsects : ComplexThreatWorker_SleepingThreat
	{
		protected override bool CanResolveInt(ComplexResolveParams parms)
		{
			if (base.CanResolveInt(parms))
			{
				if (parms.hostileFaction != null)
				{
					return parms.hostileFaction == Faction.OfInsects;
				}
				return true;
			}
			return false;
		}

		protected override IEnumerable<PawnKindDef> GetPawnKindsForPoints(float points)
		{
			return PawnUtility.GetCombatPawnKindsForPoints((PawnKindDef k) => k.RaceProps.Insect, points);
		}
	}
}
