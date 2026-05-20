using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class PawnColumnWorker_Trainable_Special : PawnColumnWorker
{
	public override void DoHeader(Rect rect, PawnTable table)
	{
		base.DoHeader(rect, table);
		MouseoverSounds.DoRegion(rect);
	}

	public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
	{
		if (pawn.training != null && !pawn.RaceProps.specialTrainables.NullOrEmpty())
		{
			int num = (int)((rect.width - 24f) / 2f);
			int num2 = Mathf.Max(3, 0);
			Rect rect2 = new Rect(rect.x + (float)num, rect.y + (float)num2, 24f, 24f);
			DoSpecialTrainableCheckbox(rect2, pawn, doTooltip: true);
		}
	}

	private void DoSpecialTrainableCheckbox(Rect rect, Pawn pawn, bool doTooltip)
	{
		GetStatus(pawn, out var learned, out var checkOn, out var canTrain, out var _);
		bool flag = checkOn;
		Texture2D texChecked = (learned ? TrainingCardUtility.LearnedTrainingTex : null);
		Texture2D texUnchecked = (learned ? TrainingCardUtility.LearnedNotTrainingTex : null);
		Widgets.Checkbox(rect.position, ref checkOn, rect.width, !canTrain, paintable: true, texChecked, texUnchecked);
		if (checkOn != flag)
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AnimalTraining, KnowledgeAmount.Total);
			foreach (TrainableDef specialTrainable in pawn.RaceProps.specialTrainables)
			{
				pawn.training.SetWantedRecursive(specialTrainable, checkOn);
			}
		}
		if (doTooltip)
		{
			DoSpecialTrainableTooltip(rect, pawn);
		}
	}

	private void DoSpecialTrainableTooltip(Rect rect, Pawn pawn)
	{
		if (!Mouse.IsOver(rect))
		{
			return;
		}
		TooltipHandler.TipRegion(rect, delegate
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (TrainableDef specialTrainable in pawn.RaceProps.specialTrainables)
			{
				bool visible;
				AcceptanceReport acceptanceReport = pawn.training.CanAssignToTrain(specialTrainable, out visible);
				stringBuilder.AppendLineIfNotEmpty();
				stringBuilder.AppendLine(specialTrainable.LabelCap + "\n\n" + specialTrainable.description);
				if (!acceptanceReport.Accepted)
				{
					stringBuilder.AppendLine().AppendLine(acceptanceReport.Reason);
				}
				else if (!specialTrainable.prerequisites.NullOrEmpty())
				{
					stringBuilder.AppendLine();
					foreach (TrainableDef prerequisite in specialTrainable.prerequisites)
					{
						if (!pawn.training.HasLearned(prerequisite))
						{
							stringBuilder.AppendLine("TrainingNeedsPrerequisite".Translate(prerequisite.LabelCap));
						}
					}
				}
			}
			return stringBuilder.ToString();
		}, (int)(rect.y * 511f + rect.x));
	}

	public override int GetMinWidth(PawnTable table)
	{
		return Mathf.Max(base.GetMinWidth(table), 24);
	}

	public override int GetMaxWidth(PawnTable table)
	{
		return Mathf.Min(base.GetMaxWidth(table), GetMinWidth(table));
	}

	public override int GetMinCellHeight(Pawn pawn)
	{
		return Mathf.Max(base.GetMinCellHeight(pawn), 24);
	}

	public override int Compare(Pawn a, Pawn b)
	{
		return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
	}

	private int GetValueToCompare(Pawn pawn)
	{
		if (pawn.training == null || pawn.RaceProps.specialTrainables.NullOrEmpty())
		{
			return int.MinValue;
		}
		GetStatus(pawn, out var learned, out var checkOn, out var canTrain, out var visible);
		if (learned)
		{
			return 4;
		}
		if (!visible)
		{
			return 0;
		}
		if (!canTrain)
		{
			return 1;
		}
		if (!checkOn)
		{
			return 2;
		}
		return 3;
	}

	private static void GetStatus(Pawn pawn, out bool learned, out bool checkOn, out bool canTrain, out bool visible)
	{
		learned = true;
		checkOn = true;
		canTrain = true;
		visible = false;
		foreach (TrainableDef specialTrainable in pawn.RaceProps.specialTrainables)
		{
			if (!pawn.training.HasLearned(specialTrainable))
			{
				learned = false;
			}
			if (!pawn.training.GetWanted(specialTrainable))
			{
				checkOn = false;
			}
			if (!pawn.training.CanAssignToTrain(specialTrainable, out var visible2))
			{
				canTrain = false;
			}
			if (visible2)
			{
				visible = true;
			}
		}
	}
}
