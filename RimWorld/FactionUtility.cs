using System.Linq;
using Verse;

namespace RimWorld;

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

	public static AcceptanceReport CanTradeWith(this Pawn p, Faction faction, TraderKindDef traderKind = null)
	{
		if (p.skills == null || p.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
		{
			return new AcceptanceReport("IncapableOfCapacity".Translate(SkillDefOf.Social.label));
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
		if (!Find.FactionManager.AllFactions.Where((Faction x) => x.def == ft).TryRandomElement(out var result) && !Find.FactionManager.AllFactions.Where((Faction x) => x.def.replacesFaction != null && x.def.replacesFaction == ft).TryRandomElement(out result))
		{
			return null;
		}
		return result;
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

	public static bool IsPlayerSafe(this Faction faction)
	{
		return faction?.IsPlayer ?? false;
	}

	public static void ResetAllFactionRelations()
	{
		foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
		{
			item.RemoveAllRelations();
			foreach (Faction item2 in Find.FactionManager.AllFactionsListForReading)
			{
				if (item != item2)
				{
					item.TryMakeInitialRelationsWith(item2);
				}
			}
		}
	}

	public static int GetSlavesInFactionCount(Faction faction)
	{
		if (faction == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction))
		{
			if (item.IsSlave)
			{
				num++;
			}
		}
		return num;
	}
}
