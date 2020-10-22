using System;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace Verse
{
	public sealed class ListerBuildings
	{
		public List<Building> allBuildingsColonist = new List<Building>();

		public List<Building> allBuildingsNonColonist = new List<Building>();

		public HashSet<Building> allBuildingsColonistCombatTargets = new HashSet<Building>();

		public HashSet<Building> allBuildingsColonistElecFire = new HashSet<Building>();

		public void Add(Building b)
		{
			if (b.def.building != null && b.def.building.isNaturalRock)
			{
				return;
			}
			if (b.Faction == Faction.OfPlayer)
			{
				allBuildingsColonist.Add(b);
				if (b is IAttackTarget)
				{
					allBuildingsColonistCombatTargets.Add(b);
				}
			}
			else
			{
				allBuildingsNonColonist.Add(b);
			}
			CompProperties_Power compProperties = b.def.GetCompProperties<CompProperties_Power>();
			if (compProperties != null && compProperties.shortCircuitInRain)
			{
				allBuildingsColonistElecFire.Add(b);
			}
		}

		public void Remove(Building b)
		{
			allBuildingsColonist.Remove(b);
			allBuildingsNonColonist.Remove(b);
			if (b is IAttackTarget)
			{
				allBuildingsColonistCombatTargets.Remove(b);
			}
			CompProperties_Power compProperties = b.def.GetCompProperties<CompProperties_Power>();
			if (compProperties != null && compProperties.shortCircuitInRain)
			{
				allBuildingsColonistElecFire.Remove(b);
			}
		}

		public bool ColonistsHaveBuilding(ThingDef def)
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def == def)
				{
					return true;
				}
			}
			return false;
		}

		public bool ColonistsHaveBuilding(Func<Thing, bool> predicate)
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (predicate(allBuildingsColonist[i]))
				{
					return true;
				}
			}
			return false;
		}

		public bool ColonistsHaveResearchBench()
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i] is Building_ResearchBench)
				{
					return true;
				}
			}
			return false;
		}

		public bool ColonistsHaveBuildingWithPowerOn(ThingDef def)
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def == def)
				{
					CompPowerTrader compPowerTrader = allBuildingsColonist[i].TryGetComp<CompPowerTrader>();
					if (compPowerTrader == null || compPowerTrader.PowerOn)
					{
						return true;
					}
				}
			}
			return false;
		}

		public IEnumerable<Building> AllBuildingsColonistOfDef(ThingDef def)
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def == def)
				{
					yield return allBuildingsColonist[i];
				}
			}
		}

		public IEnumerable<T> AllBuildingsColonistOfClass<T>() where T : Building
		{
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				T val = allBuildingsColonist[i] as T;
				if (val != null)
				{
					yield return val;
				}
			}
		}
	}
}
