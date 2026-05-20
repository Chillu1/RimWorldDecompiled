using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI;

public static class GenAI
{
	public static bool MachinesLike(Faction machineFaction, Pawn p)
	{
		if (p.Faction == null && p.NonHumanlikeOrWildMan() && (p.HostFaction != machineFaction || p.IsPrisoner))
		{
			return false;
		}
		if (p.IsPrisoner && p.HostFaction == machineFaction)
		{
			return false;
		}
		if (p.Faction != null && p.Faction.HostileTo(machineFaction))
		{
			return false;
		}
		return true;
	}

	public static bool CanUseItemForWork(Pawn p, Thing item)
	{
		if (item.IsForbidden(p))
		{
			return false;
		}
		if (!p.CanReserveAndReach(item, PathEndMode.ClosestTouch, p.NormalMaxDanger()))
		{
			return false;
		}
		return true;
	}

	public static bool CanBeCaptured(this Pawn pawn)
	{
		if (!pawn.RaceProps.Humanlike)
		{
			return false;
		}
		LifeStageDef curLifeStage = pawn.ageTracker.CurLifeStage;
		if (curLifeStage != null && curLifeStage.claimable)
		{
			return false;
		}
		if (pawn.IsSubhuman)
		{
			return false;
		}
		if (pawn.InMentalState && !pawn.Downed)
		{
			return false;
		}
		if (pawn.Faction == Faction.OfPlayer)
		{
			if (pawn.Downed)
			{
				if (!pawn.guilt.IsGuilty)
				{
					return pawn.IsPrisonerOfColony;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool CanBeArrestedBy(this Pawn pawn, Pawn arrester)
	{
		if (!pawn.RaceProps.Humanlike)
		{
			return false;
		}
		if (pawn.IsSubhuman)
		{
			return false;
		}
		if ((pawn.InAggroMentalState && pawn.HostileTo(arrester)) || pawn.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		if (pawn.IsPrisonerOfColony && pawn.Position.IsInPrisonCell(pawn.MapHeld))
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && Find.Anomaly.IsPawnHypnotized(pawn))
		{
			return false;
		}
		if (pawn.DevelopmentalStage.Baby())
		{
			return false;
		}
		return true;
	}

	public static bool InDangerousCombat(Pawn pawn)
	{
		Region root = pawn.GetRegion();
		bool found = false;
		RegionTraverser.BreadthFirstTraverse(root, (Region r1, Region r2) => r2.Room == root.Room, (Region r) => r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn).Any(delegate(Thing t)
		{
			if (t is Pawn { Downed: false } pawn2 && (float)(pawn.Position - pawn2.Position).LengthHorizontalSquared < 144f && pawn2.HostileTo(pawn.Faction))
			{
				found = true;
				return true;
			}
			return false;
		}), 9);
		return found;
	}

	public static IntVec3 RandomRaidDest(IntVec3 raidSpawnLoc, Map map)
	{
		List<ThingDef> allBedDefBestToWorst = RestUtility.AllBedDefBestToWorst;
		List<Building> list = new List<Building>(map.mapPawns.FreeColonistsAndPrisonersSpawnedCount);
		for (int i = 0; i < allBedDefBestToWorst.Count; i++)
		{
			foreach (Building item in map.listerBuildings.AllBuildingsColonistOfDef(allBedDefBestToWorst[i]))
			{
				if (((Building_Bed)item).OwnersForReading.Any() && !item.IsClearableFreeBuilding && map.reachability.CanReach(raidSpawnLoc, item, PathEndMode.OnCell, TraverseMode.PassAllDestroyableThings, Danger.Deadly))
				{
					list.Add(item);
				}
			}
		}
		if (list.TryRandomElement(out var result))
		{
			return result.Position;
		}
		IEnumerable<Building> source = map.listerBuildings.allBuildingsColonist.Where((Building b) => !b.def.building.ai_combatDangerous && !b.def.building.isInert && !b.def.building.ai_neverTrashThis && !b.IsClearableFreeBuilding);
		if (source.Any())
		{
			for (int num = 0; num < 500; num++)
			{
				IntVec3 intVec = source.RandomElement().RandomAdjacentCell8Way();
				if (intVec.Walkable(map) && map.reachability.CanReach(raidSpawnLoc, intVec, PathEndMode.OnCell, TraverseMode.PassAllDestroyableThings, Danger.Deadly))
				{
					return intVec;
				}
			}
		}
		if (map.mapPawns.FreeColonistsSpawned.Where((Pawn x) => map.reachability.CanReach(raidSpawnLoc, x, PathEndMode.OnCell, TraverseMode.PassAllDestroyableThings, Danger.Deadly)).TryRandomElement(out var result2))
		{
			return result2.Position;
		}
		if (CellFinderLoose.TryGetRandomCellWith((IntVec3 x) => map.reachability.CanReach(raidSpawnLoc, x, PathEndMode.OnCell, TraverseMode.PassAllDestroyableThings, Danger.Deadly), map, 1000, out var result3))
		{
			return result3;
		}
		return map.Center;
	}

	public static bool EnemyIsNear(Pawn p, float radius, bool meleeOnly = false)
	{
		Thing threat;
		return EnemyIsNear(p, radius, out threat, meleeOnly);
	}

	public static bool EnemyIsNear(Pawn p, float radius, out Thing threat, bool meleeOnly = false, bool requireLos = false)
	{
		threat = null;
		if (!p.Spawned)
		{
			return false;
		}
		bool flag = p.Position.Fogged(p.Map);
		List<IAttackTarget> potentialTargetsFor = p.Map.attackTargetsCache.GetPotentialTargetsFor(p);
		for (int i = 0; i < potentialTargetsFor.Count; i++)
		{
			IAttackTarget attackTarget = potentialTargetsFor[i];
			if (attackTarget.ThreatDisabled(p) || (!flag && attackTarget.Thing.Position.Fogged(attackTarget.Thing.Map)) || (requireLos && !GenSight.LineOfSightToThing(p.Position, attackTarget.Thing, p.MapHeld)))
			{
				continue;
			}
			if (meleeOnly && attackTarget is Pawn { equipment: not null } pawn)
			{
				CompEquippable primaryEq = pawn.equipment.PrimaryEq;
				if (primaryEq != null && !primaryEq.PrimaryVerb.IsMeleeAttack)
				{
					continue;
				}
			}
			if (p.Position.InHorDistOf(((Thing)attackTarget).Position, radius))
			{
				threat = (Thing)attackTarget;
				return true;
			}
		}
		return false;
	}
}
