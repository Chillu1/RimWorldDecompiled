using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class BuildCopyCommandUtility
	{
		private static Dictionary<BuildableDef, Designator_Build> cache = new Dictionary<BuildableDef, Designator_Build>();

		private static int lastCacheTick = -1;

		public static Command BuildCopyCommand(BuildableDef buildable, ThingDef stuff)
		{
			return BuildCommand(buildable, stuff, "CommandBuildCopy".Translate(), "CommandBuildCopyDesc".Translate(), allowHotKey: true);
		}

		public static Command BuildCommand(BuildableDef buildable, ThingDef stuff, string label, string description, bool allowHotKey)
		{
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
				des.SetStuffDef(stuff);
				Find.DesignatorManager.Select(des);
			};
			command_Action.defaultLabel = label;
			command_Action.defaultDesc = description;
			command_Action.icon = des.icon;
			command_Action.iconProportions = des.iconProportions;
			command_Action.iconDrawScale = des.iconDrawScale;
			command_Action.iconTexCoords = des.iconTexCoords;
			command_Action.iconAngle = des.iconAngle;
			command_Action.iconOffset = des.iconOffset;
			command_Action.order = 10f;
			if (stuff != null)
			{
				command_Action.defaultIconColor = buildable.GetColorForStuff(stuff);
			}
			else
			{
				command_Action.defaultIconColor = buildable.uiIconColor;
			}
			if (allowHotKey)
			{
				command_Action.hotKey = KeyBindingDefOf.Misc11;
			}
			return command_Action;
		}

		public static Designator_Build FindAllowedDesignator(BuildableDef buildable, bool mustBeVisible = true)
		{
			Game game = Current.Game;
			if (game != null)
			{
				if (lastCacheTick != game.tickManager.TicksGame)
				{
					cache.Clear();
					lastCacheTick = game.tickManager.TicksGame;
				}
				if (cache.ContainsKey(buildable))
				{
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
				List<Designator> allResolvedDesignators = allDefsListForReading[i].AllResolvedDesignators;
				for (int j = 0; j < allResolvedDesignators.Count; j++)
				{
					Designator_Build designator_Build = FindAllowedDesignatorRecursive(allResolvedDesignators[j], buildable, mustBeVisible);
					if (designator_Build != null)
					{
						if (!cache.ContainsKey(buildable))
						{
							cache.Add(buildable, designator_Build);
						}
						return designator_Build;
					}
				}
			}
			if (!cache.ContainsKey(buildable))
			{
				cache.Add(buildable, null);
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
			Designator_Build designator_Build = designator as Designator_Build;
			if (designator_Build != null && designator_Build.PlacingDef == buildable)
			{
				return designator_Build;
			}
			Designator_Dropdown designator_Dropdown = designator as Designator_Dropdown;
			if (designator_Dropdown != null)
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
}
