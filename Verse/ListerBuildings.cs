using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.AI;

namespace Verse;

public sealed class ListerBuildings : IDisposable
{
	public class TrackingScope : IDisposable, IEnumerable<Building>, IEnumerable
	{
		private bool enabled;

		private readonly ListerBuildings parent;

		public readonly HashSet<Building> tracked;

		public readonly Predicate<Building> predicate;

		public bool Enabled => enabled;

		public TrackingScope(ListerBuildings parent, Predicate<Building> predicate = null)
		{
			tracked = SimplePool<HashSet<Building>>.Get();
			this.parent = parent;
			this.predicate = predicate;
			Start();
		}

		public void Dispose()
		{
			tracked.Clear();
			SimplePool<HashSet<Building>>.Return(tracked);
			Stop();
		}

		public void Start()
		{
			if (!enabled)
			{
				enabled = true;
				parent.StartTracker(this);
			}
		}

		public void Stop()
		{
			if (enabled)
			{
				enabled = false;
				parent.ReleaseTracker(this);
			}
		}

		public IEnumerator<Building> GetEnumerator()
		{
			return tracked.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return tracked.GetEnumerator();
		}
	}

	public readonly List<Building> allBuildingsColonist = new List<Building>();

	public readonly List<Building> allBuildingsNonColonist = new List<Building>();

	public readonly HashSet<Building> allBuildingsColonistCombatTargets = new HashSet<Building>();

	public readonly HashSet<Building> allBuildingsColonistElecFire = new HashSet<Building>();

	public readonly HashSet<Building> allBuildingsAnimalPenMarkers = new HashSet<Building>();

	public readonly HashSet<Building> allBuildingsHitchingPosts = new HashSet<Building>();

	private readonly Dictionary<Thing, Blueprint_Install> reinstallationMap = new Dictionary<Thing, Blueprint_Install>();

	private readonly List<TrackingScope> scopes = new List<TrackingScope>();

	private static List<Building> allBuildingsColonistOfDefResult = new List<Building>();

	private static List<Building> allBuildingsColonistOfGroupResult = new List<Building>();

	public TrackingScope Track(Predicate<Building> predicate = null)
	{
		return new TrackingScope(this, predicate);
	}

	private void StartTracker(TrackingScope scope)
	{
		scopes.Add(scope);
	}

	private void ReleaseTracker(TrackingScope scope)
	{
		scopes.Remove(scope);
	}

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
		if (b.TryGetComp<CompAnimalPenMarker>() != null)
		{
			allBuildingsAnimalPenMarkers.Add(b);
		}
		if (b.def == ThingDefOf.CaravanPackingSpot)
		{
			allBuildingsHitchingPosts.Add(b);
		}
		foreach (TrackingScope scope in scopes)
		{
			if ((scope.Enabled && scope.predicate == null) || scope.predicate(b))
			{
				scope.tracked.Add(b);
			}
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
		allBuildingsAnimalPenMarkers.Remove(b);
		allBuildingsHitchingPosts.Remove(b);
		foreach (TrackingScope scope in scopes)
		{
			if (scope.Enabled)
			{
				scope.tracked.Remove(b);
			}
		}
	}

	public void RegisterInstallBlueprint(Blueprint_Install blueprint)
	{
		reinstallationMap.Add(blueprint.MiniToInstallOrBuildingToReinstall.GetInnerIfMinified(), blueprint);
	}

	public void DeregisterInstallBlueprint(Blueprint_Install blueprint)
	{
		Thing thing = blueprint.MiniToInstallOrBuildingToReinstall?.GetInnerIfMinified();
		if (thing != null)
		{
			reinstallationMap.Remove(thing);
			return;
		}
		Thing thing2 = null;
		foreach (KeyValuePair<Thing, Blueprint_Install> item in reinstallationMap)
		{
			if (item.Value == blueprint)
			{
				thing2 = item.Key;
				break;
			}
		}
		if (thing2 != null)
		{
			reinstallationMap.Remove(thing2);
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
		return AllBuildingsColonistOfClass<Building_ResearchBench>().Any();
	}

	public bool ColonistsHaveBuildingWithPowerOn(ThingDef def)
	{
		List<Building> list = AllBuildingsColonistOfDef(def);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def == def)
			{
				CompPowerTrader compPowerTrader = list[i].TryGetComp<CompPowerTrader>();
				if (compPowerTrader == null || compPowerTrader.PowerOn)
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<Building> AllBuildingsColonistOfDef(ThingDef def)
	{
		allBuildingsColonistOfDefResult.Clear();
		for (int i = 0; i < allBuildingsColonist.Count; i++)
		{
			if (allBuildingsColonist[i].def == def)
			{
				allBuildingsColonistOfDefResult.Add(allBuildingsColonist[i]);
			}
		}
		return allBuildingsColonistOfDefResult;
	}

	public List<Building> AllBuildingsColonistOfGroup(ThingRequestGroup group)
	{
		allBuildingsColonistOfGroupResult.Clear();
		for (int i = 0; i < allBuildingsColonist.Count; i++)
		{
			if (group.Includes(allBuildingsColonist[i].def))
			{
				allBuildingsColonistOfGroupResult.Add(allBuildingsColonist[i]);
			}
		}
		return allBuildingsColonistOfGroupResult;
	}

	public IEnumerable<T> AllBuildingsColonistOfClass<T>() where T : Building
	{
		for (int i = 0; i < allBuildingsColonist.Count; i++)
		{
			if (allBuildingsColonist[i] is T val)
			{
				yield return val;
			}
		}
	}

	public IEnumerable<T> AllColonistBuildingsOfType<T>()
	{
		for (int i = 0; i < allBuildingsColonist.Count; i++)
		{
			Building building = allBuildingsColonist[i];
			if (building is T)
			{
				yield return (T)(object)((building is T) ? building : null);
			}
		}
	}

	public IEnumerable<Building> AllBuildingsNonColonistOfDef(ThingDef def)
	{
		for (int i = 0; i < allBuildingsNonColonist.Count; i++)
		{
			if (allBuildingsNonColonist[i].def == def)
			{
				yield return allBuildingsNonColonist[i];
			}
		}
	}

	public bool TryGetReinstallBlueprint(Thing building, out Blueprint_Install bp)
	{
		return reinstallationMap.TryGetValue(building, out bp);
	}

	public void Notify_FactionRemoved(Faction faction)
	{
		for (int i = 0; i < allBuildingsNonColonist.Count; i++)
		{
			if (allBuildingsNonColonist[i].Faction == faction)
			{
				allBuildingsNonColonist[i].SetFaction(null);
			}
		}
	}

	public void Dispose()
	{
		allBuildingsColonist.Clear();
		allBuildingsNonColonist.Clear();
		allBuildingsColonistCombatTargets.Clear();
		allBuildingsColonistElecFire.Clear();
		allBuildingsAnimalPenMarkers.Clear();
		allBuildingsHitchingPosts.Clear();
		reinstallationMap.Clear();
	}
}
