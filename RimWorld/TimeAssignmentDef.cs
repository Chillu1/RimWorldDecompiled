using UnityEngine;
using Verse;

namespace RimWorld;

public class TimeAssignmentDef : Def
{
	public Color color;

	public bool allowRest = true;

	public bool allowJoy = true;

	[Unsaved(false)]
	public string cachedHighlightNotSelectedTag;

	private Texture2D colorTextureInt;

	public Texture2D ColorTexture
	{
		get
		{
			if (colorTextureInt == null)
			{
				colorTextureInt = SolidColorMaterials.NewSolidColorTexture(color);
			}
			return colorTextureInt;
		}
	}

	public override void PostLoad()
	{
		base.PostLoad();
		cachedHighlightNotSelectedTag = "TimeAssignmentButton-" + defName + "-NotSelected";
	}
}
