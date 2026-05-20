using Verse;

namespace RimWorld.Planet;

public class MoveableStockpile : MoveableArea
{
	private bool hidden;

	private StorageSettings settings;

	public MoveableStockpile()
	{
	}

	public MoveableStockpile(Gravship gravship, Zone_Stockpile stockpile)
		: base(gravship, stockpile.label, stockpile.RenamableLabel, stockpile.color, stockpile.ID)
	{
		hidden = stockpile.Hidden;
		settings = new StorageSettings();
		settings.CopyFrom(stockpile.settings);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref hidden, "hidden", defaultValue: false);
		Scribe_Deep.Look(ref settings, "settings");
	}

	public void TryCreateStockpile(ZoneManager zoneManager, IntVec3 newOrigin)
	{
		Zone_Stockpile zone_Stockpile = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, zoneManager)
		{
			label = label,
			Hidden = hidden,
			color = color,
			ID = id
		};
		zone_Stockpile.settings = new StorageSettings(zone_Stockpile);
		zone_Stockpile.settings.CopyFrom(settings);
		zoneManager.RegisterZone(zone_Stockpile);
		foreach (IntVec3 relativeCell in base.RelativeCells)
		{
			zone_Stockpile.AddCell(newOrigin + relativeCell);
		}
	}
}
