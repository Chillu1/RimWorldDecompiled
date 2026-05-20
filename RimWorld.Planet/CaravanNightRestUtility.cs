using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class CaravanNightRestUtility
{
	public const float WakeUpHour = 6f;

	public const float RestStartHour = 22f;

	public static bool RestingNowAt(PlanetTile tile)
	{
		return WouldBeRestingAt(tile, GenTicks.TicksAbs);
	}

	public static bool WouldBeRestingAt(PlanetTile tile, long ticksAbs)
	{
		float num = GenDate.HourFloat(ticksAbs, Find.WorldGrid.LongLatOf(tile).x);
		if (!(num < 6f))
		{
			return num > 22f;
		}
		return true;
	}

	public static int LeftRestTicksAt(PlanetTile tile)
	{
		return LeftRestTicksAt(tile, GenTicks.TicksAbs);
	}

	public static int LeftRestTicksAt(PlanetTile tile, long ticksAbs)
	{
		if (!WouldBeRestingAt(tile, ticksAbs))
		{
			return 0;
		}
		float num = GenDate.HourFloat(ticksAbs, Find.WorldGrid.LongLatOf(tile).x);
		if (num < 6f)
		{
			return Mathf.CeilToInt((6f - num) * 2500f);
		}
		return Mathf.CeilToInt((24f - num + 6f) * 2500f);
	}

	public static int LeftNonRestTicksAt(PlanetTile tile)
	{
		return LeftNonRestTicksAt(tile, GenTicks.TicksAbs);
	}

	public static int LeftNonRestTicksAt(PlanetTile tile, long ticksAbs)
	{
		if (WouldBeRestingAt(tile, ticksAbs))
		{
			return 0;
		}
		float num = GenDate.HourFloat(ticksAbs, Find.WorldGrid.LongLatOf(tile).x);
		return Mathf.CeilToInt((22f - num) * 2500f);
	}
}
