using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IssueDef : Def
{
	public bool allowMultiplePrecepts;

	public bool forceWriteLabelInPreceptFloatMenuOption;

	public string iconPath;

	private Texture2D icon;

	public Texture2D Icon
	{
		get
		{
			if (string.IsNullOrEmpty(iconPath))
			{
				return null;
			}
			if (icon == null)
			{
				icon = ContentFinder<Texture2D>.Get(iconPath);
			}
			return icon;
		}
	}

	public bool HasDefaultPrecept => DefDatabase<PreceptDef>.AllDefs.Any((PreceptDef x) => x.issue == this && x.defaultSelectionWeight > 0f && x.visible);
}
