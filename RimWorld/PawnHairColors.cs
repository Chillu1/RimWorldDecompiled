using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PawnHairColors
	{
		public static Color RandomHairColor(Color skinColor, int ageYears)
		{
			if (Rand.Value < 0.02f)
			{
				return new Color(Rand.Value, Rand.Value, Rand.Value);
			}
			if (ageYears > 40)
			{
				float num = GenMath.SmootherStep(40f, 75f, ageYears);
				if (Rand.Value < num)
				{
					float num2 = Rand.Range(0.65f, 0.85f);
					return new Color(num2, num2, num2);
				}
			}
			if (PawnSkinColors.IsDarkSkin(skinColor) || Rand.Value < 0.5f)
			{
				float value = Rand.Value;
				if (value < 0.25f)
				{
					return new Color(0.2f, 0.2f, 0.2f);
				}
				if (value < 0.5f)
				{
					return new Color(0.31f, 0.28f, 0.26f);
				}
				if (value < 0.75f)
				{
					return new Color(0.25f, 0.2f, 0.15f);
				}
				return new Color(0.3f, 0.2f, 0.1f);
			}
			float value2 = Rand.Value;
			if (value2 < 0.25f)
			{
				return new Color(0.3529412f, 58f / 255f, 32f / 255f);
			}
			if (value2 < 0.5f)
			{
				return new Color(44f / 85f, 83f / 255f, 47f / 255f);
			}
			if (value2 < 0.75f)
			{
				return new Color(193f / 255f, 146f / 255f, 0.333333343f);
			}
			return new Color(79f / 85f, 202f / 255f, 52f / 85f);
		}
	}
}
