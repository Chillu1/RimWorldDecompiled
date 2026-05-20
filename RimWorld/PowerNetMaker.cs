using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class PowerNetMaker
{
	private static HashSet<Building> closedSet = new HashSet<Building>();

	private static HashSet<Building> openSet = new HashSet<Building>();

	private static HashSet<Building> currentSet = new HashSet<Building>();

	private static IEnumerable<CompPower> ContiguousPowerBuildings(Building root)
	{
		closedSet.Clear();
		openSet.Clear();
		currentSet.Clear();
		openSet.Add(root);
		do
		{
			foreach (Building item in openSet)
			{
				closedSet.Add(item);
			}
			HashSet<Building> hashSet = currentSet;
			currentSet = openSet;
			openSet = hashSet;
			openSet.Clear();
			foreach (Building item2 in currentSet)
			{
				foreach (IntVec3 item3 in GenAdj.CellsAdjacentCardinal(item2))
				{
					if (!item3.InBounds(item2.Map))
					{
						continue;
					}
					List<Thing> thingList = item3.GetThingList(item2.Map);
					for (int i = 0; i < thingList.Count; i++)
					{
						if (thingList[i] is Building { TransmitsPowerNow: not false } building && !openSet.Contains(building) && !currentSet.Contains(building) && !closedSet.Contains(building))
						{
							openSet.Add(building);
							break;
						}
					}
				}
			}
		}
		while (openSet.Count > 0);
		CompPower[] result = closedSet.Select((Building b) => b.PowerComp).ToArray();
		closedSet.Clear();
		openSet.Clear();
		currentSet.Clear();
		return result;
	}

	public static PowerNet NewPowerNetStartingFrom(Building root)
	{
		return new PowerNet(ContiguousPowerBuildings(root));
	}

	public static void UpdateVisualLinkagesFor(PowerNet net)
	{
	}
}
