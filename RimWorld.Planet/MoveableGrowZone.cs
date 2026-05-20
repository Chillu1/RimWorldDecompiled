using Verse;

namespace RimWorld.Planet;

public class MoveableGrowZone : MoveableArea
{
	public ThingDef plantDef;

	public bool allowSow;

	public bool allowCut;

	public MoveableGrowZone(Gravship gravship, Zone_Growing growZone)
		: base(gravship, growZone.label, growZone.RenamableLabel, growZone.color, growZone.ID)
	{
		plantDef = growZone.PlantDefToGrow;
		allowSow = growZone.allowSow;
		allowCut = growZone.allowCut;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref plantDef, "plantDef");
		Scribe_Values.Look(ref allowSow, "allowSow", defaultValue: false);
		Scribe_Values.Look(ref allowCut, "allowCut", defaultValue: false);
	}

	public void TryCreateGrowZone(ZoneManager zoneManager, IntVec3 newOrigin)
	{
		Zone_Growing zone_Growing = new Zone_Growing(zoneManager)
		{
			label = label,
			color = color,
			ID = id
		};
		zone_Growing.SetPlantDefToGrow(plantDef);
		zone_Growing.allowSow = allowSow;
		zone_Growing.allowCut = allowCut;
		zoneManager.RegisterZone(zone_Growing);
		foreach (IntVec3 relativeCell in base.RelativeCells)
		{
			zone_Growing.AddCell(newOrigin + relativeCell);
		}
	}
}
