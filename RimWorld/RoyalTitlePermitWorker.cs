using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoyalTitlePermitWorker
{
	public RoyalTitlePermitDef def;

	public virtual IEnumerable<Gizmo> GetPawnGizmos(Pawn pawn, Faction faction)
	{
		return null;
	}

	public virtual IEnumerable<Gizmo> GetCaravanGizmos(Pawn pawn, Faction faction)
	{
		return null;
	}

	public virtual IEnumerable<DiaOption> GetFactionCommDialogOptions(Map map, Pawn pawn, Faction factionInFavor)
	{
		return null;
	}

	public virtual IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
	{
		return null;
	}

	[Obsolete]
	protected virtual bool AidDisabled(Map map, Pawn pawn, Faction faction, out string reason)
	{
		return AidDisabled_NewTemp(map, pawn, faction, out reason);
	}

	protected virtual bool AidDisabled_NewTemp(Map map, Pawn pawn, Faction faction, out string reason, bool temperatureMatters = true)
	{
		if (faction.HostileTo(Faction.OfPlayer))
		{
			reason = "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION"));
			return true;
		}
		if (map.generatorDef.isUnderground)
		{
			reason = "CommandCallRoyalAidMapUnreachable".Translate(faction.Named("FACTION"));
			return true;
		}
		if (def.layerBlacklist.Contains(pawn.MapHeld.Tile.LayerDef))
		{
			reason = "CommandCallRoyalAidMapUnreachable".Translate(faction.Named("FACTION"));
			return true;
		}
		if (temperatureMatters && !TemperatureIsAcceptable(map, faction))
		{
			reason = "BadTemperature".Translate();
			return true;
		}
		reason = null;
		return false;
	}

	protected bool FillAidOption(Pawn pawn, Faction faction, ref string description, out bool free)
	{
		int lastUsedTick = pawn.royalty.GetPermit(def, faction).LastUsedTick;
		int num = Math.Max(GenTicks.TicksGame - lastUsedTick, 0);
		if (lastUsedTick < 0 || num >= def.CooldownTicks)
		{
			description += "CommandCallRoyalAidFreeOption".Translate();
			free = true;
			return true;
		}
		int numTicks = ((lastUsedTick > 0) ? Math.Max(def.CooldownTicks - num, 0) : 0);
		description += "CommandCallRoyalAidFavorOption".Translate(numTicks.TicksToDays().ToString("0.0"), def.royalAid.favorCost, faction.Named("FACTION"));
		if (pawn.royalty.GetFavor(faction) >= def.royalAid.favorCost)
		{
			free = false;
			return true;
		}
		free = false;
		return false;
	}

	protected bool FillCaravanAidOption(Pawn pawn, Faction faction, out string description, out bool free, out bool disableNotEnoughFavor)
	{
		description = def.description;
		disableNotEnoughFavor = false;
		free = false;
		if (!def.usableOnWorldMap)
		{
			free = false;
			return false;
		}
		int lastUsedTick = pawn.royalty.GetPermit(def, faction).LastUsedTick;
		int num = Math.Max(GenTicks.TicksGame - lastUsedTick, 0);
		if (lastUsedTick < 0 || num >= def.CooldownTicks)
		{
			description += " (" + "CommandCallRoyalAidFreeOption".Translate() + ")";
			free = true;
		}
		else
		{
			int numTicks = ((lastUsedTick > 0) ? Math.Max(def.CooldownTicks - num, 0) : 0);
			description += " (" + "CommandCallRoyalAidFavorOption".Translate(numTicks.TicksToDays().ToString("0.0"), def.royalAid.favorCost, faction.Named("FACTION")) + ")";
			if (pawn.royalty.GetFavor(faction) < def.royalAid.favorCost)
			{
				disableNotEnoughFavor = true;
			}
		}
		return true;
	}

	protected virtual bool TemperatureIsAcceptable(Map map, Faction faction)
	{
		return faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp);
	}
}
