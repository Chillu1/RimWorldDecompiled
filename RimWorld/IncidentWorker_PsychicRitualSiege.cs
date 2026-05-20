using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_PsychicRitualSiege : IncidentWorker_RaidEnemy
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (parms.psychicRitualDef == null && !DefDatabase<PsychicRitualDef>.AllDefs.Where((PsychicRitualDef x) => x.aiCastable && x.minThreatPoints <= parms.points).TryRandomElement(out parms.psychicRitualDef))
		{
			return false;
		}
		parms.pawnGroupKind = PawnGroupKindDefOf.PsychicRitualSiege;
		return base.TryExecuteWorker(parms);
	}

	protected override bool TryResolveRaidFaction(IncidentParms parms)
	{
		if (Faction.OfHoraxCult != null)
		{
			parms.faction = Faction.OfHoraxCult;
			return true;
		}
		return false;
	}

	public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
	{
		parms.raidStrategy = RaidStrategyDefOf.PsychicRitualSiege;
	}

	protected override string GetLetterLabel(IncidentParms parms)
	{
		return parms.raidStrategy.letterLabelEnemy;
	}

	protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
	{
		TaggedString taggedString = parms.raidArrivalMode.textEnemy.Formatted(parms.faction.def.pawnsPlural, parms.faction.NameColored).CapitalizeFirst();
		taggedString += "\n\n";
		if (!parms.psychicRitualDef.letterAIArrivedText.NullOrEmpty())
		{
			taggedString += parms.psychicRitualDef.letterAIArrivedText;
		}
		else
		{
			taggedString += parms.raidStrategy.arrivalTextEnemy.Formatted(parms.psychicRitualDef.label.Named("RITUAL"));
		}
		taggedString += "\n\n" + "AttackersMayCastRitualMultipleTimes".Translate();
		Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
		if (pawn != null)
		{
			taggedString += "\n\n";
			taggedString += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER")).Resolve();
		}
		if (parms.raidAgeRestriction != null && !parms.raidAgeRestriction.arrivalTextExtra.NullOrEmpty())
		{
			taggedString += "\n\n";
			taggedString += parms.raidAgeRestriction.arrivalTextExtra.Formatted(parms.faction.def.pawnsPlural.Named("PAWNSPLURAL")).Resolve();
		}
		return taggedString.Resolve();
	}
}
