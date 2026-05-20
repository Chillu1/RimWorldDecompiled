using UnityEngine;
using Verse;

namespace RimWorld;

public static class RenameUIUtility
{
	public static void DrawRenameButton(Rect rect, Pawn pawn)
	{
		TooltipHandler.TipRegionByKey(rect, pawn.RaceProps.IsMechanoid ? "RenameMech" : "RenameAnimal");
		if (Widgets.ButtonImage(rect, TexButton.Rename))
		{
			Find.WindowStack.Add(pawn.NamePawnDialog());
		}
	}

	public static void DrawRenameButton(Rect rect, IRenameable renamable)
	{
		TooltipHandler.TipRegionByKey(rect, "Rename");
		if (Widgets.ButtonImage(rect, TexButton.Rename))
		{
			Find.WindowStack.Add(new Dialog_RenameBuildingStorage(renamable));
		}
	}

	public static void DrawRenameButton(Rect rect, IStorageGroupMember building)
	{
		TooltipHandler.TipRegionByKey(rect, "Rename");
		if (Widgets.ButtonImage(rect, TexButton.Rename))
		{
			Find.WindowStack.Add(new Dialog_RenameBuildingStorage_CreateNew(building));
		}
	}

	public static void DrawRenameButton(Rect rect, CompAnimalPenMarker marker)
	{
		TooltipHandler.TipRegionByKey(rect, "Rename");
		if (Widgets.ButtonImage(rect, TexButton.Rename))
		{
			Find.WindowStack.Add(new Dialog_RenameAnimalPen(marker.parent.Map, marker));
		}
	}

	public static void DrawRenameButton(Rect rect, Building_GravEngine engine)
	{
		TooltipHandler.TipRegionByKey(rect, "Rename");
		if (Widgets.ButtonImage(rect, TexButton.Rename))
		{
			Find.WindowStack.Add(new Dialog_RenameGravship(engine));
		}
	}
}
