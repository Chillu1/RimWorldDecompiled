using Verse;

namespace RimWorld;

public static class AutoHomeAreaMaker
{
	private const int BorderWidth = 4;

	private static bool ShouldAdd()
	{
		if (Find.PlaySettings.autoHomeArea)
		{
			return Current.ProgramState == ProgramState.Playing;
		}
		return false;
	}

	public static void Notify_BuildingSpawned(Thing b)
	{
		if (ShouldAdd() && b.def.building.expandHomeArea && b.Faction == Faction.OfPlayer)
		{
			MarkHomeAroundThing(b);
		}
	}

	public static void Notify_BuildingClaimed(Thing b)
	{
		if (ShouldAdd() && b.def.building.expandHomeArea && b.Faction == Faction.OfPlayer)
		{
			MarkHomeAroundThing(b);
		}
	}

	public static void MarkHomeAroundThing(Thing t)
	{
		if (!ShouldAdd())
		{
			return;
		}
		CellRect cellRect = new CellRect(t.Position.x - t.RotatedSize.x / 2 - 4, t.Position.z - t.RotatedSize.z / 2 - 4, t.RotatedSize.x + 8, t.RotatedSize.z + 8);
		cellRect.ClipInsideMap(t.Map);
		foreach (IntVec3 item in cellRect)
		{
			t.Map.areaManager.Home[item] = true;
		}
	}

	public static void Notify_ZoneCellAdded(IntVec3 c, Zone zone)
	{
		if (!ShouldAdd())
		{
			return;
		}
		foreach (IntVec3 item in CellRect.CenteredOn(c, 4).ClipInsideMap(zone.Map))
		{
			zone.Map.areaManager.Home[item] = true;
		}
	}
}
