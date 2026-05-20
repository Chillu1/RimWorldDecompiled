using UnityEngine;

namespace Verse;

public class CreditRecord_Role : CreditsEntry
{
	public string roleKey;

	public string creditee;

	public string extra;

	public bool displayKey;

	public bool compressed;

	public bool smallFont;

	public CreditRecord_Role()
	{
	}

	public CreditRecord_Role(string roleKey, string creditee, string extra = null)
	{
		this.roleKey = roleKey;
		this.creditee = creditee;
		this.extra = extra;
	}

	public override float DrawHeight(float width)
	{
		if (roleKey.NullOrEmpty())
		{
			width *= 0.5f;
		}
		Text.Font = (smallFont ? GameFont.Small : GameFont.Medium);
		float result = (compressed ? Text.CalcHeight(creditee, width * 0.5f) : 50f);
		Text.Font = GameFont.Medium;
		return result;
	}

	public override void Draw(Rect rect)
	{
		Text.Font = (smallFont ? GameFont.Small : GameFont.Medium);
		Text.Anchor = TextAnchor.MiddleLeft;
		Rect rect2 = rect;
		rect2.width = 0f;
		if (!roleKey.NullOrEmpty())
		{
			rect2.width = rect.width / 2f;
			if (displayKey)
			{
				Widgets.Label(rect2, roleKey);
			}
		}
		Rect rect3 = rect;
		rect3.xMin = rect2.xMax;
		if (roleKey.NullOrEmpty())
		{
			Text.Anchor = TextAnchor.MiddleCenter;
		}
		Widgets.Label(rect3, creditee);
		Text.Anchor = TextAnchor.MiddleLeft;
		if (!extra.NullOrEmpty())
		{
			Rect rect4 = rect3;
			rect4.yMin += 28f;
			Text.Font = GameFont.Tiny;
			GUI.color = new Color(0.7f, 0.7f, 0.7f);
			Widgets.Label(rect4, extra);
			GUI.color = Color.white;
		}
		Text.Font = GameFont.Medium;
		Text.Anchor = TextAnchor.UpperLeft;
	}

	public CreditRecord_Role Compress()
	{
		compressed = true;
		return this;
	}

	public CreditRecord_Role WithSmallFont()
	{
		smallFont = true;
		return this;
	}
}
