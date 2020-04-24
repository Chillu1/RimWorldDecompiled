using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class PawnColumnWorker_Text : PawnColumnWorker
	{
		private static NumericStringComparer comparer = new NumericStringComparer();

		protected virtual int Width => def.width;

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			MouseoverSounds.DoRegion(rect);
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			Rect rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
			string textFor = GetTextFor(pawn);
			if (textFor == null)
			{
				return;
			}
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Widgets.Label(rect2, textFor);
			Text.WordWrap = true;
			Text.Anchor = TextAnchor.UpperLeft;
			if (Mouse.IsOver(rect2))
			{
				string tip = GetTip(pawn);
				if (!tip.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect2, tip);
				}
			}
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), Width);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return comparer.Compare(GetTextFor(a), GetTextFor(a));
		}

		protected abstract string GetTextFor(Pawn pawn);

		protected virtual string GetTip(Pawn pawn)
		{
			return null;
		}
	}
}
