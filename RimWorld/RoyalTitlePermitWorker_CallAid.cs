using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class RoyalTitlePermitWorker_CallAid : RoyalTitlePermitWorker_Targeted
{
	private Faction calledFaction;

	private float biocodeChance;

	public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction)
	{
		if (AidDisabled_NewTemp(map, pawn, faction, out var reason))
		{
			yield return new FloatMenuOption(def.LabelCap + ": " + reason, null);
			yield break;
		}
		if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(pawn.MapHeld, faction))
		{
			yield return new FloatMenuOption(def.LabelCap + ": " + "HostileVisitorsPresent".Translate(), null);
			yield break;
		}
		Action action = null;
		string description = def.LabelCap + ": ";
		if (FillAidOption(pawn, faction, ref description, out var free))
		{
			action = delegate
			{
				BeginCallAid(pawn, map, faction, free);
			};
		}
		yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
	}

	private void BeginCallAid(Pawn caller, Map map, Faction faction, bool free, float biocodeChance = 1f)
	{
		IEnumerable<Faction> source = from f in (from p in map.mapPawns.AllPawnsSpawned
				where p.Faction != null && !p.Faction.IsPlayer && p.Faction != faction && !p.IsPrisonerOfColony
				select p.Faction).Distinct()
			where f.HostileTo(Faction.OfPlayer) && !faction.HostileTo(f)
			select f;
		if (source.Any())
		{
			Find.WindowStack.Add(new Dialog_MessageBox("CommandCallRoyalAidWarningNonHostileFactions".Translate(faction, source.Select((Faction f) => f.NameColored.Resolve()).ToCommaList()), "Confirm".Translate(), Call, "GoBack".Translate()));
		}
		else
		{
			Call();
		}
		void Call()
		{
			targetingParameters = new TargetingParameters();
			targetingParameters.canTargetLocations = true;
			targetingParameters.canTargetSelf = false;
			targetingParameters.canTargetPawns = false;
			targetingParameters.canTargetFires = false;
			targetingParameters.canTargetBuildings = false;
			targetingParameters.canTargetItems = false;
			base.caller = caller;
			base.map = map;
			calledFaction = faction;
			base.free = free;
			this.biocodeChance = biocodeChance;
			float rangeActual = base.RangeClamped;
			targetingParameters.validator = delegate(TargetInfo target)
			{
				if (rangeActual > 0f && target.Cell.DistanceTo(caller.Position) > rangeActual)
				{
					return false;
				}
				if (target.Cell.Fogged(map) || !DropCellFinder.CanPhysicallyDropInto(target.Cell, map, canRoofPunch: true))
				{
					return false;
				}
				return target.Cell.GetEdifice(map) == null && !target.Cell.Impassable(map);
			};
			Find.Targeter.BeginTargeting(this);
		}
	}

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		CallAid(caller, map, target.Cell, calledFaction, free, biocodeChance);
	}

	private void CallAid(Pawn caller, Map map, IntVec3 spawnPos, Faction faction, bool free, float biocodeChance = 1f)
	{
		IncidentParms incidentParms = new IncidentParms();
		incidentParms.target = map;
		incidentParms.faction = faction;
		incidentParms.raidArrivalModeForQuickMilitaryAid = true;
		incidentParms.biocodeApparelChance = biocodeChance;
		incidentParms.biocodeWeaponsChance = biocodeChance;
		incidentParms.spawnCenter = spawnPos;
		if (def.royalAid.pawnKindDef != null)
		{
			incidentParms.pawnKind = def.royalAid.pawnKindDef;
			incidentParms.pawnCount = def.royalAid.pawnCount;
		}
		else
		{
			incidentParms.points = def.royalAid.points;
		}
		faction.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
		if (IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms))
		{
			if (!free)
			{
				caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
			}
			caller.royalty.GetPermit(def, faction).Notify_Used();
		}
		else
		{
			Log.Error("Could not send aid to map " + map?.ToString() + " from faction " + faction);
		}
	}

	protected override bool TemperatureIsAcceptable(Map map, Faction faction)
	{
		if (def.royalAid.pawnKindDef == null)
		{
			return base.TemperatureIsAcceptable(map, faction);
		}
		if (!def.royalAid.overrideAcceptableTemperatureRange.HasValue)
		{
			return base.TemperatureIsAcceptable(map, faction);
		}
		return def.royalAid.overrideAcceptableTemperatureRange.Value.Includes(map.mapTemperature.SeasonalTemp);
	}
}
