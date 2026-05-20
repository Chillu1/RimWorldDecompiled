using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public static class TexUI
{
	public static readonly Texture2D TitleBGTex;

	public static readonly Texture2D HighlightTex;

	public static readonly Texture2D HighlightSelectedTex;

	public static readonly Texture2D ArrowTexRight;

	public static readonly Texture2D ArrowTexLeft;

	public static readonly Texture2D ConcaveArrowTexRight;

	public static readonly Texture2D ConcaveArrowTexLeft;

	public static readonly Texture2D WinExpandWidget;

	public static readonly Texture2D ArrowTex;

	public static readonly Texture2D RotLeftTex;

	public static readonly Texture2D RotRightTex;

	public static readonly Texture2D GuiltyTex;

	public static readonly Texture2D CopyTex;

	public static readonly Texture2D DismissTex;

	public static readonly Texture2D RenameTex;

	public static readonly Texture2D MakeDefault;

	public static readonly Texture2D RectHighlight;

	public static readonly Texture2D GrayBg;

	public static readonly Texture2D DotHighlight;

	public static readonly Texture2D SelectionBracketWhole;

	public static readonly Texture2D Placeholder;

	public static readonly Color OldActiveResearchColor;

	public static readonly Color OldFinishedResearchColor;

	public static readonly Color AvailResearchColor;

	public static readonly Color ActiveResearchColor;

	public static readonly Color OtherActiveResearchColor;

	public static readonly Color FinishedResearchColor;

	public static readonly Color LockedResearchColor;

	public static readonly Color HiddenResearchColor;

	public static readonly Color HighlightBgResearchColor;

	public static readonly Color HighlightBorderResearchColor;

	public static readonly Color BorderResearchSelectedColor;

	public static readonly Color BorderResearchingColor;

	public static readonly Color DefaultBorderResearchColor;

	public static readonly Color ResearchMainTabColor;

	public static readonly Color FinishedResearchColorTransparent;

	public static readonly Color DefaultLineResearchColor;

	public static readonly Color HighlightLineResearchColor;

	public static readonly Color DependencyOutlineResearchColor;

	public static readonly Texture2D FastFillTex;

	public static readonly GUIStyle FastFillStyle;

	public static readonly Texture2D TextBGBlack;

	public static readonly Texture2D GrayTextBG;

	public static readonly Texture2D FloatMenuOptionBG;

	public static readonly Material GrayscaleGUI;

	public static Texture2D SteamDeck_ButtonA;

	static TexUI()
	{
		TitleBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.05f));
		HighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));
		HighlightSelectedTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.94f, 0.5f, 0.18f));
		ArrowTexRight = ContentFinder<Texture2D>.Get("UI/Widgets/ArrowRight");
		ArrowTexLeft = ContentFinder<Texture2D>.Get("UI/Widgets/ArrowLeft");
		ConcaveArrowTexRight = ContentFinder<Texture2D>.Get("UI/Widgets/ConcaveArrowRight");
		ConcaveArrowTexLeft = ContentFinder<Texture2D>.Get("UI/Widgets/ConcaveArrowLeft");
		WinExpandWidget = ContentFinder<Texture2D>.Get("UI/Widgets/WinExpandWidget");
		ArrowTex = ContentFinder<Texture2D>.Get("UI/Misc/AlertFlashArrow");
		RotLeftTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotLeft");
		RotRightTex = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight");
		GuiltyTex = ContentFinder<Texture2D>.Get("UI/Icons/Guilty");
		CopyTex = ContentFinder<Texture2D>.Get("UI/Buttons/Copy");
		DismissTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dismiss");
		RenameTex = ContentFinder<Texture2D>.Get("UI/Buttons/Rename");
		MakeDefault = ContentFinder<Texture2D>.Get("UI/Buttons/MakeDefault");
		RectHighlight = ContentFinder<Texture2D>.Get("UI/Overlays/HighlightAtlas");
		GrayBg = SolidColorMaterials.NewSolidColorTexture(new ColorInt(51, 63, 51, 200).ToColor);
		DotHighlight = ContentFinder<Texture2D>.Get("UI/Overlays/DotHighlight");
		SelectionBracketWhole = ContentFinder<Texture2D>.Get("UI/Overlays/SelectionBracketWhole");
		Placeholder = ContentFinder<Texture2D>.Get("PlaceholderImage");
		OldActiveResearchColor = new ColorInt(0, 64, 64, 255).ToColor;
		OldFinishedResearchColor = new ColorInt(0, 64, 16, 255).ToColor;
		AvailResearchColor = new ColorInt(32, 32, 32, 255).ToColor;
		ActiveResearchColor = new ColorInt(81, 66, 7, 255).ToColor;
		OtherActiveResearchColor = new ColorInt(78, 109, 129, 130).ToColor;
		FinishedResearchColor = new ColorInt(0, 64, 64, 255).ToColor;
		LockedResearchColor = new ColorInt(42, 42, 42, 255).ToColor;
		HiddenResearchColor = new ColorInt(42, 42, 42, 255).ToColor;
		HighlightBgResearchColor = new ColorInt(30, 30, 30, 255).ToColor;
		HighlightBorderResearchColor = new ColorInt(160, 160, 160, 255).ToColor;
		BorderResearchSelectedColor = new ColorInt(240, 240, 240, 255).ToColor;
		BorderResearchingColor = new ColorInt(253, 225, 114, 255).ToColor;
		DefaultBorderResearchColor = new ColorInt(80, 80, 80, 255).ToColor;
		ResearchMainTabColor = new Color(0.2f, 0.8f, 0.85f);
		FinishedResearchColorTransparent = new ColorInt(78, 109, 129, 140).ToColor;
		DefaultLineResearchColor = new ColorInt(60, 60, 60, 255).ToColor;
		HighlightLineResearchColor = new ColorInt(51, 205, 217, 255).ToColor;
		DependencyOutlineResearchColor = new ColorInt(217, 20, 51, 255).ToColor;
		FastFillTex = Texture2D.whiteTexture;
		FastFillStyle = new GUIStyle
		{
			normal = new GUIStyleState
			{
				background = FastFillTex
			}
		};
		TextBGBlack = ContentFinder<Texture2D>.Get("UI/Widgets/TextBGBlack");
		GrayTextBG = ContentFinder<Texture2D>.Get("UI/Overlays/GrayTextBG");
		FloatMenuOptionBG = ContentFinder<Texture2D>.Get("UI/Widgets/FloatMenuOptionBG");
		GrayscaleGUI = MatLoader.LoadMat("Misc/GrayscaleGUI");
		SteamDeck_ButtonA = ContentFinder<Texture2D>.Get("UI/Icons/SteamDeck/button_a");
		GrayscaleGUI.SetTexture(ShaderPropertyIDs.MaskTex, Texture2D.redTexture);
	}
}
