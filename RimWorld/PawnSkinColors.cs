using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PawnSkinColors
	{
		private struct SkinColorData
		{
			public float melanin;

			public float selector;

			public Color color;

			public SkinColorData(float melanin, float selector, Color color)
			{
				this.melanin = melanin;
				this.selector = selector;
				this.color = color;
			}
		}

		private static readonly SkinColorData[] SkinColors = new SkinColorData[6]
		{
			new SkinColorData(0f, 0f, new Color(242f / 255f, 79f / 85f, 224f / 255f)),
			new SkinColorData(0.25f, 0.2f, new Color(1f, 239f / 255f, 71f / 85f)),
			new SkinColorData(0.5f, 0.7f, new Color(1f, 239f / 255f, 63f / 85f)),
			new SkinColorData(0.75f, 0.8f, new Color(76f / 85f, 158f / 255f, 0.3529412f)),
			new SkinColorData(0.9f, 0.9f, new Color(26f / 51f, 91f / 255f, 16f / 85f)),
			new SkinColorData(1f, 1f, new Color(33f / 85f, 14f / 51f, 12f / 85f))
		};

		public static bool IsDarkSkin(Color color)
		{
			Color skinColor = GetSkinColor(0.5f);
			return color.r + color.g + color.b <= skinColor.r + skinColor.g + skinColor.b + 0.01f;
		}

		public static Color GetSkinColor(float melanin)
		{
			int skinDataIndexOfMelanin = GetSkinDataIndexOfMelanin(melanin);
			if (skinDataIndexOfMelanin == SkinColors.Length - 1)
			{
				return SkinColors[skinDataIndexOfMelanin].color;
			}
			float t = Mathf.InverseLerp(SkinColors[skinDataIndexOfMelanin].melanin, SkinColors[skinDataIndexOfMelanin + 1].melanin, melanin);
			return Color.Lerp(SkinColors[skinDataIndexOfMelanin].color, SkinColors[skinDataIndexOfMelanin + 1].color, t);
		}

		public static float RandomMelanin(Faction fac)
		{
			float num = ((fac != null) ? Rand.Range(Mathf.Clamp01(fac.centralMelanin - fac.def.geneticVariance), Mathf.Clamp01(fac.centralMelanin + fac.def.geneticVariance)) : Rand.Value);
			int num2 = 0;
			for (int i = 0; i < SkinColors.Length && num >= SkinColors[i].selector; i++)
			{
				num2 = i;
			}
			if (num2 == SkinColors.Length - 1)
			{
				return SkinColors[num2].melanin;
			}
			float t = Mathf.InverseLerp(SkinColors[num2].selector, SkinColors[num2 + 1].selector, num);
			return Mathf.Lerp(SkinColors[num2].melanin, SkinColors[num2 + 1].melanin, t);
		}

		public static float GetMelaninCommonalityFactor(float melanin)
		{
			int skinDataIndexOfMelanin = GetSkinDataIndexOfMelanin(melanin);
			if (skinDataIndexOfMelanin == SkinColors.Length - 1)
			{
				return GetSkinDataCommonalityFactor(skinDataIndexOfMelanin);
			}
			float t = Mathf.InverseLerp(SkinColors[skinDataIndexOfMelanin].melanin, SkinColors[skinDataIndexOfMelanin + 1].melanin, melanin);
			return Mathf.Lerp(GetSkinDataCommonalityFactor(skinDataIndexOfMelanin), GetSkinDataCommonalityFactor(skinDataIndexOfMelanin + 1), t);
		}

		public static float GetRandomMelaninSimilarTo(float value, float clampMin = 0f, float clampMax = 1f)
		{
			return Mathf.Clamp01(Mathf.Clamp(Rand.Gaussian(value, 0.05f), clampMin, clampMax));
		}

		private static float GetSkinDataCommonalityFactor(int skinDataIndex)
		{
			float num = 0f;
			for (int i = 0; i < SkinColors.Length; i++)
			{
				num = Mathf.Max(num, GetTotalAreaWhereClosestToSelector(i));
			}
			return GetTotalAreaWhereClosestToSelector(skinDataIndex) / num;
		}

		private static float GetTotalAreaWhereClosestToSelector(int skinDataIndex)
		{
			float num = 0f;
			if (skinDataIndex == 0)
			{
				num += SkinColors[skinDataIndex].selector;
			}
			else if (SkinColors.Length > 1)
			{
				num += (SkinColors[skinDataIndex].selector - SkinColors[skinDataIndex - 1].selector) / 2f;
			}
			if (skinDataIndex == SkinColors.Length - 1)
			{
				num += 1f - SkinColors[skinDataIndex].selector;
			}
			else if (SkinColors.Length > 1)
			{
				num += (SkinColors[skinDataIndex + 1].selector - SkinColors[skinDataIndex].selector) / 2f;
			}
			return num;
		}

		private static int GetSkinDataIndexOfMelanin(float melanin)
		{
			int result = 0;
			for (int i = 0; i < SkinColors.Length && melanin >= SkinColors[i].melanin; i++)
			{
				result = i;
			}
			return result;
		}
	}
}
