using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Listing_ScenEdit : Listing_Standard
	{
		private Scenario scen;

		public Listing_ScenEdit(Scenario scen)
		{
			this.scen = scen;
		}

		public Rect GetScenPartRect(ScenPart part, float height)
		{
			string label = part.Label;
			Rect rect = GetRect(height);
			Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, 0.08f));
			WidgetRow widgetRow = new WidgetRow(rect.x, rect.y, UIDirection.RightThenDown, 72f, 0f);
			if (part.def.PlayerAddRemovable && widgetRow.ButtonIcon(TexButton.DeleteX, null, GenUI.SubtleMouseoverColor))
			{
				scen.RemovePart(part);
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			if (scen.CanReorder(part, ReorderDirection.Up) && widgetRow.ButtonIcon(TexButton.ReorderUp))
			{
				scen.Reorder(part, ReorderDirection.Up);
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			if (scen.CanReorder(part, ReorderDirection.Down) && widgetRow.ButtonIcon(TexButton.ReorderDown))
			{
				scen.Reorder(part, ReorderDirection.Down);
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
			}
			Text.Anchor = TextAnchor.UpperRight;
			Rect rect2 = rect.LeftPart(0.5f).Rounded();
			rect2.xMax -= 4f;
			Widgets.Label(rect2, label);
			Text.Anchor = TextAnchor.UpperLeft;
			Gap(4f);
			return rect.RightPart(0.5f).Rounded();
		}
	}
}
