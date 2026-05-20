using System.Collections.Generic;

namespace Verse;

public static class Autotests_RegionListers
{
	private static Dictionary<Region, List<Thing>> expectedListers = new Dictionary<Region, List<Thing>>();

	private static List<Region> tmpTouchableRegions = new List<Region>();

	public static void CheckBugs(Map map)
	{
		CalculateExpectedListers(map);
		CheckThingRegisteredTwice(map);
		CheckThingNotRegisteredButShould();
		CheckThingRegisteredButShouldnt(map);
	}

	private static void CheckThingRegisteredTwice(Map map)
	{
		foreach (KeyValuePair<Region, List<Thing>> expectedLister in expectedListers)
		{
			CheckDuplicates(expectedLister.Value, expectedLister.Key, expected: true);
		}
		foreach (Region allRegion in map.regionGrid.AllRegions)
		{
			CheckDuplicates(allRegion.ListerThings.AllThings, allRegion, expected: false);
		}
	}

	private static void CheckDuplicates(List<Thing> lister, Region region, bool expected)
	{
		for (int i = 1; i < lister.Count; i++)
		{
			for (int j = 0; j < i; j++)
			{
				if (lister[i] == lister[j])
				{
					if (expected)
					{
						Log.Error("Region error: thing " + lister[i]?.ToString() + " is expected to be registered twice in " + region?.ToString() + "? This should never happen.");
					}
					else
					{
						Log.Error("Region error: thing " + lister[i]?.ToString() + " is registered twice in " + region);
					}
				}
			}
		}
	}

	private static void CheckThingNotRegisteredButShould()
	{
		foreach (KeyValuePair<Region, List<Thing>> expectedLister in expectedListers)
		{
			List<Thing> value = expectedLister.Value;
			List<Thing> allThings = expectedLister.Key.ListerThings.AllThings;
			for (int i = 0; i < value.Count; i++)
			{
				if (!allThings.Contains(value[i]))
				{
					Log.Error("Region error: thing " + value[i]?.ToString() + " at " + value[i].Position.ToString() + " should be registered in " + expectedLister.Key?.ToString() + " but it's not.");
				}
			}
		}
	}

	private static void CheckThingRegisteredButShouldnt(Map map)
	{
		foreach (Region allRegion in map.regionGrid.AllRegions)
		{
			if (!expectedListers.TryGetValue(allRegion, out var value))
			{
				value = null;
			}
			List<Thing> allThings = allRegion.ListerThings.AllThings;
			for (int i = 0; i < allThings.Count; i++)
			{
				if (value == null || !value.Contains(allThings[i]))
				{
					Log.Error("Region error: thing " + allThings[i]?.ToString() + " at " + allThings[i].Position.ToString() + " is registered in " + allRegion?.ToString() + " but it shouldn't be.");
				}
			}
		}
	}

	private static void CalculateExpectedListers(Map map)
	{
		expectedListers.Clear();
		List<Thing> allThings = map.listerThings.AllThings;
		for (int i = 0; i < allThings.Count; i++)
		{
			Thing thing = allThings[i];
			if (!ListerThings.EverListable(thing.def, ListerThingsUse.Region))
			{
				continue;
			}
			RegionListersUpdater.GetTouchableRegions(thing, map, tmpTouchableRegions);
			for (int j = 0; j < tmpTouchableRegions.Count; j++)
			{
				Region key = tmpTouchableRegions[j];
				if (!expectedListers.TryGetValue(key, out var value))
				{
					value = new List<Thing>();
					expectedListers.Add(key, value);
				}
				value.Add(allThings[i]);
			}
		}
	}
}
