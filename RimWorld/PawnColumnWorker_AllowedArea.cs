using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld;

public class PawnColumnWorker_AllowedArea : PawnColumnWorker
{
	private const int TopAreaHeight = 65;

	private const int ManageAreasButtonHeight = 32;

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), 200);
	}

	public override int GetOptimalWidth(PawnTable table)
	{
		return Mathf.Clamp(273, GetMinWidth(table), GetMaxWidth(table));
	}

	public override int GetMinHeaderHeight(PawnTable table)
	{
		return Mathf.Max(base.GetMinHeaderHeight(table), 65);
	}

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.Faction == Faction.OfPlayer && (!pawn.IsMutant || pawn.mutant.Def.respectsAllowedArea) && (!pawn.RaceProps.IsMechanoid || pawn.GetOverseer() != null))
		{
			if (pawn.playerSettings.SupportsAllowedAreas)
			{
				AreaAllowedGUI.DoAllowedAreaSelectors(rect, pawn);
			}
			else if (AnimalPenUtility.NeedsToBeManagedByRope(pawn))
			{
				AnimalPenGUI.DoAllowedAreaMessage(rect, pawn);
			}
			else if (pawn.RaceProps.Dryad)
			{
				Text.Anchor = TextAnchor.MiddleCenter;
				Text.Font = GameFont.Tiny;
				GUI.color = Color.gray;
				Widgets.Label(rect, "CannotAssignAllowedAreaToDryad".Translate());
				GUI.color = Color.white;
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}
	}

	public override void DoHeader(Rect rect, PawnTable table)
	{
		base.DoHeader(rect, table);
		if (Widgets.ButtonText(new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f), "ManageAreas".Translate()))
		{
			Find.WindowStack.Add(new Dialog_ManageAreas(Find.CurrentMap));
		}
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
	}

	private int GetValueToCompare(Pawn pawn)
	{
		if (pawn.Faction != Faction.OfPlayer || !pawn.playerSettings.SupportsAllowedAreas)
		{
			return int.MinValue;
		}
		return pawn.playerSettings.AreaRestrictionInPawnCurrentMap?.ID ?? (-2147483647);
	}

	protected override void HeaderClicked(Rect headerRect, PawnTable table)
	{
		base.HeaderClicked(headerRect, table);
		if (!Event.current.shift || Find.CurrentMap == null)
		{
			return;
		}
		List<Pawn> pawnsListForReading = table.PawnsListForReading;
		for (int i = 0; i < pawnsListForReading.Count; i++)
		{
			if (pawnsListForReading[i].Faction == Faction.OfPlayer && pawnsListForReading[i].playerSettings.SupportsAllowedAreas)
			{
				if (Event.current.button == 0)
				{
					pawnsListForReading[i].playerSettings.AreaRestrictionInPawnCurrentMap = Find.CurrentMap.areaManager.Home;
				}
				else if (Event.current.button == 1)
				{
					pawnsListForReading[i].playerSettings.AreaRestrictionInPawnCurrentMap = null;
				}
			}
		}
		if (Event.current.button == 0)
		{
			SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
		}
		else if (Event.current.button == 1)
		{
			SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
		}
	}

	protected override string GetHeaderTip(PawnTable table)
	{
		string text = base.GetHeaderTip(table);
		if (!SteamDeck.IsSteamDeckInNonKeyboardMode)
		{
			text += "\n" + "AllowedAreaShiftClickTip".Translate();
		}
		return text;
	}
}
