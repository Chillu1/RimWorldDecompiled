using System;
using UnityEngine;

namespace Verse
{
	public class ListableOption_WebLink : ListableOption
	{
		public Texture2D image;

		public string url;

		private static readonly Vector2 Imagesize = new Vector2(24f, 18f);

		public ListableOption_WebLink(string label, Texture2D image)
			: base(label, null)
		{
			minHeight = 24f;
			this.image = image;
		}

		public ListableOption_WebLink(string label, string url, Texture2D image)
			: this(label, image)
		{
			this.url = url;
		}

		public ListableOption_WebLink(string label, Action action, Texture2D image)
			: this(label, image)
		{
			base.action = action;
		}

		public override float DrawOption(Vector2 pos, float width)
		{
			float num = width - Imagesize.x - 3f;
			float num2 = Text.CalcHeight(label, num);
			float num3 = Mathf.Max(minHeight, num2);
			Rect rect = new Rect(pos.x, pos.y, width, num3);
			GUI.color = Color.white;
			if (image != null)
			{
				Rect position = new Rect(pos.x, pos.y + num3 / 2f - Imagesize.y / 2f, Imagesize.x, Imagesize.y);
				if (Mouse.IsOver(rect))
				{
					GUI.color = Widgets.MouseoverOptionColor;
				}
				GUI.DrawTexture(position, image);
			}
			Widgets.Label(new Rect(rect.xMax - num, pos.y, num, num2), label);
			GUI.color = Color.white;
			if (Widgets.ButtonInvisible(rect))
			{
				if (action != null)
				{
					action();
				}
				else
				{
					Application.OpenURL(url);
				}
			}
			return num3;
		}
	}
}
