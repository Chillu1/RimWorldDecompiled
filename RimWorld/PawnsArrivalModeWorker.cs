using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public abstract class PawnsArrivalModeWorker
	{
		public PawnsArrivalModeDef def;

		public virtual bool CanUseWith(IncidentParms parms)
		{
			if (parms.faction != null)
			{
				if (def.minTechLevel != TechLevel.Undefined && (int)parms.faction.def.techLevel < (int)def.minTechLevel)
				{
					return false;
				}
				List<PawnsArrivalModeDef> arrivalModeWhitelist = parms.faction.def.arrivalModeWhitelist;
				List<PawnsArrivalModeDef> arrivalModeBlacklist = parms.faction.def.arrivalModeBlacklist;
				if (!arrivalModeWhitelist.NullOrEmpty() && !arrivalModeWhitelist.Contains(def))
				{
					return false;
				}
				if (!arrivalModeBlacklist.NullOrEmpty() && arrivalModeBlacklist.Contains(def))
				{
					return false;
				}
			}
			if (parms.raidArrivalModeForQuickMilitaryAid && !def.forQuickMilitaryAid)
			{
				return false;
			}
			if (parms.raidStrategy != null && !parms.raidStrategy.arriveModes.Contains(def))
			{
				return false;
			}
			if (parms.target is Map map && !CanUseOnMap(map))
			{
				return false;
			}
			return true;
		}

		public virtual bool CanUseOnMap(Map map)
		{
			if (!def.biomeWhitelist.NullOrEmpty() && !def.biomeWhitelist.Contains(map.Biome))
			{
				return false;
			}
			if (!def.biomeBlacklist.NullOrEmpty() && def.biomeBlacklist.Contains(map.Biome))
			{
				return false;
			}
			if (map.Biome.onlyAllowWhitelistedArrivalModes && (def.biomeWhitelist.NullOrEmpty() || !def.biomeWhitelist.Contains(map.Biome)))
			{
				return false;
			}
			if (map.Tile.Valid && !CanUseOnTile(map.Tile))
			{
				return false;
			}
			if (def.walkIn && !map.CanEverExit)
			{
				return false;
			}
			return true;
		}

		public virtual bool CanUseOnTile(PlanetTile tile)
		{
			if (!def.layerWhitelist.NullOrEmpty() && !def.layerWhitelist.Contains(tile.LayerDef))
			{
				return false;
			}
			if (!def.layerBlacklist.NullOrEmpty() && def.layerBlacklist.Contains(tile.LayerDef))
			{
				return false;
			}
			if (tile.LayerDef.onlyAllowWhitelistedArrivalModes && (def.layerWhitelist.NullOrEmpty() || !def.layerWhitelist.Contains(tile.LayerDef)))
			{
				return false;
			}
			return true;
		}

		public virtual float GetSelectionWeight(IncidentParms parms)
		{
			if (def.selectionWeightCurvesPerFaction != null && parms.faction != null)
			{
				List<FactionCurve> selectionWeightCurvesPerFaction = def.selectionWeightCurvesPerFaction;
				for (int i = 0; i < selectionWeightCurvesPerFaction.Count; i++)
				{
					if (selectionWeightCurvesPerFaction[i].faction == parms.faction.def)
					{
						return selectionWeightCurvesPerFaction[i].Evaluate(parms.points);
					}
				}
			}
			if (def.selectionWeightCurve != null)
			{
				return def.selectionWeightCurve.Evaluate(parms.points);
			}
			return 0f;
		}

		public abstract void Arrive(List<Pawn> pawns, IncidentParms parms);

		public virtual void TravellingTransportersArrived(List<ActiveTransporterInfo> transporters, Map map)
		{
			throw new NotSupportedException("Traveling transport pods arrived with mode " + def.defName);
		}

		public abstract bool TryResolveRaidSpawnCenter(IncidentParms parms);
	}
}
