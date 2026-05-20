using UnityEngine;

namespace Verse
{
	public class CreditRecord_RoleTwoCols : CreditsEntry
	{
		public string creditee1;

		public string creditee2;

		public string extra;

		public bool compressed;

		public CreditRecord_RoleTwoCols()
		{
		}

		public CreditRecord_RoleTwoCols(string creditee1, string creditee2, string extra = null)
		{
			this.creditee1 = creditee1;
			this.creditee2 = creditee2;
			this.extra = extra;
		}

		public override float DrawHeight(float width)
		{
			float a = Text.CalcHeight(creditee1, width * 0.5f);
			float b = Text.CalcHeight(creditee2, width * 0.5f);
			if (!compressed)
			{
				return 50f;
			}
			return Mathf.Max(a, b);
		}

		public override void Draw(Rect rect)
		{
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect2 = rect;
			rect2.width = 0f;
			rect2.width = rect.width / 2f;
			Widgets.Label(rect2, creditee1);
			Rect rect3 = rect;
			rect3.xMin = rect2.xMax;
			Widgets.Label(rect3, creditee2);
			if (!extra.NullOrEmpty())
			{
				Rect rect4 = rect3;
				rect4.yMin += 28f;
				Text.Font = GameFont.Tiny;
				GUI.color = new Color(0.7f, 0.7f, 0.7f);
				Widgets.Label(rect4, extra);
				GUI.color = Color.white;
			}
		}

		public CreditRecord_RoleTwoCols Compress()
		{
			compressed = true;
			return this;
		}
	}
}
