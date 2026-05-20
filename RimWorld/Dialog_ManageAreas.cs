using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_ManageAreas : Window
{
	private Map map;

	public override Vector2 InitialSize => new Vector2(550f, 400f);

	public Dialog_ManageAreas(Map map)
	{
		this.map = map;
		forcePause = true;
		doCloseX = true;
		doCloseButton = true;
		closeOnClickedOutside = true;
		absorbInputAroundWindow = true;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Listing_Standard listing_Standard = new Listing_Standard();
		listing_Standard.ColumnWidth = inRect.width;
		listing_Standard.Begin(inRect);
		List<Area> allAreas = map.areaManager.AllAreas;
		int i = 0;
		for (int j = 0; j < allAreas.Count; j++)
		{
			if (allAreas[j].Mutable)
			{
				Rect rect = listing_Standard.GetRect(24f);
				DoAreaRow(rect, allAreas[j], j);
				listing_Standard.Gap(6f);
				i++;
			}
		}
		if (map.areaManager.CanMakeNewAllowed())
		{
			for (; i < 9; i++)
			{
				listing_Standard.Gap(30f);
			}
			if (listing_Standard.ButtonText("NewArea".Translate()))
			{
				map.areaManager.TryMakeNewAllowed(out var _);
			}
		}
		listing_Standard.End();
	}

	private void DoAreaRow(Rect rect, Area area, int i)
	{
		if (Mouse.IsOver(rect))
		{
			area.MarkForDraw();
			GUI.color = area.Color;
			Widgets.DrawHighlight(rect);
			GUI.color = Color.white;
		}
		if (i % 2 == 1)
		{
			Widgets.DrawLightHighlight(rect);
		}
		Widgets.BeginGroup(rect);
		WidgetRow widgetRow = new WidgetRow(0f, 0f);
		Rect butRect = widgetRow.Icon(area.ColorTexture);
		if (area is Area_Allowed area2 && Widgets.ButtonInvisible(butRect))
		{
			Find.WindowStack.Add(new Dialog_AllowedAreaColorPicker(area2));
		}
		widgetRow.Gap(4f);
		using (new TextBlock(TextAnchor.LowerLeft))
		{
			widgetRow.LabelEllipses(area.Label, 160f);
		}
		if (widgetRow.ButtonText("ExpandArea".Translate(), null, drawBackground: true, doMouseoverSound: true, active: true, 60f))
		{
			SelectDesignator<Designator_AreaAllowedExpand>(area);
		}
		if (widgetRow.ButtonText("ShrinkArea".Translate(), null, drawBackground: true, doMouseoverSound: true, active: true, 60f))
		{
			SelectDesignator<Designator_AreaAllowedClear>(area);
		}
		if (widgetRow.ButtonText("InvertArea".Translate(), null, drawBackground: true, doMouseoverSound: true, active: true, 60f))
		{
			area.Invert();
		}
		if (widgetRow.ButtonIcon(TexButton.Rename, null, GenUI.SubtleMouseoverColor))
		{
			Find.WindowStack.Add(new Dialog_RenameArea(area));
		}
		if (widgetRow.ButtonIcon(TexButton.Copy, null, GenUI.SubtleMouseoverColor))
		{
			if (map.areaManager.TryMakeNewAllowed(out var area3))
			{
				foreach (IntVec3 activeCell in area.ActiveCells)
				{
					area3[activeCell] = true;
				}
			}
			else
			{
				Messages.Message("MaxAreasReached".Translate(10), MessageTypeDefOf.RejectInput);
			}
		}
		if (widgetRow.ButtonIcon(TexButton.Delete, null, GenUI.SubtleMouseoverColor))
		{
			area.Delete();
		}
		Widgets.EndGroup();
	}

	private void SelectDesignator<T>(Area area) where T : Designator_AreaAllowed
	{
		Find.MainTabsRoot.EscapeCurrentTab();
		foreach (Designator allResolvedDesignator in DesignationCategoryDefOf.Zone.AllResolvedDesignators)
		{
			if (allResolvedDesignator is T des)
			{
				Designator_AreaAllowed.selectedArea = area;
				Find.DesignatorManager.Select(des);
			}
		}
		Find.WindowStack.TryRemove(this);
	}
}
