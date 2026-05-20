using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse;

public static class AreaUtility
{
	public static void MakeAllowedAreaListFloatMenu(Action<Area> selAction, bool addNullAreaOption, bool addManageOption, Map map)
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		if (addNullAreaOption)
		{
			list.Add(new FloatMenuOption("NoAreaAllowed".Translate(), delegate
			{
				selAction(null);
			}, MenuOptionPriority.High));
		}
		foreach (Area item2 in map.areaManager.AllAreas.Where((Area a) => a.AssignableAsAllowed()))
		{
			Area localArea = item2;
			FloatMenuOption item = new FloatMenuOption(localArea.Label, delegate
			{
				selAction(localArea);
			}, MenuOptionPriority.Default, delegate
			{
				localArea.MarkForDraw();
			});
			list.Add(item);
		}
		if (addManageOption)
		{
			list.Add(new FloatMenuOption("ManageAreas".Translate(), delegate
			{
				Find.WindowStack.Add(new Dialog_ManageAreas(map));
			}, MenuOptionPriority.Low));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public static string AreaAllowedLabel(Pawn pawn)
	{
		if (pawn.playerSettings != null)
		{
			return AreaAllowedLabel_Area(pawn.playerSettings.AreaRestrictionInPawnCurrentMap);
		}
		return AreaAllowedLabel_Area(null);
	}

	public static string AreaAllowedLabel_Area(Area area)
	{
		if (area != null)
		{
			return area.Label;
		}
		return "NoAreaAllowed".Translate();
	}
}
