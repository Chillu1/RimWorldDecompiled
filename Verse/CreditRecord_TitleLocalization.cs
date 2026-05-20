using UnityEngine;

namespace Verse
{
	public class CreditRecord_TitleLocalization : CreditsEntry
	{
		public string title;

		public CreditRecord_TitleLocalization()
		{
		}

		public CreditRecord_TitleLocalization(string title)
		{
			this.title = title;
		}

		public override float DrawHeight(float width)
		{
			return 40f;
		}

		public override void Draw(Rect rect)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, title);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Widgets.DrawLineHorizontal(rect.x + 10f, Mathf.Round(rect.yMax) - 14f, rect.width - 20f);
			GUI.color = Color.white;
		}
	}
}
