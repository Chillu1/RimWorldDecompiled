using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Designator_ZoneAdd_Fishing : Designator_ZoneAdd
{
	private readonly List<IntVec3> freshWaterCells = new List<IntVec3>();

	private readonly List<IntVec3> saltWaterCells = new List<IntVec3>();

	private readonly List<Zone> selectedZones = new List<Zone>();

	protected override string NewZoneLabel => "Zone_Fishing".Translate();

	public override bool Visible
	{
		get
		{
			if (ModsConfig.OdysseyActive)
			{
				if (!ResearchProjectDefOf.Fishing.IsFinished)
				{
					return DebugSettings.godMode;
				}
				return true;
			}
			return false;
		}
	}

	protected virtual bool ShowRightClickHideOptions => true;

	public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
	{
		get
		{
			if (!ShowRightClickHideOptions)
			{
				yield break;
			}
			foreach (FloatMenuOption hideOption in Command_Hide_ZoneFishing.GetHideOptions())
			{
				yield return hideOption;
			}
		}
	}

	public Designator_ZoneAdd_Fishing()
	{
		zoneTypeToPlace = typeof(Zone_Fishing);
		defaultLabel = "Zone_Fishing".Translate();
		defaultDesc = "DesignatorFishingZoneDesc".Translate();
		tutorTag = "ZoneAdd_Fishing";
		icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Fishing");
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		if (c.GetWaterBodyType(base.Map) == WaterBodyType.None || c.GetTerrain(base.Map).passability == Traversability.Impassable || !base.Map.waterBodyTracker.AnyFishPopulationAt(c))
		{
			return "CannotPlaceFishingZone".Translate();
		}
		AcceptanceReport result = base.CanDesignateCell(c);
		if (!result.Accepted)
		{
			return result;
		}
		return true;
	}

	protected override Zone MakeNewZone()
	{
		if (!ModLister.CheckOdyssey("Fishing"))
		{
			return null;
		}
		return new Zone_Fishing(Find.CurrentMap.zoneManager);
	}

	public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
	{
		if (!ModLister.CheckOdyssey("Fishing"))
		{
			return;
		}
		try
		{
			bool flag = false;
			foreach (IntVec3 cell in cells)
			{
				if (!flag && (bool)CanDesignateCell(cell))
				{
					flag = true;
				}
				switch (cell.GetWaterBodyType(base.Map))
				{
				case WaterBodyType.Saltwater:
					saltWaterCells.Add(cell);
					break;
				case WaterBodyType.Freshwater:
					freshWaterCells.Add(cell);
					break;
				}
			}
			if (!flag)
			{
				Finalize(somethingSucceeded: false);
				return;
			}
			Zone_Fishing zone_Fishing = Find.Selector.SelectedZone as Zone_Fishing;
			WaterBodyType? obj = zone_Fishing?.Cells[0].GetWaterBodyType(base.Map);
			if (zone_Fishing != null)
			{
				selectedZones.Add(zone_Fishing);
			}
			if (obj != WaterBodyType.Saltwater)
			{
				Find.Selector.ClearSelection();
			}
			if (saltWaterCells.Count > 0)
			{
				base.DesignateMultiCell((IEnumerable<IntVec3>)saltWaterCells);
			}
			if (Find.Selector.SelectedZone is Zone_Fishing zone_Fishing2 && zone_Fishing2 != zone_Fishing)
			{
				selectedZones.Add(zone_Fishing2);
			}
			if (obj == WaterBodyType.Freshwater)
			{
				Find.Selector.Select(zone_Fishing, playSound: false, forceDesignatorDeselect: false);
			}
			else
			{
				Find.Selector.ClearSelection();
			}
			if (freshWaterCells.Count > 0)
			{
				base.DesignateMultiCell((IEnumerable<IntVec3>)freshWaterCells);
			}
			if (Find.Selector.SelectedZone is Zone_Fishing zone_Fishing3 && zone_Fishing3 != zone_Fishing)
			{
				selectedZones.Add(zone_Fishing3);
			}
			Find.Selector.ClearSelection();
			foreach (Zone selectedZone in selectedZones)
			{
				Find.Selector.Select(selectedZone, playSound: false, forceDesignatorDeselect: false);
			}
		}
		finally
		{
			saltWaterCells.Clear();
			freshWaterCells.Clear();
			selectedZones.Clear();
		}
	}

	protected override void FinalizeDesignationFailed()
	{
		base.FinalizeDesignationFailed();
		Messages.Message("CannotPlaceFishingZone".Translate(), MessageTypeDefOf.RejectInput, historical: false);
	}
}
