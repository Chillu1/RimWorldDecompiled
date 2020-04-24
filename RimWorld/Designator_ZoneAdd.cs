using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Designator_ZoneAdd : Designator_Zone
	{
		protected Type zoneTypeToPlace;

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

		protected abstract string NewZoneLabel
		{
			get;
		}

		protected abstract Zone MakeNewZone();

		public Designator_ZoneAdd()
		{
			soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			soundDragChanged = null;
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
			if (c.Fogged(base.Map))
			{
				return false;
			}
			if (c.InNoZoneEdgeArea(base.Map))
			{
				return "TooCloseToMapEdge".Translate();
			}
			Zone zone = base.Map.zoneManager.ZoneAt(c);
			if (zone != null && zone.GetType() != zoneTypeToPlace)
			{
				return false;
			}
			foreach (Thing item in base.Map.thingGrid.ThingsAt(c))
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
			List<IntVec3> list = cells.ToList();
			bool flag = false;
			if (list.Count == 1)
			{
				Zone zone = base.Map.zoneManager.ZoneAt(list[0]);
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
			list.RemoveAll((IntVec3 c) => base.Map.zoneManager.ZoneAt(c) != null);
			if (list.Count == 0 || (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, list))))
			{
				return;
			}
			if (SelectedZone == null)
			{
				SelectedZone = MakeNewZone();
				base.Map.zoneManager.RegisterZone(SelectedZone);
				SelectedZone.AddCell(list[0]);
				list.RemoveAt(0);
			}
			while (true)
			{
				flag = true;
				int count = list.Count;
				for (int num = list.Count - 1; num >= 0; num--)
				{
					bool flag2 = false;
					for (int i = 0; i < 4; i++)
					{
						IntVec3 c2 = list[num] + GenAdj.CardinalDirections[i];
						if (c2.InBounds(base.Map) && base.Map.zoneManager.ZoneAt(c2) == SelectedZone)
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						SelectedZone.AddCell(list[num]);
						list.RemoveAt(num);
					}
				}
				if (list.Count == 0)
				{
					break;
				}
				if (list.Count == count)
				{
					SelectedZone = MakeNewZone();
					base.Map.zoneManager.RegisterZone(SelectedZone);
					SelectedZone.AddCell(list[0]);
					list.RemoveAt(0);
				}
			}
			SelectedZone.CheckContiguous();
			Finalize(flag);
			TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, list));
		}
	}
}
