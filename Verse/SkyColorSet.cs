using UnityEngine;

namespace Verse
{
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
			SkyColorSet result = default(SkyColorSet);
			result.sky = Color.Lerp(A.sky, B.sky, t);
			result.shadow = Color.Lerp(A.shadow, B.shadow, t);
			result.overlay = Color.Lerp(A.overlay, B.overlay, t);
			result.saturation = Mathf.Lerp(A.saturation, B.saturation, t);
			return result;
		}

		public override string ToString()
		{
			return "(sky=" + sky + ", shadow=" + shadow + ", overlay=" + overlay + ", sat=" + saturation + ")";
		}
	}
}
