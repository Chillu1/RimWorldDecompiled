using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class PawnColumnWorker_AllowedArea : PawnColumnWorker
	{
		private const int TopAreaHeight = 65;

		private const int ManageAreasButtonHeight = 32;

		protected override GameFont DefaultHeaderFont => GameFont.Tiny;

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
			if (pawn.Faction == Faction.OfPlayer)
			{
				AreaAllowedGUI.DoAllowedAreaSelectors(rect, pawn);
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
			if (pawn.Faction != Faction.OfPlayer)
			{
				return int.MinValue;
			}
			return pawn.playerSettings.AreaRestriction?.ID ?? (-2147483647);
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
				if (pawnsListForReading[i].Faction != Faction.OfPlayer)
				{
					return;
				}
				if (Event.current.button == 0)
				{
					pawnsListForReading[i].playerSettings.AreaRestriction = Find.CurrentMap.areaManager.Home;
				}
				else if (Event.current.button == 1)
				{
					pawnsListForReading[i].playerSettings.AreaRestriction = null;
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
			return base.GetHeaderTip(table) + "\n" + "AllowedAreaShiftClickTip".Translate();
		}
	}
}
