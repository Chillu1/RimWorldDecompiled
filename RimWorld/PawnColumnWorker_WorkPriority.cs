using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld;

public class PawnColumnWorker_WorkPriority : PawnColumnWorker
{
	private const int LabelRowHeight = 50;

	private Vector2 cachedWorkLabelSize;

	public override bool VisibleCurrently => def.workType.VisibleCurrently;

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.Dead || pawn.workSettings == null || !pawn.workSettings.EverWork)
		{
			return;
		}
		Text.Font = GameFont.Medium;
		float x = rect.x + (rect.width - 25f) / 2f;
		float y = rect.y + 2.5f;
		bool incapable = IsIncapableOfWholeWorkType(pawn, def.workType);
		WidgetsWork.DrawWorkBoxFor(x, y, pawn, def.workType, incapable);
		Rect rect2 = new Rect(x, y, 25f, 25f);
		if (Mouse.IsOver(rect2))
		{
			TooltipHandler.TipRegion(rect2, () => WidgetsWork.TipForPawnWorker(pawn, def.workType, incapable), pawn.thingIDNumber ^ def.workType.GetHashCode());
		}
		Text.Font = GameFont.Small;
	}

	public override void DoHeader(Rect rect, PawnTable table)
	{
		base.DoHeader(rect, table);
		Text.Font = GameFont.Small;
		if (cachedWorkLabelSize == default(Vector2))
		{
			cachedWorkLabelSize = Text.CalcSize(def.workType.labelShort.CapitalizeFirst());
		}
		Rect labelRect = GetLabelRect(rect);
		MouseoverSounds.DoRegion(labelRect);
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(labelRect, def.workType.labelShort.CapitalizeFirst());
		GUI.color = new Color(1f, 1f, 1f, 0.3f);
		Widgets.DrawLineVertical(labelRect.center.x, labelRect.yMax - 3f, rect.y + 50f - labelRect.yMax + 3f);
		Widgets.DrawLineVertical(labelRect.center.x + 1f, labelRect.yMax - 3f, rect.y + 50f - labelRect.yMax + 3f);
		GUI.color = Color.white;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public override int GetMinHeaderHeight(PawnTable table)
	{
		return 50;
	}

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), 32);
	}

	public override int GetOptimalWidth(PawnTable table)
	{
		return Mathf.Clamp(39, GetMinWidth(table), GetMaxWidth(table));
	}

	public override int GetMaxWidth(PawnTable table)
	{
		return Mathf.Min(base.GetMaxWidth(table), 80);
	}

	private bool IsIncapableOfWholeWorkType(Pawn p, WorkTypeDef work)
	{
		for (int i = 0; i < work.workGiversByPriority.Count; i++)
		{
			bool flag = true;
			for (int j = 0; j < work.workGiversByPriority[i].requiredCapacities.Count; j++)
			{
				PawnCapacityDef capacity = work.workGiversByPriority[i].requiredCapacities[j];
				if (!p.health.capacities.CapableOf(capacity))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return false;
			}
		}
		return true;
	}

	protected override Rect GetInteractableHeaderRect(Rect headerRect, PawnTable table)
	{
		return GetLabelRect(headerRect);
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
	}

	private float GetValueToCompare(Pawn pawn)
	{
		if (pawn.workSettings == null || !pawn.workSettings.EverWork)
		{
			return -2f;
		}
		if (pawn.WorkTypeIsDisabled(def.workType))
		{
			return -1f;
		}
		return pawn.skills.AverageOfRelevantSkillsFor(def.workType);
	}

	private Rect GetLabelRect(Rect headerRect)
	{
		float x = headerRect.center.x;
		Rect result = new Rect(x - cachedWorkLabelSize.x / 2f, headerRect.y, cachedWorkLabelSize.x, cachedWorkLabelSize.y);
		if (def.moveWorkTypeLabelDown)
		{
			result.y += 20f;
		}
		return result;
	}

	protected override string GetHeaderTip(PawnTable table)
	{
		TaggedString taggedString = def.workType.gerundLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + def.workType.description + "\n\n" + SpecificWorkListString(def.workType);
		taggedString += "\n";
		if (def.sortable)
		{
			taggedString += "\n" + "ClickToSortByThisColumn".Translate().Colorize(ColoredText.SubtleGrayColor);
		}
		if (!SteamDeck.IsSteamDeckInNonKeyboardMode)
		{
			if (Find.PlaySettings.useWorkPriorities)
			{
				taggedString += "\n" + "WorkPriorityShiftClickTip".Translate().Colorize(ColoredText.SubtleGrayColor);
			}
			else
			{
				taggedString += "\n" + "WorkPriorityShiftClickEnableDisableTip".Translate().Colorize(ColoredText.SubtleGrayColor);
			}
		}
		return taggedString.Resolve();
	}

	private static string SpecificWorkListString(WorkTypeDef def)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < def.workGiversByPriority.Count; i++)
		{
			stringBuilder.Append(" - " + def.workGiversByPriority[i].LabelCap);
			if (def.workGiversByPriority[i].emergency)
			{
				stringBuilder.Append(" (" + "EmergencyWorkMarker".Translate() + ")");
			}
			if (i < def.workGiversByPriority.Count - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}

	protected override void HeaderClicked(Rect headerRect, PawnTable table)
	{
		base.HeaderClicked(headerRect, table);
		if (!Event.current.shift)
		{
			return;
		}
		List<Pawn> pawnsListForReading = table.PawnsListForReading;
		for (int i = 0; i < pawnsListForReading.Count; i++)
		{
			Pawn pawn = pawnsListForReading[i];
			if (pawn.workSettings == null || !pawn.workSettings.EverWork || pawn.WorkTypeIsDisabled(def.workType))
			{
				continue;
			}
			if (Find.PlaySettings.useWorkPriorities)
			{
				int priority = pawn.workSettings.GetPriority(def.workType);
				if (Event.current.button == 0 && priority != 1)
				{
					int num = priority - 1;
					if (num < 0)
					{
						num = 4;
					}
					pawn.workSettings.SetPriority(def.workType, num);
				}
				if (Event.current.button == 1 && priority != 0)
				{
					int num2 = priority + 1;
					if (num2 > 4)
					{
						num2 = 0;
					}
					pawn.workSettings.SetPriority(def.workType, num2);
				}
			}
			else if (pawn.workSettings.GetPriority(def.workType) > 0)
			{
				if (Event.current.button == 1)
				{
					pawn.workSettings.SetPriority(def.workType, 0);
				}
			}
			else if (Event.current.button == 0)
			{
				pawn.workSettings.SetPriority(def.workType, 3);
			}
		}
		if (Find.PlaySettings.useWorkPriorities)
		{
			SoundDefOf.DragSlider.PlayOneShotOnCamera();
		}
		else if (Event.current.button == 0)
		{
			SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
		}
		else if (Event.current.button == 1)
		{
			SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
		}
	}
}
