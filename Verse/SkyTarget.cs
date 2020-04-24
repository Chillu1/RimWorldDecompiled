using UnityEngine;

namespace Verse
{
	public struct SkyTarget
	{
		public float glow;

		public SkyColorSet colors;

		public float lightsourceShineSize;

		public float lightsourceShineIntensity;

		public SkyTarget(float glow, SkyColorSet colorSet, float lightsourceShineSize, float lightsourceShineIntensity)
		{
			this.glow = glow;
			this.lightsourceShineSize = lightsourceShineSize;
			this.lightsourceShineIntensity = lightsourceShineIntensity;
			colors = colorSet;
		}

		public static SkyTarget Lerp(SkyTarget A, SkyTarget B, float t)
		{
			SkyTarget result = default(SkyTarget);
			result.colors = SkyColorSet.Lerp(A.colors, B.colors, t);
			result.glow = Mathf.Lerp(A.glow, B.glow, t);
			result.lightsourceShineSize = Mathf.Lerp(A.lightsourceShineSize, B.lightsourceShineSize, t);
			result.lightsourceShineIntensity = Mathf.Lerp(A.lightsourceShineIntensity, B.lightsourceShineIntensity, t);
			return result;
		}

		public static SkyTarget LerpDarken(SkyTarget A, SkyTarget B, float t)
		{
			SkyTarget result = default(SkyTarget);
			result.colors = SkyColorSet.Lerp(A.colors, B.colors, t);
			result.glow = Mathf.Lerp(A.glow, Mathf.Min(A.glow, B.glow), t);
			result.lightsourceShineSize = Mathf.Lerp(A.lightsourceShineSize, Mathf.Min(A.lightsourceShineSize, B.lightsourceShineSize), t);
			result.lightsourceShineIntensity = Mathf.Lerp(A.lightsourceShineIntensity, Mathf.Min(A.lightsourceShineIntensity, B.lightsourceShineIntensity), t);
			return result;
		}

		public override string ToString()
		{
			return "(glow=" + glow.ToString("F2") + ", colors=" + colors.ToString() + ", lightsourceShineSize=" + lightsourceShineSize.ToString() + ", lightsourceShineIntensity=" + lightsourceShineIntensity.ToString() + ")";
		}
	}
}
