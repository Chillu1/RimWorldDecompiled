using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class CocoonInfestationUtility
{
	private const float MinDistanceBetweenCocoons = 2f;

	private static List<ThingDef> tmpCocoonsToSpawn = new List<ThingDef>();

	public static bool TryFindCocoonSpawnPositionNear(IntVec3 root, Map map, ThingDef cocoon, out IntVec3 result)
	{
		result = CellFinder.FindNoWipeSpawnLocNear(root, map, cocoon, Rot4.North, Mathf.FloorToInt(GenRadial.MaxRadialPatternRadius), (IntVec3 c) => CanSpawnCocoonAt(c, map));
		return result.IsValid;
	}

	public static bool CanSpawnCocoonAt(IntVec3 c, Map map)
	{
		if (!c.Walkable(map) || c.GetFirstThing(map, ThingDefOf.InsectJelly) != null || c.GetFirstThing(map, ThingDefOf.GlowPod) != null)
		{
			return false;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(c, 2f, useCenter: true))
		{
			if (!item.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is CocoonSpawner)
				{
					return false;
				}
				if (thingList[i].def.building != null)
				{
					if (thingList[i].def.building.isInsectCocoon)
					{
						return false;
					}
					if (thingList[i].def.passability == Traversability.Impassable)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public static IEnumerable<ThingDef> GetCocoonsToSpawn(float points)
	{
		IEnumerable<ThingDef> allCocoons = DefDatabase<ThingDef>.AllDefs.Where((ThingDef t) => t.building != null && t.building.isInsectCocoon);
		int tries = 0;
		ThingDef cocoonDef;
		while (points > 0f && allCocoons.TryRandomElement(out cocoonDef))
		{
			yield return cocoonDef;
			points -= cocoonDef.building.combatPower;
			tries++;
			if (tries > 1000)
			{
				Log.Warning("Failed to spend all points when selecing cocoons to spawn.");
				break;
			}
			cocoonDef = null;
		}
	}

	public static List<Thing> SpawnCocoonInfestation(IntVec3 root, Map map, float points)
	{
		List<Thing> list = new List<Thing>();
		if (!ModLister.CheckBiotech("Spawn cocoon infestation"))
		{
			return list;
		}
		tmpCocoonsToSpawn.Clear();
		tmpCocoonsToSpawn.AddRange(GetCocoonsToSpawn(points));
		if (tmpCocoonsToSpawn.Count == 0)
		{
			Log.Warning("Failed to find cocoons to spawn with points:" + points);
			return list;
		}
		int nextCocoonGroupID = Find.UniqueIDsManager.GetNextCocoonGroupID();
		IntVec3 result;
		for (int i = 0; i < tmpCocoonsToSpawn.Count && TryFindCocoonSpawnPositionNear(root, map, tmpCocoonsToSpawn[i], out result); i++)
		{
			list.Add(GenSpawn.Spawn(MakeCocoon(tmpCocoonsToSpawn[i], nextCocoonGroupID), result, map, WipeMode.FullRefund));
		}
		tmpCocoonsToSpawn.Clear();
		return list;
	}

	private static Thing MakeCocoon(ThingDef cocoonDef, int groupID)
	{
		CocoonSpawner obj = (CocoonSpawner)ThingMaker.MakeThing(ThingDefOf.CocoonSpawner);
		obj.cocoon = cocoonDef;
		obj.groupID = groupID;
		return obj;
	}
}
