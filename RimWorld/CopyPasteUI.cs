using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class CopyPasteUI
	{
		public const float CopyPasteIconHeight = 24f;

		public const float CopyPasteIconWidth = 18f;

		public const float CopyPasteColumnWidth = 36f;

		public static void DoCopyPasteButtons(Rect rect, Action copyAction, Action pasteAction)
		{
			MouseoverSounds.DoRegion(rect);
			Rect rect2 = new Rect(rect.x, rect.y + (rect.height / 2f - 12f), 18f, 24f);
			if (Widgets.ButtonImage(rect2, TexButton.Copy))
			{
				copyAction();
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
			TooltipHandler.TipRegionByKey(rect2, "Copy");
			if (pasteAction != null)
			{
				Rect rect3 = rect2;
				rect3.x = rect2.xMax;
				if (Widgets.ButtonImage(rect3, TexButton.Paste))
				{
					pasteAction();
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegionByKey(rect3, "Paste");
			}
		}
	}
}
