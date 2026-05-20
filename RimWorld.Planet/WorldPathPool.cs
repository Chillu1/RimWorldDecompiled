using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class WorldPathPool
{
	private readonly List<WorldPath> paths = new List<WorldPath>(64);

	public static WorldPath NotFoundPath { get; }

	static WorldPathPool()
	{
		NotFoundPath = WorldPath.NewNotFound();
	}

	public WorldPath GetEmptyWorldPath()
	{
		for (int i = 0; i < paths.Count; i++)
		{
			if (!paths[i].inUse)
			{
				paths[i].inUse = true;
				return paths[i];
			}
		}
		if (paths.Count > Find.WorldObjects.CaravansCount + 2 + (Find.WorldObjects.RoutePlannerWaypointsCount - 1))
		{
			Log.ErrorOnce("WorldPathPool leak: more paths than caravans. Force-recovering.", 664788);
			paths.Clear();
		}
		WorldPath worldPath = new WorldPath();
		paths.Add(worldPath);
		worldPath.inUse = true;
		return worldPath;
	}
}
