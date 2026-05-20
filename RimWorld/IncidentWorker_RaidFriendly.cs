using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class IncidentWorker_RaidFriendly : IncidentWorker_Raid
{
	public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
	{
		List<Faction> list = (from p in ((Map)parms.target).attackTargetsCache.TargetsHostileToColony
			where GenHostility.IsActiveThreatToPlayer(p)
			select ((Thing)p).Faction).Distinct().ToList();
		Faction faction = parms.faction;
		parms.faction = f;
		if (!RaidStrategyDefOf.ImmediateAttackFriendly.Worker.CanUseWith(parms, null))
		{
			parms.faction = faction;
			return false;
		}
		parms.faction = faction;
		if (base.FactionCanBeGroupSource(f, parms, desperate) && !f.Hidden && f.PlayerRelationKind == FactionRelationKind.Ally)
		{
			if (list.Any())
			{
				return list.Any((Faction hf) => hf.HostileTo(f));
			}
			return true;
		}
		return false;
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		return ((Map)parms.target).attackTargetsCache.TargetsHostileToColony.Where((IAttackTarget p) => GenHostility.IsActiveThreatToPlayer(p)).Sum((IAttackTarget p) => (p is Pawn pawn) ? pawn.kindDef.combatPower : 0f) > 120f;
	}

	protected override bool TryResolveRaidFaction(IncidentParms parms)
	{
		if (parms.faction != null)
		{
			return true;
		}
		if (!CandidateFactions(parms).Any())
		{
			return false;
		}
		parms.faction = CandidateFactions(parms).RandomElementByWeight((Faction fac) => (float)fac.PlayerGoodwill + 120.00001f);
		return true;
	}

	public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
	{
		if (parms.raidStrategy == null)
		{
			parms.raidStrategy = RaidStrategyDefOf.ImmediateAttackFriendly;
		}
	}

	protected override void ResolveRaidPoints(IncidentParms parms)
	{
		if (parms.points <= 0f)
		{
			parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
		}
	}

	protected override string GetLetterLabel(IncidentParms parms)
	{
		return parms.raidStrategy.letterLabelFriendly + ": " + parms.faction.Name;
	}

	protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
	{
		string text = string.Format(parms.raidArrivalMode.textFriendly, parms.faction.def.pawnsPlural, parms.faction.Name.ApplyTag(parms.faction));
		text += "\n\n";
		text += parms.raidStrategy.arrivalTextFriendly;
		Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
		if (pawn != null)
		{
			text += "\n\n";
			text += "FriendlyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
		}
		return text;
	}

	protected override LetterDef GetLetterDef()
	{
		return LetterDefOf.PositiveEvent;
	}

	protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
	{
		return "LetterRelatedPawnsRaidFriendly".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
	}
}
