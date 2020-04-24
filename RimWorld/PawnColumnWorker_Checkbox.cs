using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class PawnColumnWorker_Checkbox : PawnColumnWorker
	{
		public const int HorizontalPadding = 2;

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			MouseoverSounds.DoRegion(rect);
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (!HasCheckbox(pawn))
			{
				return;
			}
			int num = (int)((rect.width - 24f) / 2f);
			int num2 = Mathf.Max(3, 0);
			Vector2 topLeft = new Vector2(rect.x + (float)num, rect.y + (float)num2);
			Rect rect2 = new Rect(topLeft.x, topLeft.y, 24f, 24f);
			bool checkOn = GetValue(pawn);
			bool flag = checkOn;
			Widgets.Checkbox(topLeft, ref checkOn, 24f, disabled: false, def.paintable);
			if (Mouse.IsOver(rect2))
			{
				string tip = GetTip(pawn);
				if (!tip.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect2, tip);
				}
			}
			if (checkOn != flag)
			{
				SetValue(pawn, checkOn);
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 28);
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
			if (!HasCheckbox(pawn))
			{
				return 0;
			}
			if (!GetValue(pawn))
			{
				return 1;
			}
			return 2;
		}

		protected virtual string GetTip(Pawn pawn)
		{
			return null;
		}

		protected virtual bool HasCheckbox(Pawn pawn)
		{
			return true;
		}

		protected abstract bool GetValue(Pawn pawn);

		protected abstract void SetValue(Pawn pawn, bool value);

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
				if (!HasCheckbox(pawnsListForReading[i]))
				{
					continue;
				}
				if (Event.current.button == 0)
				{
					if (!GetValue(pawnsListForReading[i]))
					{
						SetValue(pawnsListForReading[i], value: true);
					}
				}
				else if (Event.current.button == 1 && GetValue(pawnsListForReading[i]))
				{
					SetValue(pawnsListForReading[i], value: false);
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
