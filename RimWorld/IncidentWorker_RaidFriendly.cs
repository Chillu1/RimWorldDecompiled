using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class IncidentWorker_RaidFriendly : IncidentWorker_Raid
	{
		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			IEnumerable<Faction> source = (from x in map.attackTargetsCache.TargetsHostileToColony
				where GenHostility.IsActiveThreatToPlayer(x)
				select x into p
				select ((Thing)p).Faction).Distinct();
			if (base.FactionCanBeGroupSource(f, map, desperate) && !f.Hidden && f.PlayerRelationKind == FactionRelationKind.Ally)
			{
				if (source.Any())
				{
					return source.Any((Faction hf) => hf.HostileTo(f));
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
			return ((Map)parms.target).attackTargetsCache.TargetsHostileToColony.Where((IAttackTarget p) => GenHostility.IsActiveThreatToPlayer(p)).Sum((IAttackTarget p) => (p as Pawn)?.kindDef.combatPower ?? 0f) > 120f;
		}

		protected override bool TryResolveRaidFaction(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (parms.faction != null)
			{
				return true;
			}
			if (!CandidateFactions(map).Any())
			{
				return false;
			}
			parms.faction = CandidateFactions(map).RandomElementByWeight((Faction fac) => (float)fac.PlayerGoodwill + 120.000008f);
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
			string str = string.Format(parms.raidArrivalMode.textFriendly, parms.faction.def.pawnsPlural, parms.faction.Name.ApplyTag(parms.faction));
			str += "\n\n";
			str += parms.raidStrategy.arrivalTextFriendly;
			Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
			if (pawn != null)
			{
				str += "\n\n";
				str += "FriendlyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
			}
			return str;
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
}
