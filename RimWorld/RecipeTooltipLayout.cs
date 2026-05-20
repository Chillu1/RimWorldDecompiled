using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RecipeTooltipLayout
	{
		private Vector2 curPos;

		private Vector2 maxSize;

		private float indent;

		public Vector2 Size => maxSize;

		public bool Empty
		{
			get
			{
				if (!(Size.x <= 0f))
				{
					return Size.y <= 0f;
				}
				return true;
			}
		}

		public void Label(string text, bool draw, GameFont font = GameFont.Small)
		{
			GameFont font2 = Text.Font;
			Text.Font = font;
			Vector2 vector = Text.CalcSize(text);
			Rect rect = new Rect(curPos.x, curPos.y, vector.x, vector.y);
			if (draw)
			{
				Widgets.Label(rect, text);
			}
			curPos.x = rect.xMax;
			ExpandToFit(rect.xMax, rect.yMax);
			Text.Font = font2;
		}

		public void Icon(Texture2D icon, Color color, float iconSize, bool draw)
		{
			Rect position = new Rect(curPos.x, curPos.y, iconSize, iconSize);
			if (draw)
			{
				Color color2 = GUI.color;
				GUI.color = color;
				GUI.DrawTexture(position, icon);
				GUI.color = color2;
			}
			curPos.x = position.xMax;
			ExpandToFit(position.xMax, position.yMax);
		}

		public void Gap(float x, float y)
		{
			curPos.x += x;
			curPos.y += y;
			ExpandToFit(curPos.x, curPos.y);
		}

		public void Reset(float newIndent = 0f)
		{
			curPos = Vector2.zero;
			maxSize = Vector2.zero;
			indent = newIndent;
			curPos.x = Mathf.Max(curPos.x, indent);
			curPos.y = Mathf.Max(curPos.y, indent);
		}

		public void Expand(float width, float height)
		{
			maxSize.x += width;
			maxSize.y += height;
		}

		public void Newline(GameFont font = GameFont.Small)
		{
			curPos.x = indent;
			curPos.y += Text.LineHeightOf(font);
			ExpandToFit(curPos.x, curPos.y);
		}

		private void ExpandToFit(float x, float y)
		{
			maxSize.x = Mathf.Max(maxSize.x, x);
			maxSize.y = Mathf.Max(maxSize.y, y);
		}
	}
}
