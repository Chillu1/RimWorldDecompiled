using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class GenStuff
	{
		public static ThingDef DefaultStuffFor(BuildableDef bd)
		{
			if (!bd.MadeFromStuff)
			{
				return null;
			}
			ThingDef thingDef = bd as ThingDef;
			if (thingDef != null)
			{
				if (thingDef.IsMeleeWeapon)
				{
					if (ThingDefOf.Steel.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Steel;
					}
					if (ThingDefOf.Plasteel.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Plasteel;
					}
				}
				if (thingDef.IsApparel)
				{
					if (ThingDefOf.Cloth.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Cloth;
					}
					if (ThingDefOf.Leather_Plain.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Leather_Plain;
					}
					if (ThingDefOf.Steel.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Steel;
					}
				}
			}
			if (ThingDefOf.WoodLog.stuffProps.CanMake(bd))
			{
				return ThingDefOf.WoodLog;
			}
			if (ThingDefOf.Steel.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Steel;
			}
			if (ThingDefOf.Plasteel.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Plasteel;
			}
			if (ThingDefOf.BlocksGranite.stuffProps.CanMake(bd))
			{
				return ThingDefOf.BlocksGranite;
			}
			if (ThingDefOf.Cloth.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Cloth;
			}
			if (ThingDefOf.Leather_Plain.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Leather_Plain;
			}
			return AllowedStuffsFor(bd).First();
		}

		public static ThingDef RandomStuffFor(ThingDef td)
		{
			if (!td.MadeFromStuff)
			{
				return null;
			}
			return AllowedStuffsFor(td).RandomElement();
		}

		public static ThingDef RandomStuffByCommonalityFor(ThingDef td, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				return null;
			}
			if (!TryRandomStuffByCommonalityFor(td, out ThingDef stuff, maxTechLevel))
			{
				return DefaultStuffFor(td);
			}
			return stuff;
		}

		public static IEnumerable<ThingDef> AllowedStuffsFor(BuildableDef td, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				yield break;
			}
			List<ThingDef> allDefs = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefs.Count; i++)
			{
				ThingDef thingDef = allDefs[i];
				if (thingDef.IsStuff && (maxTechLevel == TechLevel.Undefined || (int)thingDef.techLevel <= (int)maxTechLevel) && thingDef.stuffProps.CanMake(td))
				{
					yield return thingDef;
				}
			}
		}

		public static IEnumerable<ThingDef> AllowedStuffs(List<StuffCategoryDef> categories, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			List<ThingDef> allDefs = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefs.Count; i++)
			{
				ThingDef thingDef = allDefs[i];
				if (!thingDef.IsStuff || (maxTechLevel != 0 && (int)thingDef.techLevel > (int)maxTechLevel))
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < thingDef.stuffProps.categories.Count; j++)
				{
					for (int k = 0; k < categories.Count; k++)
					{
						if (thingDef.stuffProps.categories[j] == categories[k])
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					yield return thingDef;
				}
			}
		}

		public static bool TryRandomStuffByCommonalityFor(ThingDef td, out ThingDef stuff, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				stuff = null;
				return true;
			}
			return AllowedStuffsFor(td, maxTechLevel).TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out stuff);
		}

		public static bool TryRandomStuffFor(ThingDef td, out ThingDef stuff, TechLevel maxTechLevel = TechLevel.Undefined, Predicate<ThingDef> validator = null)
		{
			if (!td.MadeFromStuff)
			{
				stuff = null;
				return true;
			}
			IEnumerable<ThingDef> source = AllowedStuffsFor(td, maxTechLevel);
			if (validator != null)
			{
				source = source.Where((ThingDef x) => validator(x));
			}
			return source.TryRandomElement(out stuff);
		}

		public static ThingDef RandomStuffInexpensiveFor(ThingDef thingDef, Faction faction, Predicate<ThingDef> validator = null)
		{
			return RandomStuffInexpensiveFor(thingDef, faction?.def.techLevel ?? TechLevel.Undefined, validator);
		}

		public static ThingDef RandomStuffInexpensiveFor(ThingDef thingDef, TechLevel maxTechLevel = TechLevel.Undefined, Predicate<ThingDef> validator = null)
		{
			if (!thingDef.MadeFromStuff)
			{
				return null;
			}
			IEnumerable<ThingDef> enumerable = AllowedStuffsFor(thingDef, maxTechLevel);
			if (validator != null)
			{
				enumerable = enumerable.Where((ThingDef x) => validator(x));
			}
			float cheapestPrice = -1f;
			foreach (ThingDef item in enumerable)
			{
				float num = item.BaseMarketValue / item.VolumePerUnit;
				if (cheapestPrice == -1f || num < cheapestPrice)
				{
					cheapestPrice = num;
				}
			}
			enumerable = enumerable.Where((ThingDef x) => x.BaseMarketValue / x.VolumePerUnit <= cheapestPrice * 4f);
			if (enumerable.TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out ThingDef result))
			{
				return result;
			}
			return null;
		}
	}
}
