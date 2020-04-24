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

		public static bool CanTradeWith(this Pawn p, Faction faction, TraderKindDef traderKind = null)
		{
			if (p.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
			{
				return false;
			}
			if (faction != null)
			{
				if (faction.HostileTo(p.Faction))
				{
					return false;
				}
				if (traderKind == null || traderKind.permitRequiredForTrading == null)
				{
					return true;
				}
				if (p.royalty == null || !p.royalty.HasPermit(traderKind.permitRequiredForTrading, faction))
				{
					return false;
				}
			}
			return true;
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
