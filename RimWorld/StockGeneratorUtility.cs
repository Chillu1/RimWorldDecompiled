using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StockGeneratorUtility
	{
		public static IEnumerable<Thing> TryMakeForStock(ThingDef thingDef, int count)
		{
			if (thingDef.MadeFromStuff)
			{
				for (int i = 0; i < count; i++)
				{
					Thing thing = TryMakeForStockSingle(thingDef, 1);
					if (thing != null)
					{
						yield return thing;
					}
				}
			}
			else
			{
				Thing thing2 = TryMakeForStockSingle(thingDef, count);
				if (thing2 != null)
				{
					yield return thing2;
				}
			}
		}

		public static Thing TryMakeForStockSingle(ThingDef thingDef, int stackCount)
		{
			if (stackCount <= 0)
			{
				return null;
			}
			if (!thingDef.tradeability.TraderCanSell())
			{
				Log.Error("Tried to make non-trader-sellable thing for trader stock: " + thingDef);
				return null;
			}
			ThingDef result = null;
			if (thingDef.MadeFromStuff && !(from x in GenStuff.AllowedStuffsFor(thingDef)
				where !PawnWeaponGenerator.IsDerpWeapon(thingDef, x)
				select x).TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out result))
			{
				result = GenStuff.RandomStuffByCommonalityFor(thingDef);
			}
			Thing thing = ThingMaker.MakeThing(thingDef, result);
			thing.stackCount = stackCount;
			return thing;
		}
	}
}
