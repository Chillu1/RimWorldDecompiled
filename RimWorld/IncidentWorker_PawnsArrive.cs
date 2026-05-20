using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public abstract class IncidentWorker_PawnsArrive : IncidentWorker
{
	protected virtual bool MustHaveSettlementOnLayer => false;

	protected IEnumerable<Faction> CandidateFactions(IncidentParms parms, bool desperate = false)
	{
		return Find.FactionManager.AllFactions.Where((Faction f) => FactionCanBeGroupSource(f, parms, desperate));
	}

	public virtual bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
	{
		Map map = (Map)parms.target;
		if (f.IsPlayer)
		{
			return false;
		}
		if (f.defeated)
		{
			return false;
		}
		if (f.temporary)
		{
			return false;
		}
		if (!desperate && (!f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) || !f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp)))
		{
			return false;
		}
		if (MustHaveSettlementOnLayer && !f.Hidden && map.Tile.Valid && !Find.WorldObjects.AnyFactionSettlementOnLayer(f, map.Tile.Layer))
		{
			return false;
		}
		if (!f.def.arrivalLayerWhitelist.NullOrEmpty() && !f.def.arrivalLayerWhitelist.Contains(map.Tile.LayerDef))
		{
			return false;
		}
		if (!f.def.arrivalLayerBlacklist.NullOrEmpty() && f.def.arrivalLayerBlacklist.Contains(map.Tile.LayerDef))
		{
			return false;
		}
		if (map.Tile.LayerDef.onlyAllowWhitelistedArrivals && (f.def.arrivalLayerWhitelist.NullOrEmpty() || !f.def.arrivalLayerWhitelist.Contains(map.Tile.LayerDef)))
		{
			return false;
		}
		return true;
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (parms.faction == null)
		{
			return CandidateFactions(parms).Any();
		}
		return true;
	}

	public string DebugListingOfGroupSources()
	{
		StringBuilder stringBuilder = new StringBuilder();
		IncidentParms parms = new IncidentParms
		{
			target = Find.CurrentMap
		};
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			stringBuilder.Append(allFaction.Name);
			if (FactionCanBeGroupSource(allFaction, parms))
			{
				stringBuilder.Append("    YES");
			}
			else if (FactionCanBeGroupSource(allFaction, parms, desperate: true))
			{
				stringBuilder.Append("    YES-DESPERATE");
			}
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}
}
