using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_WorkMode : PawnColumnWorker
	{
		private const int Width = 160;

		private const int LeftMargin = 3;

		private static List<FloatMenuOption> tmpFloatMenuOptions = new List<FloatMenuOption>();

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			MechanitorControlGroup mechControlGroup = pawn.GetMechControlGroup();
			if (mechControlGroup == null)
			{
				return;
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Rect rect2 = rect;
			rect2.xMin += 3f;
			Widgets.Label(rect2, mechControlGroup.WorkMode.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			if (Mouse.IsOver(rect))
			{
				AcceptanceReport canControlMechs = mechControlGroup.Tracker.CanControlMechs;
				TipSignal tooltip = pawn.GetTooltip();
				tooltip.text = "ClickToChangeWorkMode".Translate();
				if (!canControlMechs && !canControlMechs.Reason.NullOrEmpty())
				{
					ref string text = ref tooltip.text;
					text = text + "\n\n" + ("DisabledCommand".Translate() + ": " + canControlMechs.Reason).Colorize(ColorLibrary.RedReadable);
				}
				TooltipHandler.TipRegion(rect, tooltip);
				if ((bool)canControlMechs && Widgets.ButtonInvisible(rect))
				{
					tmpFloatMenuOptions.Clear();
					tmpFloatMenuOptions.AddRange(MechanitorControlGroupGizmo.GetWorkModeOptions(mechControlGroup));
					Find.WindowStack.Add(new FloatMenu(tmpFloatMenuOptions));
					tmpFloatMenuOptions.Clear();
				}
				Widgets.DrawHighlight(rect);
			}
		}

		public override bool CanGroupWith(Pawn pawn, Pawn other)
		{
			MechanitorControlGroup mechControlGroup = pawn.GetMechControlGroup();
			if (mechControlGroup != null)
			{
				return other.GetMechControlGroup() == mechControlGroup;
			}
			return false;
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 160);
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return Mathf.Min(base.GetMaxWidth(table), GetMinWidth(table));
		}
	}
}
