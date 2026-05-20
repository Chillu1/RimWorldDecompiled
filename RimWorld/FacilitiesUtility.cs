using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class FacilitiesUtility
{
	private const float MaxDistToLinkToFacilityEver = 10f;

	private static int RegionsToSearch = (1 + 2 * Mathf.CeilToInt(5f / 6f)) * (1 + 2 * Mathf.CeilToInt(5f / 6f));

	private static HashSet<Region> visited = new HashSet<Region>();

	private static HashSet<Thing> processed = new HashSet<Thing>();

	private static bool working;

	public static void NotifyFacilitiesAboutChangedLOSBlockers(List<Region> affectedRegions)
	{
		if (!affectedRegions.Any())
		{
			return;
		}
		if (working)
		{
			Log.Warning("Tried to update facilities while already updating.");
			return;
		}
		working = true;
		try
		{
			visited.Clear();
			processed.Clear();
			int facilitiesToProcess = affectedRegions[0].Map.listerThings.ThingsInGroup(ThingRequestGroup.Facility).Count;
			int affectedByFacilitiesToProcess = affectedRegions[0].Map.listerThings.ThingsInGroup(ThingRequestGroup.AffectedByFacilities).Count;
			int facilitiesProcessed = 0;
			int affectedByFacilitiesProcessed = 0;
			if (facilitiesToProcess <= 0 || affectedByFacilitiesToProcess <= 0)
			{
				return;
			}
			for (int i = 0; i < affectedRegions.Count; i++)
			{
				if (visited.Contains(affectedRegions[i]))
				{
					continue;
				}
				RegionTraverser.BreadthFirstTraverse(affectedRegions[i], (Region from, Region r) => !visited.Contains(r), delegate(Region x)
				{
					visited.Add(x);
					List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
					for (int j = 0; j < list.Count; j++)
					{
						if (!processed.Contains(list[j]))
						{
							processed.Add(list[j]);
							CompFacility compFacility = list[j].TryGetComp<CompFacility>();
							CompAffectedByFacilities compAffectedByFacilities = list[j].TryGetComp<CompAffectedByFacilities>();
							if (compFacility != null)
							{
								compFacility.Notify_LOSBlockerSpawnedOrDespawned();
								facilitiesProcessed++;
							}
							if (compAffectedByFacilities != null)
							{
								compAffectedByFacilities.Notify_LOSBlockerSpawnedOrDespawned();
								affectedByFacilitiesProcessed++;
							}
						}
					}
					return facilitiesProcessed >= facilitiesToProcess && affectedByFacilitiesProcessed >= affectedByFacilitiesToProcess;
				}, RegionsToSearch);
				if (facilitiesProcessed >= facilitiesToProcess && affectedByFacilitiesProcessed >= affectedByFacilitiesToProcess)
				{
					break;
				}
			}
		}
		finally
		{
			working = false;
			visited.Clear();
			processed.Clear();
		}
	}
}
