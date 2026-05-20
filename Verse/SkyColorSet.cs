using UnityEngine;

namespace Verse;

public struct SkyColorSet
{
	public Color sky;

	public Color shadow;

	public Color overlay;

	public float saturation;

	public SkyColorSet(Color sky, Color shadow, Color overlay, float saturation)
	{
		this.sky = sky;
		this.shadow = shadow;
		this.overlay = overlay;
		this.saturation = saturation;
	}

	public static SkyColorSet Lerp(SkyColorSet A, SkyColorSet B, float t)
	{
		return new SkyColorSet
		{
			sky = Color.Lerp(A.sky, B.sky, t),
			shadow = Color.Lerp(A.shadow, B.shadow, t),
			overlay = Color.Lerp(A.overlay, B.overlay, t),
			saturation = Mathf.Lerp(A.saturation, B.saturation, t)
		};
	}

	public static SkyColorSet LerpDarken(SkyColorSet A, SkyColorSet B, float t)
	{
		return new SkyColorSet
		{
			sky = Color.Lerp(A.sky, A.sky.Min(B.sky), t),
			shadow = Color.Lerp(A.shadow, A.shadow.Min(B.shadow), t),
			overlay = Color.Lerp(A.overlay, A.overlay.Min(B.overlay), t),
			saturation = Mathf.Lerp(A.saturation, Mathf.Min(A.saturation, B.saturation), t)
		};
	}

	public override string ToString()
	{
		string[] obj = new string[9] { "(sky=", null, null, null, null, null, null, null, null };
		Color color = sky;
		obj[1] = color.ToString();
		obj[2] = ", shadow=";
		color = shadow;
		obj[3] = color.ToString();
		obj[4] = ", overlay=";
		color = overlay;
		obj[5] = color.ToString();
		obj[6] = ", sat=";
		obj[7] = saturation.ToString();
		obj[8] = ")";
		return string.Concat(obj);
	}
}
