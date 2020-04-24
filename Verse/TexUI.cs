using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class TexUI
	{
		public static readonly Texture2D TitleBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.05f));

		public static readonly Texture2D HighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

		public static readonly Texture2D HighlightSelectedTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.94f, 0.5f, 0.18f));

		public static readonly Texture2D ArrowTexRight = ContentFinder<Texture2D>.Get("UI/Widgets/ArrowRight");

		public static readonly Texture2D ArrowTexLeft = ContentFinder<Texture2D>.Get("UI/Widgets/ArrowLeft");

		public static readonly Texture2D WinExpandWidget = ContentFinder<Texture2D>.Get("UI/Widgets/WinExpandWidget");

		public static readonly Texture2D ArrowTex = ContentFinder<Texture2D>.Get("UI/Misc/AlertFlashArrow");

		public static readonly Texture2D RotLeftTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotLeft");

		public static readonly Texture2D RotRightTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight");

		public static readonly Texture2D GrayBg = SolidColorMaterials.NewSolidColorTexture(new ColorInt(51, 63, 51, 200).ToColor);

		public static readonly Color AvailResearchColor = new ColorInt(32, 32, 32, 255).ToColor;

		public static readonly Color ActiveResearchColor = new ColorInt(0, 64, 64, 255).ToColor;

		public static readonly Color FinishedResearchColor = new ColorInt(0, 64, 16, 255).ToColor;

		public static readonly Color LockedResearchColor = new ColorInt(42, 42, 42, 255).ToColor;

		public static readonly Color RelatedResearchColor = new ColorInt(0, 0, 0, 255).ToColor;

		public static readonly Color HighlightBgResearchColor = new ColorInt(30, 30, 30, 255).ToColor;

		public static readonly Color HighlightBorderResearchColor = new ColorInt(160, 160, 160, 255).ToColor;

		public static readonly Color DefaultBorderResearchColor = new ColorInt(80, 80, 80, 255).ToColor;

		public static readonly Color DefaultLineResearchColor = new ColorInt(60, 60, 60, 255).ToColor;

		public static readonly Color HighlightLineResearchColor = new ColorInt(51, 205, 217, 255).ToColor;

		public static readonly Color DependencyOutlineResearchColor = new ColorInt(217, 20, 51, 255).ToColor;

		public static readonly Texture2D FastFillTex = Texture2D.whiteTexture;

		public static readonly GUIStyle FastFillStyle = new GUIStyle
		{
			normal = new GUIStyleState
			{
				background = FastFillTex
			}
		};

		public static readonly Texture2D TextBGBlack = ContentFinder<Texture2D>.Get("UI/Widgets/TextBGBlack");

		public static readonly Texture2D GrayTextBG = ContentFinder<Texture2D>.Get("UI/Overlays/GrayTextBG");

		public static readonly Texture2D FloatMenuOptionBG = ContentFinder<Texture2D>.Get("UI/Widgets/FloatMenuOptionBG");

		public static readonly Material GrayscaleGUI = MatLoader.LoadMat("Misc/GrayscaleGUI");
	}
}
