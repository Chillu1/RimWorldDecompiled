using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class Designator_ZoneAdd : Designator_Zone
{
	protected Type zoneTypeToPlace;

	private List<IntVec3> unsetCells = new List<IntVec3>();

	private Zone SelectedZone
	{
		get
		{
			return Find.Selector.SelectedZone;
		}
		set
		{
			Find.Selector.ClearSelection();
			if (value != null)
			{
				Find.Selector.Select(value, playSound: false, forceDesignatorDeselect: false);
			}
		}
	}

	protected abstract string NewZoneLabel { get; }

	protected abstract Zone MakeNewZone();

	public Designator_ZoneAdd()
	{
		soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
		soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
		soundSucceeded = SoundDefOf.Designate_ZoneAdd;
		useMouseIcon = true;
		hotKey = KeyBindingDefOf.Misc6;
	}

	public override void SelectedUpdate()
	{
		base.SelectedUpdate();
		if (Find.Selector.SelectedZone != null && Find.Selector.SelectedZone.GetType() != zoneTypeToPlace)
		{
			Find.Selector.Deselect(Find.Selector.SelectedZone);
		}
	}

	public override void DrawMouseAttachments()
	{
		if (useMouseIcon)
		{
			string text = "";
			if (!Input.GetKey(KeyCode.Mouse0))
			{
				Zone selectedZone = Find.Selector.SelectedZone;
				text = ((selectedZone == null) ? ((string)"CreateNewZone".Translate(NewZoneLabel)) : ((string)"ExpandOrCreateZone".Translate(selectedZone.label, NewZoneLabel)));
			}
			GenUI.DrawMouseAttachment(icon, text);
		}
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		if (!c.InBounds(base.Map))
		{
			return false;
		}
		Zone zone = base.Map.zoneManager.ZoneAt(c);
		if (zone != null && zone.GetType() != zoneTypeToPlace)
		{
			return false;
		}
		return IsZoneableCell(c, base.Map);
	}

	public static AcceptanceReport IsZoneableCell(IntVec3 c, Map map)
	{
		if (!c.InBounds(map))
		{
			return false;
		}
		if (c.Fogged(map))
		{
			return false;
		}
		if (c.InNoZoneEdgeArea(map))
		{
			return "TooCloseToMapEdge".Translate();
		}
		foreach (Thing item in map.thingGrid.ThingsAt(c))
		{
			if (!item.def.CanOverlapZones)
			{
				return false;
			}
		}
		return true;
	}

	public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
	{
		unsetCells.Clear();
		unsetCells.AddRange(cells);
		bool flag = false;
		if (unsetCells.Count == 1)
		{
			Zone zone = base.Map.zoneManager.ZoneAt(unsetCells[0]);
			if (zone != null)
			{
				if (zone.GetType() == zoneTypeToPlace)
				{
					SelectedZone = zone;
				}
				return;
			}
		}
		if (SelectedZone == null)
		{
			Zone zone2 = null;
			foreach (IntVec3 cell in cells)
			{
				Zone zone3 = base.Map.zoneManager.ZoneAt(cell);
				if (zone3 != null && zone3.GetType() == zoneTypeToPlace)
				{
					if (zone2 == null)
					{
						zone2 = zone3;
					}
					else if (zone3 != zone2)
					{
						zone2 = null;
						break;
					}
				}
			}
			SelectedZone = zone2;
		}
		unsetCells.RemoveAll((IntVec3 c2) => base.Map.zoneManager.ZoneAt(c2) != null);
		if (unsetCells.Count == 0 || (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, unsetCells))))
		{
			return;
		}
		if (SelectedZone == null)
		{
			SelectedZone = MakeNewZone();
			base.Map.zoneManager.RegisterZone(SelectedZone);
			SelectedZone.AddCell(unsetCells[0]);
			unsetCells.RemoveAt(0);
		}
		while (true)
		{
			flag = true;
			int count = unsetCells.Count;
			for (int num = unsetCells.Count - 1; num >= 0; num--)
			{
				bool flag2 = false;
				for (int num2 = 0; num2 < 4; num2++)
				{
					IntVec3 c = unsetCells[num] + GenAdj.CardinalDirections[num2];
					if (c.InBounds(base.Map) && base.Map.zoneManager.ZoneAt(c) == SelectedZone)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					SelectedZone.AddCell(unsetCells[num]);
					unsetCells.RemoveAt(num);
				}
			}
			if (unsetCells.Count == 0)
			{
				break;
			}
			if (unsetCells.Count == count)
			{
				SelectedZone = MakeNewZone();
				base.Map.zoneManager.RegisterZone(SelectedZone);
				SelectedZone.AddCell(unsetCells[0]);
				unsetCells.RemoveAt(0);
			}
		}
		SelectedZone.CheckContiguous();
		Finalize(flag);
		if (SelectedZone is Zone_Stockpile zone_Stockpile)
		{
			zone_Stockpile.slotGroup.RemoveHaulDesignationOnStoredThings();
		}
		TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, unsetCells));
	}
}
