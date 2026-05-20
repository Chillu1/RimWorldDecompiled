using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class GenGrid
{
	public const int NoBuildEdgeWidth = 10;

	public const int NoZoneEdgeWidth = 5;

	public static bool InNoBuildEdgeArea(this IntVec3 c, Map map)
	{
		if (map.IsPocketMap)
		{
			return false;
		}
		if (map.Tile.LayerDef.ignoreNoBuildArea)
		{
			return false;
		}
		return c.CloseToEdge(map, 10);
	}

	public static bool InNoZoneEdgeArea(this IntVec3 c, Map map)
	{
		return c.CloseToEdge(map, 5);
	}

	public static bool CloseToEdge(this IntVec3 c, Map map, int edgeDist)
	{
		IntVec3 size = map.Size;
		if (c.x >= edgeDist && c.z >= edgeDist && c.x < size.x - edgeDist)
		{
			return c.z >= size.z - edgeDist;
		}
		return true;
	}

	public static bool OnEdge(this IntVec3 c, Map map)
	{
		IntVec3 size = map.Size;
		if (c.x != 0 && c.x != size.x - 1 && c.z != 0)
		{
			return c.z == size.z - 1;
		}
		return true;
	}

	public static bool OnEdge(this IntVec3 c, Map map, Rot4 dir)
	{
		if (dir == Rot4.North)
		{
			return c.z == 0;
		}
		if (dir == Rot4.South)
		{
			return c.z == map.Size.z - 1;
		}
		if (dir == Rot4.West)
		{
			return c.x == 0;
		}
		if (dir == Rot4.East)
		{
			return c.x == map.Size.x - 1;
		}
		Log.ErrorOnce("Invalid edge direction", 55370769);
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool InBounds(this IntVec3 c, Map map)
	{
		IntVec3 size = map.info.Size;
		if ((uint)c.x < size.x)
		{
			return (uint)c.z < size.z;
		}
		return false;
	}

	public static bool InBounds(this IntVec3 c, Map map, int contractedBy)
	{
		IntVec3 size = map.Size;
		if (c.x >= contractedBy && c.z >= contractedBy && c.x < size.x - contractedBy)
		{
			return c.z < size.z - contractedBy;
		}
		return false;
	}

	public static bool InBounds(this Vector3 v, Map map)
	{
		IntVec3 size = map.Size;
		if (v.x >= 0f && v.z >= 0f && v.x < (float)size.x)
		{
			return v.z < (float)size.z;
		}
		return false;
	}

	public static CellRect BoundsRect(this Map map, int contractedBy = 0)
	{
		IntVec3 size = map.Size;
		return new CellRect(contractedBy, contractedBy, size.x - contractedBy * 2, size.z - contractedBy * 2);
	}

	public static bool WalkableByNormal(this IntVec3 c, Map map)
	{
		return map.pathing.Normal.pathGrid.Walkable(c);
	}

	public static bool WalkableByFenceBlocked(this IntVec3 c, Map map)
	{
		return map.pathing.FenceBlocked.pathGrid.Walkable(c);
	}

	public static bool WalkableBy(this IntVec3 c, Map map, Pawn pawn)
	{
		return map.pathing.For(pawn).pathGrid.Walkable(c);
	}

	public static bool WalkableByAny(this IntVec3 c, Map map)
	{
		if (!map.pathing.Normal.pathGrid.Walkable(c))
		{
			return map.pathing.FenceBlocked.pathGrid.Walkable(c);
		}
		return true;
	}

	public static bool Walkable(this IntVec3 c, Map map)
	{
		if (map.pathing.Normal.pathGrid.Walkable(c))
		{
			return map.pathing.FenceBlocked.pathGrid.Walkable(c);
		}
		return false;
	}

	public static bool Standable(this IntVec3 c, Map map)
	{
		if (!c.Walkable(map))
		{
			return false;
		}
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.passability != Traversability.Standable)
			{
				return false;
			}
		}
		return true;
	}

	public static bool StandableBy(this IntVec3 c, Map map, Pawn pawn)
	{
		if (!c.WalkableBy(map, pawn))
		{
			return false;
		}
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.passability != Traversability.Standable)
			{
				return false;
			}
		}
		return true;
	}

	public static bool StandableByAny(this IntVec3 c, Map map)
	{
		if (!c.WalkableByAny(map))
		{
			return false;
		}
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.passability != Traversability.Standable)
			{
				return false;
			}
		}
		return true;
	}

	public static bool Impassable(this IntVec3 c, Map map)
	{
		List<Thing> list = map.thingGrid.ThingsListAtFast(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.passability == Traversability.Impassable)
			{
				return true;
			}
		}
		return false;
	}

	public static bool SupportsStructureType(this IntVec3 c, Map map, TerrainAffordanceDef surfaceType)
	{
		return c.GetAffordances(map).Contains(surfaceType);
	}

	public static bool CanBeSeenOver(this IntVec3 c, Map map)
	{
		if (!c.InBounds(map))
		{
			return false;
		}
		Building edifice = c.GetEdifice(map);
		if (edifice != null && !edifice.CanBeSeenOver())
		{
			return false;
		}
		return true;
	}

	public static bool CanBeSeenOverFast(this IntVec3 c, Map map)
	{
		Building edifice = c.GetEdifice(map);
		if (edifice != null && !edifice.CanBeSeenOver())
		{
			return false;
		}
		return true;
	}

	public static bool CanBeSeenOver(this Building b)
	{
		if (b.def.Fillage != FillCategory.Full)
		{
			return true;
		}
		if (b is Building_Door { Open: not false })
		{
			return true;
		}
		return false;
	}

	public static SurfaceType GetSurfaceType(this IntVec3 c, Map map)
	{
		if (!c.InBounds(map))
		{
			return SurfaceType.None;
		}
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].def.surfaceType != SurfaceType.None)
			{
				return thingList[i].def.surfaceType;
			}
		}
		return SurfaceType.None;
	}

	public static bool HasEatSurface(this IntVec3 c, Map map)
	{
		return c.GetSurfaceType(map) == SurfaceType.Eat;
	}

	public static void Fit(this Camera camera, CellRect bounds, float yPos)
	{
		camera.transform.position = bounds.CenterVector3 + Vector3.up * yPos;
		camera.aspect = (float)bounds.Width / (float)bounds.Height;
		camera.orthographicSize = (float)bounds.Height / 2f;
	}
}
