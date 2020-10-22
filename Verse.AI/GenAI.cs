using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI
{
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

		public static bool CanBeArrestedBy(this Pawn pawn, Pawn arrester)
		{
			if (!pawn.RaceProps.Humanlike)
			{
				return false;
			}
			if ((pawn.InAggroMentalState && pawn.HostileTo(arrester)) || pawn.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			if (pawn.IsPrisonerOfColony && pawn.Position.IsInPrisonCell(pawn.Map))
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
				Pawn pawn2 = t as Pawn;
				if (pawn2 != null && !pawn2.Downed && (float)(pawn.Position - pawn2.Position).LengthHorizontalSquared < 144f && pawn2.HostileTo(pawn.Faction))
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
					if (((Building_Bed)item).OwnersForReading.Any() && map.reachability.CanReach(raidSpawnLoc, item, PathEndMode.OnCell, TraverseMode.PassAllDestroyableThings, Danger.Deadly))
					{
						list.Add(item);
					}
				}
			}
			if (list.TryRandomElement(out var result))
			{
				return result.Position;
			}
			IEnumerable<Building> source = map.listerBuildings.allBuildingsColonist.Where((Building b) => !b.def.building.ai_combatDangerous && !b.def.building.isInert && !b.def.building.ai_neverTrashThis);
			if (source.Any())
			{
				for (int j = 0; j < 500; j++)
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

		public static bool EnemyIsNear(Pawn p, float radius)
		{
			if (!p.Spawned)
			{
				return false;
			}
			bool flag = p.Position.Fogged(p.Map);
			List<IAttackTarget> potentialTargetsFor = p.Map.attackTargetsCache.GetPotentialTargetsFor(p);
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				IAttackTarget attackTarget = potentialTargetsFor[i];
				if (!attackTarget.ThreatDisabled(p) && (flag || !attackTarget.Thing.Position.Fogged(attackTarget.Thing.Map)) && p.Position.InHorDistOf(((Thing)attackTarget).Position, radius))
				{
					return true;
				}
			}
			return false;
		}
	}
}
