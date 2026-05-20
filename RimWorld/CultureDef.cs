using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CultureDef : Def
	{
		public RulePackDef pawnNameMaker;

		public RulePackDef pawnNameMakerFemale;

		public RulePackDef ideoNameMaker;

		public RulePackDef deityNameMaker;

		public RulePackDef deityTypeMaker;

		public RulePackDef leaderTitleMaker;

		public RulePackDef festivalNameMaker;

		public List<ThingStyleCategoryWithPriority> thingStyleCategories;

		public List<StyleItemTagWeighted> styleItemTags;

		public IdeoWeaponClassPair preferredWeaponClasses;

		[NoTranslate]
		public List<string> allowedPlaceTags = new List<string>();

		public string iconPath;

		public Color iconColor = Color.white;

		private Texture2D icon;

		public Texture2D Icon
		{
			get
			{
				if (icon == null)
				{
					icon = ContentFinder<Texture2D>.Get(iconPath);
				}
				return icon;
			}
		}

		public RulePackDef GetPawnNameMaker(Gender gender)
		{
			if (gender == Gender.Female && pawnNameMakerFemale != null)
			{
				return pawnNameMakerFemale;
			}
			return pawnNameMaker;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (!thingStyleCategories.NullOrEmpty())
			{
				foreach (ThingStyleCategoryWithPriority thingStyleCategory in thingStyleCategories)
				{
					if (thingStyleCategory.priority <= 0f)
					{
						yield return "style category " + thingStyleCategory.category.LabelCap + " has <= 0 priority. It must be positive.";
					}
				}
			}
			if (styleItemTags.NullOrEmpty())
			{
				yield return "no style item tags defined.";
			}
			if (ModsConfig.IdeologyActive && !allowedPlaceTags.Any())
			{
				yield return "at least one allowedPlaceTags must be defined in ideology.";
			}
		}
	}
}
