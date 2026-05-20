using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ChooseColonistsForIdeo : Window
	{
		private Ideo ideo;

		private List<Pawn> pawns = new List<Pawn>();

		private Func<Pawn, bool> canChangeIdeo;

		private Func<Pawn, Ideo> originalIdeo;

		private Func<Pawn, Ideo> pawnIdeoGetter;

		private Action<Pawn, Ideo> pawnIdeoSetter;

		private const float HeaderLabelHeight = 40f;

		private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

		private const float BottomAreaHeight = 55f;

		private const float RowButtonWidth = 150f;

		public override Vector2 InitialSize => new Vector2(500f, 600f);

		public Dialog_ChooseColonistsForIdeo(Ideo ideo, IEnumerable<Pawn> pawns, Func<Pawn, bool> canChangeIdeo, Func<Pawn, Ideo> originalIdeo, Func<Pawn, Ideo> pawnIdeoGetter = null, Action<Pawn, Ideo> pawnIdeoSetter = null)
		{
			forcePause = true;
			closeOnCancel = false;
			absorbInputAroundWindow = true;
			this.ideo = ideo;
			this.pawns.AddRange(pawns);
			this.canChangeIdeo = canChangeIdeo;
			this.originalIdeo = originalIdeo;
			this.pawnIdeoGetter = pawnIdeoGetter;
			this.pawnIdeoSetter = pawnIdeoSetter;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(new Rect(0f, 0f, inRect.width, 40f), "ChooseColonistsForIdeoTitle".Translate());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(new Rect(0f, 40f, inRect.width, 40f), "ChooseColonistsForIdeoDesc".Translate());
			inRect.yMin += 112f;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = inRect.width;
			listing_Standard.Begin(inRect);
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				Rect rect = listing_Standard.GetRect(24f);
				if (i % 2 == 0)
				{
					Widgets.DrawLightHighlight(rect);
				}
				Widgets.BeginGroup(rect);
				WidgetRow widgetRow = new WidgetRow(0f, 0f);
				RenderTexture tex = PortraitsCache.Get(pawn, new Vector2(24f, 24f), Rot4.South);
				widgetRow.Icon(tex);
				Ideo ideo = ((pawnIdeoGetter != null) ? pawnIdeoGetter(pawn) : pawn.Ideo);
				GUI.color = ideo.Color;
				widgetRow.Icon(ideo.Icon);
				GUI.color = Color.white;
				widgetRow.Label(pawn.LabelShortCap);
				float width = listing_Standard.ColumnWidth - widgetRow.FinalX - 150f;
				widgetRow.Gap(width);
				if (canChangeIdeo(pawn))
				{
					if (ideo == this.ideo)
					{
						if (widgetRow.ButtonText("RevertToPreviousIdeoligion".Translate()))
						{
							if (pawnIdeoSetter != null)
							{
								pawnIdeoSetter(pawn, originalIdeo(pawn));
							}
							else
							{
								pawn.ideo.SetIdeo(originalIdeo(pawn));
							}
						}
					}
					else if (widgetRow.ButtonText("ConvertToPlayerIdeoligion".Translate()))
					{
						if (pawnIdeoSetter != null)
						{
							pawnIdeoSetter(pawn, this.ideo);
						}
						else
						{
							pawn.ideo.SetIdeo(this.ideo);
						}
					}
				}
				else
				{
					widgetRow.Label("ExistingFollowerOfPlayerIdeoligion".Translate());
				}
				Widgets.EndGroup();
				listing_Standard.Gap(6f);
			}
			listing_Standard.End();
			if (Widgets.ButtonText(new Rect(inRect.width / 2f - BottomButtonSize.x / 2f, inRect.yMax - 55f, BottomButtonSize.x, BottomButtonSize.y), "Close".Translate()))
			{
				Close();
			}
		}
	}
}
