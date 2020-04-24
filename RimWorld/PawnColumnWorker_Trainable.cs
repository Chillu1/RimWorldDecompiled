using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class PawnColumnWorker_Trainable : PawnColumnWorker
	{
		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			MouseoverSounds.DoRegion(rect);
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.training != null)
			{
				bool visible;
				AcceptanceReport canTrain = pawn.training.CanAssignToTrain(def.trainable, out visible);
				if (visible && canTrain.Accepted)
				{
					int num = (int)((rect.width - 24f) / 2f);
					int num2 = Mathf.Max(3, 0);
					TrainingCardUtility.DoTrainableCheckbox(new Rect(rect.x + (float)num, rect.y + (float)num2, 24f, 24f), pawn, def.trainable, canTrain, drawLabel: false, doTooltip: true);
				}
			}
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
			if (pawn.training == null)
			{
				return int.MinValue;
			}
			if (pawn.training.HasLearned(def.trainable))
			{
				return 4;
			}
			bool visible;
			AcceptanceReport acceptanceReport = pawn.training.CanAssignToTrain(def.trainable, out visible);
			if (!visible)
			{
				return 0;
			}
			if (!acceptanceReport.Accepted)
			{
				return 1;
			}
			if (!pawn.training.GetWanted(def.trainable))
			{
				return 2;
			}
			return 3;
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
				if (pawnsListForReading[i].training == null || pawnsListForReading[i].training.HasLearned(def.trainable))
				{
					continue;
				}
				bool visible;
				AcceptanceReport acceptanceReport = pawnsListForReading[i].training.CanAssignToTrain(def.trainable, out visible);
				if (!visible || !acceptanceReport.Accepted)
				{
					continue;
				}
				bool wanted = pawnsListForReading[i].training.GetWanted(def.trainable);
				if (Event.current.button == 0)
				{
					if (!wanted)
					{
						pawnsListForReading[i].training.SetWantedRecursive(def.trainable, checkOn: true);
					}
				}
				else if (Event.current.button == 1 && wanted)
				{
					pawnsListForReading[i].training.SetWantedRecursive(def.trainable, checkOn: false);
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
			return base.GetHeaderTip(table) + "\n" + "CheckboxShiftClickTip".Translate();
		}
	}
}
