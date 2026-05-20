using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class StyleCategoryDef : Def
{
	public List<ThingDefStyle> thingDefStyles;

	[NoTranslate]
	public string iconPath;

	public bool fixedIdeoOnly;

	public List<BuildableDef> addDesignators;

	public List<DesignatorDropdownGroupDef> addDesignatorGroups;

	public SoundDef soundOngoingRitual;

	public RitualVisualEffectDef ritualVisualEffectDef;

	private Texture2D cachedIcon;

	private List<BuildableDef> cachedAllDesignatorBuildables;

	private static List<ThingStyleDef> tmpAvailableStyles = new List<ThingStyleDef>();

	public Texture2D Icon
	{
		get
		{
			if (cachedIcon == null)
			{
				if (iconPath.NullOrEmpty())
				{
					cachedIcon = BaseContent.BadTex;
				}
				else
				{
					cachedIcon = ContentFinder<Texture2D>.Get(iconPath);
				}
			}
			return cachedIcon;
		}
	}

	public List<BuildableDef> AllDesignatorBuildables
	{
		get
		{
			if (cachedAllDesignatorBuildables == null)
			{
				cachedAllDesignatorBuildables = new List<BuildableDef>();
				if (addDesignators != null)
				{
					foreach (BuildableDef addDesignator in addDesignators)
					{
						cachedAllDesignatorBuildables.Add(addDesignator);
					}
				}
				if (addDesignatorGroups != null)
				{
					foreach (DesignatorDropdownGroupDef addDesignatorGroup in addDesignatorGroups)
					{
						cachedAllDesignatorBuildables.AddRange(addDesignatorGroup.BuildablesWithoutDefaultDesignators());
					}
				}
			}
			return cachedAllDesignatorBuildables;
		}
	}

	public ThingStyleDef GetStyleForThingDef(BuildableDef thingDef, Precept precept = null)
	{
		try
		{
			for (int i = 0; i < thingDefStyles.Count; i++)
			{
				if (thingDefStyles[i].ThingDef == thingDef)
				{
					tmpAvailableStyles.Add(thingDefStyles[i].StyleDef);
				}
			}
			if (tmpAvailableStyles.Count == 0)
			{
				return null;
			}
			if (tmpAvailableStyles.Count == 1 || precept == null)
			{
				return tmpAvailableStyles[0];
			}
			return tmpAvailableStyles[Rand.RangeSeeded(0, tmpAvailableStyles.Count, precept.randomSeed)];
		}
		finally
		{
			tmpAvailableStyles.Clear();
		}
	}
}
