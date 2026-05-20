using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IncidentWorker_DiseaseAnimal : IncidentWorker_Disease
{
	protected override IEnumerable<Pawn> PotentialVictimCandidates(IIncidentTarget target)
	{
		if (target is Map map)
		{
			return from p in map.mapPawns.PawnsInFaction(Faction.OfPlayer)
				where p.HostFaction == null && !p.RaceProps.Humanlike
				select p;
		}
		return ((Caravan)target).PawnsListForReading.Where((Pawn p) => !p.RaceProps.Humanlike);
	}

	protected override IEnumerable<Pawn> ActualVictims(IncidentParms parms)
	{
		Pawn[] potentialVictims = PotentialVictims(parms.target).ToArray();
		IEnumerable<ThingDef> source = potentialVictims.Select((Pawn v) => v.def).Distinct();
		ThingDef targetRace = source.RandomElementByWeightWithFallback((ThingDef race) => (from v in potentialVictims
			where v.def == race
			select v.BodySize).Sum());
		IEnumerable<Pawn> source2 = potentialVictims.Where((Pawn v) => v.def == targetRace);
		int num = source2.Count();
		int randomInRange = new IntRange(Mathf.RoundToInt((float)num * def.diseaseVictimFractionRange.min), Mathf.RoundToInt((float)num * def.diseaseVictimFractionRange.max)).RandomInRange;
		return Enumerable.Take(count: Mathf.Clamp(randomInRange, 1, def.diseaseMaxVictims), source: source2.InRandomOrder());
	}
}
