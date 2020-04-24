using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class WidgetRow
	{
		private float startX;

		private float curX;

		private float curY;

		private float maxWidth = 99999f;

		private float gap;

		private UIDirection growDirection = UIDirection.RightThenUp;

		public const float IconSize = 24f;

		public const float DefaultGap = 4f;

		private const float DefaultMaxWidth = 99999f;

		public const float LabelGap = 2f;

		public const float ButtonExtraSpace = 16f;

		public float FinalX => curX;

		public float FinalY => curY;

		public WidgetRow()
		{
		}

		public WidgetRow(float x, float y, UIDirection growDirection = UIDirection.RightThenUp, float maxWidth = 99999f, float gap = 4f)
		{
			Init(x, y, growDirection, maxWidth, gap);
		}

		public void Init(float x, float y, UIDirection growDirection = UIDirection.RightThenUp, float maxWidth = 99999f, float gap = 4f)
		{
			this.growDirection = growDirection;
			startX = x;
			curX = x;
			curY = y;
			this.maxWidth = maxWidth;
			this.gap = gap;
		}

		private float LeftX(float elementWidth)
		{
			if (growDirection == UIDirection.RightThenUp || growDirection == UIDirection.RightThenDown)
			{
				return curX;
			}
			return curX - elementWidth;
		}

		private void IncrementPosition(float amount)
		{
			if (growDirection == UIDirection.RightThenUp || growDirection == UIDirection.RightThenDown)
			{
				curX += amount;
			}
			else
			{
				curX -= amount;
			}
			if (Mathf.Abs(curX - startX) > maxWidth)
			{
				IncrementY();
			}
		}

		private void IncrementY()
		{
			if (growDirection == UIDirection.RightThenUp || growDirection == UIDirection.LeftThenUp)
			{
				curY -= 24f + gap;
			}
			else
			{
				curY += 24f + gap;
			}
			curX = startX;
		}

		private void IncrementYIfWillExceedMaxWidth(float width)
		{
			if (Mathf.Abs(curX - startX) + Mathf.Abs(width) > maxWidth)
			{
				IncrementY();
			}
		}

		public void Gap(float width)
		{
			if (curX != startX)
			{
				IncrementPosition(width);
			}
		}

		public bool ButtonIcon(Texture2D tex, string tooltip = null, Color? mouseoverColor = null, bool doMouseoverSound = true)
		{
			IncrementYIfWillExceedMaxWidth(24f);
			Rect rect = new Rect(LeftX(24f), curY, 24f, 24f);
			if (doMouseoverSound)
			{
				MouseoverSounds.DoRegion(rect);
			}
			bool result = Widgets.ButtonImage(rect, tex, Color.white, mouseoverColor ?? GenUI.MouseoverColor);
			IncrementPosition(24f + gap);
			if (!tooltip.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, tooltip);
			}
			return result;
		}

		public void GapButtonIcon()
		{
			if (curY != startX)
			{
				IncrementPosition(24f + gap);
			}
		}

		public void ToggleableIcon(ref bool toggleable, Texture2D tex, string tooltip, SoundDef mouseoverSound = null, string tutorTag = null)
		{
			IncrementYIfWillExceedMaxWidth(24f);
			Rect rect = new Rect(LeftX(24f), curY, 24f, 24f);
			bool num = Widgets.ButtonImage(rect, tex);
			IncrementPosition(24f + gap);
			if (!tooltip.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, tooltip);
			}
			Rect position = new Rect(rect.x + rect.width / 2f, rect.y, rect.height / 2f, rect.height / 2f);
			Texture2D image = toggleable ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
			GUI.DrawTexture(position, image);
			if (mouseoverSound != null)
			{
				MouseoverSounds.DoRegion(rect, mouseoverSound);
			}
			if (num)
			{
				toggleable = !toggleable;
				if (toggleable)
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
			}
			if (tutorTag != null)
			{
				UIHighlighter.HighlightOpportunity(rect, tutorTag);
			}
		}

		public Rect Icon(Texture2D tex, string tooltip = null)
		{
			IncrementYIfWillExceedMaxWidth(24f);
			Rect rect = new Rect(LeftX(24f), curY, 24f, 24f);
			GUI.DrawTexture(rect, tex);
			if (!tooltip.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, tooltip);
			}
			IncrementPosition(24f + gap);
			return rect;
		}

		public bool ButtonText(string label, string tooltip = null, bool drawBackground = true, bool doMouseoverSound = true)
		{
			Vector2 vector = Text.CalcSize(label);
			vector.x += 16f;
			vector.y += 2f;
			IncrementYIfWillExceedMaxWidth(vector.x);
			Rect rect = new Rect(LeftX(vector.x), curY, vector.x, vector.y);
			bool result = Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound);
			if (!tooltip.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, tooltip);
			}
			IncrementPosition(rect.width + gap);
			return result;
		}

		public Rect Label(string text, float width = -1f)
		{
			if (width < 0f)
			{
				width = Text.CalcSize(text).x;
			}
			IncrementYIfWillExceedMaxWidth(width);
			Rect rect = new Rect(LeftX(width), curY, width, 24f);
			IncrementPosition(2f);
			Widgets.Label(rect, text);
			IncrementPosition(2f);
			IncrementPosition(rect.width);
			return rect;
		}

		public Rect FillableBar(float width, float height, float fillPct, string label, Texture2D fillTex, Texture2D bgTex = null)
		{
			IncrementYIfWillExceedMaxWidth(width);
			Rect rect = new Rect(LeftX(width), curY, width, height);
			Widgets.FillableBar(rect, fillPct, fillTex, bgTex, doBorder: false);
			if (!label.NullOrEmpty())
			{
				Rect rect2 = rect;
				rect2.xMin += 2f;
				rect2.xMax -= 2f;
				if (Text.Anchor >= TextAnchor.UpperLeft)
				{
					rect2.height += 14f;
				}
				Text.Font = GameFont.Tiny;
				Text.WordWrap = false;
				Widgets.Label(rect2, label);
				Text.WordWrap = true;
			}
			IncrementPosition(width);
			return rect;
		}
	}
}
