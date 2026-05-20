using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class ListerBuildingsRepairable
{
	private Dictionary<Faction, List<Thing>> repairables = new Dictionary<Faction, List<Thing>>();

	private Dictionary<Faction, HashSet<Thing>> repairablesSet = new Dictionary<Faction, HashSet<Thing>>();

	public List<Thing> RepairableBuildings(Faction fac)
	{
		return ListFor(fac);
	}

	public bool Contains(Faction fac, Building b)
	{
		return HashSetFor(fac).Contains(b);
	}

	public void Notify_BuildingSpawned(Building b)
	{
		if (b.Faction != null)
		{
			UpdateBuilding(b);
		}
	}

	public void Notify_BuildingDeSpawned(Building b)
	{
		if (b.Faction != null)
		{
			ListFor(b.Faction).Remove(b);
			HashSetFor(b.Faction).Remove(b);
		}
	}

	public void Notify_BuildingTookDamage(Building b)
	{
		if (b.Faction != null)
		{
			UpdateBuilding(b);
		}
	}

	public void Notify_BuildingRepaired(Building b)
	{
		if (b.Faction != null)
		{
			UpdateBuilding(b);
		}
	}

	private void UpdateBuilding(Building b)
	{
		if (b.Faction == null || !b.def.building.repairable)
		{
			return;
		}
		List<Thing> list = ListFor(b.Faction);
		HashSet<Thing> hashSet = HashSetFor(b.Faction);
		if (b.HitPoints < b.MaxHitPoints)
		{
			if (!list.Contains(b))
			{
				list.Add(b);
			}
			hashSet.Add(b);
		}
		else
		{
			list.Remove(b);
			hashSet.Remove(b);
		}
	}

	private List<Thing> ListFor(Faction fac)
	{
		if (!repairables.TryGetValue(fac, out var value))
		{
			value = new List<Thing>();
			repairables.Add(fac, value);
		}
		return value;
	}

	private HashSet<Thing> HashSetFor(Faction fac)
	{
		if (!repairablesSet.TryGetValue(fac, out var value))
		{
			value = new HashSet<Thing>();
			repairablesSet.Add(fac, value);
		}
		return value;
	}

	internal string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			List<Thing> list = ListFor(allFaction);
			if (list.NullOrEmpty())
			{
				continue;
			}
			stringBuilder.AppendLine("=======" + allFaction.Name + " (" + allFaction.def?.ToString() + ")");
			foreach (Thing item in list)
			{
				stringBuilder.AppendLine(item.ThingID);
			}
		}
		return stringBuilder.ToString();
	}
}
