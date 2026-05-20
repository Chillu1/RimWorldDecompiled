using System;
using RimWorld;

namespace Verse
{
	public class Graphic_MealVariants : Graphic_StackCount
	{
		public override Graphic SubGraphicFor(Thing thing)
		{
			return subGraphics[SubGraphicTypeIndex(thing) + SubGraphicIndexOffset(thing)];
		}

		public int SubGraphicTypeIndex(Thing thing)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return 0;
			}
			return FoodUtility.GetFoodKind(thing) switch
			{
				FoodKind.Meat => subGraphics.Length / 3, 
				FoodKind.NonMeat => subGraphics.Length / 3 * 2, 
				_ => 0, 
			};
		}

		public int SubGraphicIndexOffset(Thing thing)
		{
			if (thing == null)
			{
				return 0;
			}
			switch (subGraphics.Length / 3)
			{
			case 1:
				return 0;
			case 2:
				if (thing.stackCount == 1)
				{
					return 0;
				}
				return 1;
			case 3:
				if (thing.stackCount == 1)
				{
					return 0;
				}
				if (thing.stackCount == thing.def.stackLimit)
				{
					return 2;
				}
				return 1;
			default:
				throw new NotImplementedException("More than 3 different stack size meal graphics per meal type not supported");
			}
		}
	}
}
