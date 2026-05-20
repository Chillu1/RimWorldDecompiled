using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld;

public static class BuildCopyCommandUtility
{
	private static readonly Dictionary<BuildableDef, Designator_Build> cache = new Dictionary<BuildableDef, Designator_Build>();

	public static Command BuildCopyCommand(BuildableDef buildable, ThingDef stuff, Precept_Building sourcePrecept, ThingStyleDef style, bool styleOverridden, ColorInt? glowerColorOverride = null)
	{
		return BuildCommand(buildable, stuff, sourcePrecept, style, styleOverridden, "CommandBuildCopy".Translate(), "CommandBuildCopyDesc".Translate(), allowHotKey: true, glowerColorOverride);
	}

	public static Command BuildCommand(BuildableDef buildable, ThingDef stuff = null, Precept_Building sourcePrecept = null, ThingStyleDef style = null, bool styleOverridden = false, string label = null, string description = null, bool allowHotKey = false, ColorInt? glowerColorOverride = null)
	{
		if (label == null)
		{
			label = buildable.label;
		}
		if (description == null)
		{
			description = buildable.description;
		}
		Designator_Build des = FindAllowedDesignator(buildable);
		if (des == null)
		{
			return null;
		}
		if (buildable.MadeFromStuff && stuff == null)
		{
			return des;
		}
		Command_Action command_Action = new Command_Action();
		command_Action.action = delegate
		{
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			Find.DesignatorManager.Select(des);
			des.glowerColorOverride = glowerColorOverride;
			des.SetTemporaryVars(stuff, styleOverridden);
		};
		command_Action.defaultLabel = label;
		command_Action.defaultDesc = description;
		ThingDef stuffDefRaw = des.StuffDefRaw;
		des.SetStuffDef(stuff);
		des.styleDef = style;
		command_Action.icon = des.ResolvedIcon(style);
		command_Action.iconProportions = des.iconProportions;
		command_Action.iconDrawScale = des.iconDrawScale;
		command_Action.iconTexCoords = des.iconTexCoords;
		command_Action.iconAngle = des.iconAngle;
		command_Action.iconOffset = des.iconOffset;
		command_Action.Order = 10f;
		command_Action.SetColorOverride(des.IconDrawColor);
		des.sourcePrecept = sourcePrecept;
		des.SetStuffDef(stuffDefRaw);
		if (buildable.uiIconMaterial != null)
		{
			command_Action.overrideMaterial = (des.overrideMaterial = buildable.uiIconMaterial);
		}
		command_Action.defaultIconColor = ((stuff != null) ? buildable.GetColorForStuff(stuff) : buildable.uiIconColor);
		if (allowHotKey)
		{
			command_Action.hotKey = KeyBindingDefOf.Misc11;
		}
		return command_Action;
	}

	public static Designator_Build FindAllowedDesignator(BuildableDef buildable, bool mustBeVisible = true)
	{
		if (Current.Game != null)
		{
			if (cache.ContainsKey(buildable))
			{
				if (mustBeVisible)
				{
					Designator_Build designator_Build = cache[buildable];
					if (designator_Build == null || !designator_Build.Visible)
					{
						return null;
					}
				}
				return cache[buildable];
			}
		}
		else
		{
			cache.Clear();
		}
		List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			foreach (Designator allResolvedAndIdeoDesignator in allDefsListForReading[i].AllResolvedAndIdeoDesignators)
			{
				Designator_Build designator_Build2 = FindAllowedDesignatorRecursive(allResolvedAndIdeoDesignator, buildable, mustBeVisible: false);
				if (designator_Build2 == null)
				{
					continue;
				}
				cache.TryAdd(buildable, designator_Build2);
				object result;
				if (mustBeVisible)
				{
					Designator_Build designator_Build3 = cache[buildable];
					if (designator_Build3 == null || !designator_Build3.Visible)
					{
						result = null;
						goto IL_00aa;
					}
				}
				result = designator_Build2;
				goto IL_00aa;
				IL_00aa:
				return (Designator_Build)result;
			}
		}
		cache.TryAdd(buildable, null);
		return null;
	}

	public static void ClearCache()
	{
		cache.Clear();
	}

	public static Designator FindAllowedDesignatorRoot(BuildableDef buildable, bool mustBeVisible = true)
	{
		List<Designator> allResolvedDesignators = buildable.designationCategory.AllResolvedDesignators;
		for (int i = 0; i < allResolvedDesignators.Count; i++)
		{
			if (FindAllowedDesignatorRecursive(allResolvedDesignators[i], buildable, mustBeVisible) != null)
			{
				return allResolvedDesignators[i];
			}
		}
		return null;
	}

	private static Designator_Build FindAllowedDesignatorRecursive(Designator designator, BuildableDef buildable, bool mustBeVisible)
	{
		if (!Current.Game.Rules.DesignatorAllowed(designator))
		{
			return null;
		}
		if (mustBeVisible && !designator.Visible)
		{
			return null;
		}
		if (designator is Designator_Build designator_Build && designator_Build.PlacingDef == buildable)
		{
			return designator_Build;
		}
		if (designator is Designator_Dropdown designator_Dropdown)
		{
			for (int i = 0; i < designator_Dropdown.Elements.Count; i++)
			{
				Designator_Build designator_Build2 = FindAllowedDesignatorRecursive(designator_Dropdown.Elements[i], buildable, mustBeVisible);
				if (designator_Build2 != null)
				{
					return designator_Build2;
				}
			}
		}
		return null;
	}
}
