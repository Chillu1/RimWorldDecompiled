using System.Collections.Generic;
using RimWorld;

namespace Verse;

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
		if (thingDefCountClass.thingDef.CountAsResource && !bill.includeEquipped && (bill.includeTainted || !thingDefCountClass.thingDef.IsApparel || !thingDefCountClass.thingDef.apparel.careIfWornByCorpse) && bill.GetIncludeSlotGroup() == null && bill.hpRange.min == 0f && bill.hpRange.max == 1f && bill.qualityRange.min == QualityCategory.Awful && bill.qualityRange.max == QualityCategory.Legendary && !bill.limitToAllowedStuff)
		{
			return bill.Map.resourceCounter.GetCount(thingDefCountClass.thingDef) + GetCarriedCount(bill, thingDef);
		}
		int num = 0;
		ISlotGroup includeSlotGroup = bill.GetIncludeSlotGroup();
		if (includeSlotGroup == null)
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
			foreach (IHaulSource item in bill.Map.haulDestinationManager.AllHaulSourcesListForReading)
			{
				num += CountValidThings(item.GetDirectlyHeldThings(), bill, thingDef);
			}
		}
		else
		{
			foreach (Thing heldThing in includeSlotGroup.HeldThings)
			{
				Thing innerIfMinified = heldThing.GetInnerIfMinified();
				if (CountValidThing(innerIfMinified, bill, thingDef))
				{
					num += innerIfMinified.stackCount;
				}
			}
		}
		if (bill.includeEquipped)
		{
			foreach (Pawn item2 in bill.Map.mapPawns.FreeColonistsSpawned)
			{
				List<ThingWithComps> allEquipmentListForReading = item2.equipment.AllEquipmentListForReading;
				for (int j = 0; j < allEquipmentListForReading.Count; j++)
				{
					if (CountValidThing(allEquipmentListForReading[j], bill, thingDef))
					{
						num += allEquipmentListForReading[j].stackCount;
					}
				}
				List<Apparel> wornApparel = item2.apparel.WornApparel;
				for (int k = 0; k < wornApparel.Count; k++)
				{
					if (CountValidThing(wornApparel[k], bill, thingDef))
					{
						num += wornApparel[k].stackCount;
					}
				}
				ThingOwner directlyHeldThings = item2.inventory.GetDirectlyHeldThings();
				for (int l = 0; l < directlyHeldThings.Count; l++)
				{
					if (CountValidThing(directlyHeldThings[l], bill, thingDef))
					{
						num += directlyHeldThings[l].stackCount;
					}
				}
			}
		}
		return num;
	}

	private int CountValidThings(ThingOwner thingOwner, Bill_Production bill, ThingDef def)
	{
		int num = 0;
		for (int i = 0; i < thingOwner.Count; i++)
		{
			if (CountValidThing(thingOwner[i], bill, def))
			{
				num += thingOwner[i].stackCount;
			}
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
		if (thing.SpawnedOrAnyParentSpawned && thing.PositionHeld.Fogged(thing.MapHeld))
		{
			return false;
		}
		return true;
	}

	public virtual string ProductsDescription(Bill_Production bill)
	{
		return null;
	}

	public virtual bool CanPossiblyStore(Bill_Production bill, ISlotGroup slotGroup)
	{
		if (!CanCountProducts(bill))
		{
			return true;
		}
		return slotGroup.Settings.AllowedToAccept(recipe.products[0].thingDef);
	}

	private int GetCarriedCount(Bill_Production bill, ThingDef prodDef)
	{
		int num = 0;
		foreach (Pawn item in bill.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
		{
			Thing thing = item.carryTracker?.CarriedThing;
			if (thing != null)
			{
				int stackCount = thing.stackCount;
				thing = thing.GetInnerIfMinified();
				if (CountValidThing(thing, bill, prodDef))
				{
					num += stackCount;
				}
			}
		}
		return num;
	}
}
