using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class PawnsArrivalModeWorker_SpecificLocationDrop : PawnsArrivalModeWorker
{
	public const int PodOpenDelay = 520;

	public override void Arrive(List<Pawn> pawns, IncidentParms parms)
	{
		Map map = (Map)parms.target;
		bool flag = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
		List<List<Thing>> thingsGroups = pawns.Select((Pawn p) => new List<Thing> { p }).ToList();
		DropThingGroupsNear(parms.spawnCenter, map, thingsGroups, parms.dropInRadius, parms.podOpenDelay, instaDrop: false, leaveSlag: true, flag || parms.raidArrivalModeForQuickMilitaryAid, forbid: true, allowFogged: true, canTransfer: false, parms.faction);
	}

	public override void TravellingTransportersArrived(List<ActiveTransporterInfo> transporters, Map map)
	{
		throw new NotImplementedException("PawnsArrivalModeWorker_SpecificLocationDrop is only for raids");
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		_ = (Map)parms.target;
		if (!parms.raidArrivalModeForQuickMilitaryAid)
		{
			parms.podOpenDelay = 520;
		}
		parms.spawnRotation = Rot4.Random;
		return true;
	}

	private static void DropThingGroupsNear(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int radius, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true, bool forbid = true, bool allowFogged = true, bool canTransfer = false, Faction faction = null)
	{
		foreach (List<Thing> thingsGroup in thingsGroups)
		{
			if (!DropCellFinder.TryFindDropSpotNear(dropCenter, map, out var result, allowFogged, canRoofPunch, radius) && (canRoofPunch || !DropCellFinder.TryFindDropSpotNear(dropCenter, map, out result, allowFogged, canRoofPunch: true, radius)))
			{
				if (!dropCenter.IsValid)
				{
					continue;
				}
				string[] obj = new string[5]
				{
					"DropThingsNear failed to find a place to drop ",
					thingsGroup.FirstOrDefault()?.ToString(),
					" near ",
					null,
					null
				};
				IntVec3 intVec = dropCenter;
				obj[3] = intVec.ToString();
				obj[4] = ". Dropping on random square instead.";
				Log.Warning(string.Concat(obj));
				result = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(map) && !(c.GetRoof(map)?.isThickRoof ?? false), map);
			}
			if (forbid)
			{
				for (int num = 0; num < thingsGroup.Count; num++)
				{
					thingsGroup[num].SetForbidden(value: true, warnOnFail: false);
				}
			}
			if (instaDrop)
			{
				foreach (Thing item in thingsGroup)
				{
					GenPlace.TryPlaceThing(item, result, map, ThingPlaceMode.Near);
				}
				continue;
			}
			ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
			foreach (Thing item2 in thingsGroup)
			{
				activeTransporterInfo.innerContainer.TryAdd(item2);
			}
			activeTransporterInfo.openDelay = openDelay;
			activeTransporterInfo.leaveSlag = leaveSlag;
			DropPodUtility.MakeDropPodAt(result, map, activeTransporterInfo, faction);
		}
	}
}
