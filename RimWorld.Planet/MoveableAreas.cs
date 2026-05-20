using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class MoveableAreas : IExposable
{
	public List<MoveableStockpile> stockpiles = new List<MoveableStockpile>();

	public List<MoveableGrowZone> growZones = new List<MoveableGrowZone>();

	public List<MoveableArea_Allowed> allowedAreas = new List<MoveableArea_Allowed>();

	public List<MoveableStorageGroup> storageGroups = new List<MoveableStorageGroup>();

	public MoveableArea_Allowed homeArea;

	public MoveableArea buildRoofArea;

	public MoveableArea noRoofArea;

	public MoveableArea snowClearArea;

	public MoveableArea pollutionClearArea;

	public void ExposeData()
	{
		Scribe_Collections.Look(ref stockpiles, "stockpiles", LookMode.Deep);
		Scribe_Collections.Look(ref growZones, "growZones", LookMode.Deep);
		Scribe_Collections.Look(ref allowedAreas, "allowedAreas", LookMode.Deep);
		Scribe_Collections.Look(ref storageGroups, "storageGroups", LookMode.Deep);
		Scribe_Deep.Look(ref homeArea, "homeArea");
		Scribe_Deep.Look(ref buildRoofArea, "buildRoofArea");
		Scribe_Deep.Look(ref noRoofArea, "noRoofArea");
		Scribe_Deep.Look(ref snowClearArea, "snowClearArea");
		Scribe_Deep.Look(ref pollutionClearArea, "pollutionClearArea");
	}
}
