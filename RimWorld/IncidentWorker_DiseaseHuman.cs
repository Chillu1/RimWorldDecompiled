using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_DiseaseHuman : IncidentWorker_Disease
	{
		protected override IEnumerable<Pawn> PotentialVictimCandidates(IIncidentTarget target)
		{
			Map map = target as Map;
			if (map != null)
			{
				return map.mapPawns.FreeColonistsAndPrisoners;
			}
			return ((Caravan)target).PawnsListForReading.Where((Pawn x) => x.IsFreeColonist || x.IsPrisonerOfColony);
		}

		protected override IEnumerable<Pawn> ActualVictims(IncidentParms parms)
		{
			int num = PotentialVictimCandidates(parms.target).Count();
			int randomInRange = new IntRange(Mathf.RoundToInt((float)num * def.diseaseVictimFractionRange.min), Mathf.RoundToInt((float)num * def.diseaseVictimFractionRange.max)).RandomInRange;
			randomInRange = Mathf.Clamp(randomInRange, 1, def.diseaseMaxVictims);
			return PotentialVictims(parms.target).InRandomOrder().Take(randomInRange);
		}
	}
}
