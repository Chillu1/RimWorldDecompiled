using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_SelfTame : IncidentWorker
{
	private IEnumerable<Pawn> Candidates(Map map)
	{
		return map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.IsAnimal && x.Faction == null && !x.Position.Fogged(x.Map) && !x.InMentalState && !x.Downed && !x.health.hediffSet.HasHediff(HediffDefOf.Scaria) && x.GetStatValue(StatDefOf.Wildness) > 0f);
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		return Candidates(map).Any();
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		Pawn result = null;
		if (!Candidates(map).TryRandomElement(out result))
		{
			return false;
		}
		string text = result.LabelIndefinite();
		bool num = result.Name != null;
		result.SetFaction(Faction.OfPlayer);
		SendStandardLetter(baseLetterText: (num || result.Name == null) ? ((string)"LetterAnimalSelfTame".Translate(result).CapitalizeFirst()) : ((!result.Name.Numerical) ? ((string)"LetterAnimalSelfTameAndName".Translate(text, result.Name.ToStringFull, result.Named("ANIMAL")).CapitalizeFirst()) : ((string)"LetterAnimalSelfTameAndNameNumerical".Translate(text, result.Name.ToStringFull, result.Named("ANIMAL")).CapitalizeFirst())), baseLetterLabel: "LetterLabelAnimalSelfTame".Translate(result.KindLabel, result).CapitalizeFirst(), baseLetterDef: LetterDefOf.PositiveEvent, parms: parms, lookTargets: result, textArgs: Array.Empty<NamedArgument>());
		return true;
	}
}
