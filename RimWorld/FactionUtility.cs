using System.Linq;
using Verse;

namespace RimWorld
{
	public static class FactionUtility
	{
		public static bool HostileTo(this Faction fac, Faction other)
		{
			if (fac == null || other == null || other == fac)
			{
				return false;
			}
			return fac.RelationWith(other).kind == FactionRelationKind.Hostile;
		}

		public static bool AllyOrNeutralTo(this Faction fac, Faction other)
		{
			return !fac.HostileTo(other);
		}

		public static AcceptanceReport CanTradeWith_NewTemp(this Pawn p, Faction faction, TraderKindDef traderKind = null)
		{
			if (p.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
			{
				return AcceptanceReport.WasRejected;
			}
			if (faction != null)
			{
				if (faction.HostileTo(p.Faction))
				{
					return AcceptanceReport.WasRejected;
				}
				if (traderKind == null || traderKind.permitRequiredForTrading == null)
				{
					return AcceptanceReport.WasAccepted;
				}
				if (p.royalty == null || !p.royalty.HasPermit(traderKind.permitRequiredForTrading, faction))
				{
					return new AcceptanceReport("MessageNeedRoyalTitleToCallWithShip".Translate(traderKind.TitleRequiredToTrade));
				}
			}
			return AcceptanceReport.WasAccepted;
		}

		public static bool CanTradeWith(this Pawn p, Faction faction, TraderKindDef traderKind = null)
		{
			return p.CanTradeWith_NewTemp(faction, traderKind).Accepted;
		}

		public static Faction DefaultFactionFrom(FactionDef ft)
		{
			if (ft == null)
			{
				return null;
			}
			if (ft.isPlayer)
			{
				return Faction.OfPlayer;
			}
			if (Find.FactionManager.AllFactions.Where((Faction fac) => fac.def == ft).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}

		public static bool IsPoliticallyProper(this Thing thing, Pawn pawn)
		{
			if (thing.Faction == null)
			{
				return true;
			}
			if (pawn.Faction == null)
			{
				return true;
			}
			if (thing.Faction == pawn.Faction)
			{
				return true;
			}
			if (thing.Faction == pawn.HostFaction)
			{
				return true;
			}
			return false;
		}
	}
}
