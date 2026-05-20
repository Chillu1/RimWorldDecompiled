using RimWorld;
using UnityEngine;

namespace Verse;

public class ThingStyleDef : Def
{
	[MustTranslate]
	public string overrideLabel;

	public GraphicData graphicData;

	public GraphicData blueprintGraphicData;

	[NoTranslate]
	public string uiIconPath;

	public float uiIconScale = 1f;

	[NoTranslate]
	public string wornGraphicPath;

	public Color color;

	private Graphic graphic;

	private Texture2D uiIcon;

	private bool useWornGraphicMask;

	private StyleCategoryDef cachedCategory;

	private bool? hasCategory;

	public Graphic Graphic => graphic;

	public Texture2D UIIcon => uiIcon;

	public bool UseWornGraphicMask => useWornGraphicMask;

	public StyleCategoryDef Category
	{
		get
		{
			if (!hasCategory.HasValue)
			{
				foreach (StyleCategoryDef allDef in DefDatabase<StyleCategoryDef>.AllDefs)
				{
					if (allDef.thingDefStyles.Any((ThingDefStyle x) => x.StyleDef == this))
					{
						cachedCategory = allDef;
						hasCategory = true;
						break;
					}
				}
				if (cachedCategory == null)
				{
					hasCategory = false;
				}
			}
			return cachedCategory;
		}
	}

	public override void PostLoad()
	{
		if (graphicData == null)
		{
			return;
		}
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			if (graphicData.shaderType == null)
			{
				graphicData.shaderType = ShaderTypeDefOf.Cutout;
			}
			graphic = graphicData.Graphic;
			blueprintGraphicData = new GraphicData();
			blueprintGraphicData.CopyFrom(graphicData);
			blueprintGraphicData.shaderType = ShaderTypeDefOf.EdgeDetect;
			blueprintGraphicData.color = ThingDefGenerator_Buildings.BlueprintColor;
			blueprintGraphicData.colorTwo = Color.white;
			blueprintGraphicData.shadowData = null;
			blueprintGraphicData.renderQueue = 2950;
			if (graphic == BaseContent.BadGraphic)
			{
				graphic = null;
			}
			ResolveUIIcon();
		});
	}

	public Texture2D IconForIndex(int index, Rot4? rot = null)
	{
		return (Texture2D)graphic.ExtractInnerGraphicFor(null, index).MatAt(rot ?? Rot4.North).mainTexture;
	}

	private void ResolveUIIcon()
	{
		if (!uiIconPath.NullOrEmpty())
		{
			uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
		}
		else if (graphic != null)
		{
			Material material = graphic.ExtractInnerGraphicFor(null).MatAt(Rot4.North);
			uiIcon = (Texture2D)material.mainTexture;
		}
	}
}
