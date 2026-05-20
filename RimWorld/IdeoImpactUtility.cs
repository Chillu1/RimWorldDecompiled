using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class IdeoImpactUtility
	{
		public const int MaxMemeImpact = 3;

		public const int MaxCombinedImpact = 9;

		private static readonly Color IconTint = Color.Lerp(ColoredText.ImpactColor, new Color(0.3f, 0.3f, 0.3f, 1f), 0.9f);

		private static readonly Texture2D[] ImpactIcons = new Texture2D[4]
		{
			null,
			ContentFinder<Texture2D>.Get("UI/Icons/Impact/MemeImpact1"),
			ContentFinder<Texture2D>.Get("UI/Icons/Impact/MemeImpact2"),
			ContentFinder<Texture2D>.Get("UI/Icons/Impact/MemeImpact3")
		};

		public static string MemeImpactLabel(int impact)
		{
			impact = Mathf.Clamp(impact, 1, 3);
			return $"IdeoMemeImpactLabel_{impact}".Translate();
		}

		public static string OverallImpactLabel(int impact)
		{
			impact = Mathf.Clamp(impact, 1, 9);
			return $"IdeoImpactLabel_{impact}".Translate();
		}

		public static void DrawImpactIcon(Rect rect, int impact)
		{
			impact = Mathf.Clamp(impact, 1, 3);
			Color color = GUI.color;
			GUI.color = IconTint;
			GUI.DrawTexture(rect, ImpactIcons[impact]);
			GUI.color = color;
		}
	}
}
