using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class TutorUtility
	{
		public static bool BuildingOrBlueprintOrFrameCenterExists(IntVec3 c, Map map, ThingDef buildingDef)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (!(thing.Position != c))
				{
					if (thing.def == buildingDef)
					{
						return true;
					}
					if (thing.def.entityDefToBuild == buildingDef)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static CellRect FindUsableRect(int width, int height, Map map, float minFertility = 0f, bool noItems = false)
		{
			IntVec3 center = map.Center;
			float num = 1f;
			CellRect cellRect;
			while (true)
			{
				cellRect = CellRect.CenteredOn(center + new IntVec3((int)Rand.Range(0f - num, num), 0, (int)Rand.Range(0f - num, num)), width / 2);
				cellRect.Width = width;
				cellRect.Height = height;
				cellRect = cellRect.ExpandedBy(1);
				bool flag = true;
				foreach (IntVec3 item in cellRect)
				{
					if (item.Fogged(map) || !item.Walkable(map) || !item.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy) || item.GetTerrain(map).fertility < minFertility || item.GetZone(map) != null || ContainsBlockingThing(item, map, noItems) || item.InNoBuildEdgeArea(map) || item.InNoZoneEdgeArea(map))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				num += 0.25f;
			}
			return cellRect.ContractedBy(1);
		}

		private static bool ContainsBlockingThing(IntVec3 cell, Map map, bool noItems)
		{
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.category == ThingCategory.Building)
				{
					return true;
				}
				if (thingList[i] is Blueprint)
				{
					return true;
				}
				if (noItems && thingList[i].def.category == ThingCategory.Item)
				{
					return true;
				}
			}
			return false;
		}

		public static void DrawLabelOnThingOnGUI(Thing t, string label)
		{
			Vector2 vector = (t.DrawPos + new Vector3(0f, 0f, 0.5f)).MapToUIPosition();
			Vector2 vector2 = Text.CalcSize(label);
			Rect rect = new Rect(vector.x - vector2.x / 2f, vector.y - vector2.y / 2f, vector2.x, vector2.y);
			GUI.DrawTexture(rect, TexUI.GrayTextBG);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, label);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public static void DrawLabelOnGUI(Vector3 mapPos, string label)
		{
			Vector2 vector = mapPos.MapToUIPosition();
			Vector2 vector2 = Text.CalcSize(label);
			Rect rect = new Rect(vector.x - vector2.x / 2f, vector.y - vector2.y / 2f, vector2.x, vector2.y);
			GUI.DrawTexture(rect, TexUI.GrayTextBG);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, label);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public static void DrawCellRectOnGUI(CellRect cellRect, string label = null)
		{
			if (label != null)
			{
				DrawLabelOnGUI(cellRect.CenterVector3, label);
			}
		}

		public static void DrawCellRectUpdate(CellRect cellRect)
		{
			foreach (IntVec3 item in cellRect)
			{
				CellRenderer.RenderCell(item);
			}
		}

		public static void DoModalDialogIfNotKnown(ConceptDef conc, params string[] input)
		{
			if (!PlayerKnowledgeDatabase.IsComplete(conc))
			{
				DoModalDialogIfNotKnownInner(conc, string.Format(conc.HelpTextAdjusted, input));
			}
		}

		public static void DoModalDialogWithArgsIfNotKnown(ConceptDef conc, params NamedArgument[] args)
		{
			if (!PlayerKnowledgeDatabase.IsComplete(conc))
			{
				DoModalDialogIfNotKnownInner(conc, conc.HelpTextAdjusted.Formatted(args));
			}
		}

		public static void DoModalDialogWithArgsIfNotKnown(ConceptDef conc, string buttonAText, Action buttonAAction, string buttonBText = null, Action buttonBAction = null, params NamedArgument[] args)
		{
			if (!PlayerKnowledgeDatabase.IsComplete(conc))
			{
				Find.WindowStack.Add(new Dialog_MessageBox(conc.HelpTextAdjusted.Formatted(args), buttonAText, buttonAAction, buttonBText, buttonBAction));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(conc, KnowledgeAmount.Total);
			}
		}

		private static void DoModalDialogIfNotKnownInner(ConceptDef conc, string msg)
		{
			Find.WindowStack.Add(new Dialog_MessageBox(msg));
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(conc, KnowledgeAmount.Total);
		}

		public static bool EventCellsMatchExactly(EventPack ep, List<IntVec3> targetCells)
		{
			if (ep.Cell.IsValid)
			{
				if (targetCells.Count == 1)
				{
					return ep.Cell == targetCells[0];
				}
				return false;
			}
			if (ep.Cells == null)
			{
				return false;
			}
			int num = 0;
			foreach (IntVec3 cell in ep.Cells)
			{
				if (!targetCells.Contains(cell))
				{
					return false;
				}
				num++;
			}
			return num == targetCells.Count;
		}

		public static bool EventCellsAreWithin(EventPack ep, List<IntVec3> targetCells)
		{
			if (ep.Cell.IsValid)
			{
				return targetCells.Contains(ep.Cell);
			}
			if (ep.Cells != null)
			{
				return !ep.Cells.Any((IntVec3 c) => !targetCells.Contains(c));
			}
			return false;
		}
	}
}
