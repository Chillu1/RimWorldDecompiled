using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class RecipeWorkerCounter
	{
		public RecipeDef recipe;

		public virtual bool CanCountProducts(Bill_Production bill)
		{
			if (recipe.specialProducts == null && recipe.products != null)
			{
				return recipe.products.Count == 1;
			}
			return false;
		}

		public virtual int CountProducts(Bill_Production bill)
		{
			ThingDefCountClass thingDefCountClass = recipe.products[0];
			ThingDef thingDef = thingDefCountClass.thingDef;
			if (thingDefCountClass.thingDef.CountAsResource && !bill.includeEquipped && (bill.includeTainted || !thingDefCountClass.thingDef.IsApparel || !thingDefCountClass.thingDef.apparel.careIfWornByCorpse) && bill.includeFromZone == null && bill.hpRange.min == 0f && bill.hpRange.max == 1f && bill.qualityRange.min == QualityCategory.Awful && bill.qualityRange.max == QualityCategory.Legendary && !bill.limitToAllowedStuff)
			{
				return bill.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) + GetCarriedCount(bill, thingDef);
			}
			int num = 0;
			if (bill.includeFromZone == null)
			{
				num = CountValidThings(bill.Map.listerThings.ThingsOfDef(thingDefCountClass.thingDef), bill, thingDef);
				if (thingDefCountClass.thingDef.Minifiable)
				{
					List<Thing> list = bill.Map.listerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing);
					for (int i = 0; i < list.Count; i++)
					{
						MinifiedThing minifiedThing = (MinifiedThing)list[i];
						if (CountValidThing(minifiedThing.InnerThing, bill, thingDef))
						{
							num += minifiedThing.stackCount * minifiedThing.InnerThing.stackCount;
						}
					}
				}
				num += GetCarriedCount(bill, thingDef);
			}
			else
			{
				foreach (Thing allContainedThing in bill.includeFromZone.AllContainedThings)
				{
					Thing innerIfMinified = allContainedThing.GetInnerIfMinified();
					if (CountValidThing(innerIfMinified, bill, thingDef))
					{
						num += innerIfMinified.stackCount;
					}
				}
			}
			if (bill.includeEquipped)
			{
				foreach (Pawn item in bill.Map.mapPawns.FreeColonistsSpawned)
				{
					List<ThingWithComps> allEquipmentListForReading = item.equipment.AllEquipmentListForReading;
					for (int j = 0; j < allEquipmentListForReading.Count; j++)
					{
						if (CountValidThing(allEquipmentListForReading[j], bill, thingDef))
						{
							num += allEquipmentListForReading[j].stackCount;
						}
					}
					List<Apparel> wornApparel = item.apparel.WornApparel;
					for (int k = 0; k < wornApparel.Count; k++)
					{
						if (CountValidThing(wornApparel[k], bill, thingDef))
						{
							num += wornApparel[k].stackCount;
						}
					}
					ThingOwner directlyHeldThings = item.inventory.GetDirectlyHeldThings();
					for (int l = 0; l < directlyHeldThings.Count; l++)
					{
						if (CountValidThing(directlyHeldThings[l], bill, thingDef))
						{
							num += directlyHeldThings[l].stackCount;
						}
					}
				}
				return num;
			}
			return num;
		}

		public int CountValidThings(List<Thing> things, Bill_Production bill, ThingDef def)
		{
			int num = 0;
			for (int i = 0; i < things.Count; i++)
			{
				if (CountValidThing(things[i], bill, def))
				{
					num++;
				}
			}
			return num;
		}

		public bool CountValidThing(Thing thing, Bill_Production bill, ThingDef def)
		{
			ThingDef def2 = thing.def;
			if (def2 != def)
			{
				return false;
			}
			if (!bill.includeTainted && def2.IsApparel && ((Apparel)thing).WornByCorpse)
			{
				return false;
			}
			if (thing.def.useHitPoints && !bill.hpRange.IncludesEpsilon((float)thing.HitPoints / (float)thing.MaxHitPoints))
			{
				return false;
			}
			CompQuality compQuality = thing.TryGetComp<CompQuality>();
			if (compQuality != null && !bill.qualityRange.Includes(compQuality.Quality))
			{
				return false;
			}
			if (bill.limitToAllowedStuff && !bill.ingredientFilter.Allows(thing.Stuff))
			{
				return false;
			}
			return true;
		}

		public virtual string ProductsDescription(Bill_Production bill)
		{
			return null;
		}

		public virtual bool CanPossiblyStoreInStockpile(Bill_Production bill, Zone_Stockpile stockpile)
		{
			if (!CanCountProducts(bill))
			{
				return true;
			}
			return stockpile.GetStoreSettings().AllowedToAccept(recipe.products[0].thingDef);
		}

		private int GetCarriedCount(Bill_Production bill, ThingDef prodDef)
		{
			int num = 0;
			foreach (Pawn item in bill.Map.mapPawns.FreeColonistsSpawned)
			{
				Thing carriedThing = item.carryTracker.CarriedThing;
				if (carriedThing != null)
				{
					int stackCount = carriedThing.stackCount;
					carriedThing = carriedThing.GetInnerIfMinified();
					if (CountValidThing(carriedThing, bill, prodDef))
					{
						num += stackCount;
					}
				}
			}
			return num;
		}
	}
}
