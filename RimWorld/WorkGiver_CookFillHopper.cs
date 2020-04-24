using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_CookFillHopper : WorkGiver_Scanner
	{
		private static string TheOnlyAvailableFoodIsInStorageOfHigherPriorityTrans;

		private static string NoFoodToFillHopperTrans;

		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.Hopper);

		public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

		public WorkGiver_CookFillHopper()
		{
			if (TheOnlyAvailableFoodIsInStorageOfHigherPriorityTrans == null)
			{
				TheOnlyAvailableFoodIsInStorageOfHigherPriorityTrans = "TheOnlyAvailableFoodIsInStorageOfHigherPriority".Translate();
			}
			if (NoFoodToFillHopperTrans == null)
			{
				NoFoodToFillHopperTrans = "NoFoodToFillHopper".Translate();
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
			ISlotGroupParent slotGroupParent = thing as ISlotGroupParent;
			if (slotGroupParent == null)
			{
				return null;
			}
			if (!pawn.CanReserve(thing.Position))
			{
				return null;
			}
			int num = 0;
			List<Thing> list = pawn.Map.thingGrid.ThingsListAt(thing.Position);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing2 = list[i];
				if (Building_NutrientPasteDispenser.IsAcceptableFeedstock(thing2.def))
				{
					num += thing2.stackCount;
				}
			}
			if (num > 25)
			{
				JobFailReason.Is("AlreadyFilledLower".Translate());
				return null;
			}
			return HopperFillFoodJob(pawn, slotGroupParent);
		}

		public static Job HopperFillFoodJob(Pawn pawn, ISlotGroupParent hopperSgp)
		{
			Building building = (Building)hopperSgp;
			if (!pawn.CanReserveAndReach(building.Position, PathEndMode.Touch, pawn.NormalMaxDanger()))
			{
				return null;
			}
			ThingDef thingDef = null;
			Thing firstItem = building.Position.GetFirstItem(building.Map);
			if (firstItem != null)
			{
				if (!Building_NutrientPasteDispenser.IsAcceptableFeedstock(firstItem.def))
				{
					if (firstItem.IsForbidden(pawn))
					{
						return null;
					}
					return HaulAIUtility.HaulAsideJobFor(pawn, firstItem);
				}
				thingDef = firstItem.def;
			}
			List<Thing> list = (thingDef != null) ? pawn.Map.listerThings.ThingsOfDef(thingDef) : pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (!thing.def.IsNutritionGivingIngestible || (thing.def.ingestible.preferability != FoodPreferability.RawBad && thing.def.ingestible.preferability != FoodPreferability.RawTasty) || !HaulAIUtility.PawnCanAutomaticallyHaul(pawn, thing, forced: false) || !pawn.Map.haulDestinationManager.SlotGroupAt(building.Position).Settings.AllowedToAccept(thing))
				{
					continue;
				}
				if ((int)StoreUtility.CurrentStoragePriorityOf(thing) >= (int)hopperSgp.GetSlotGroup().Settings.Priority)
				{
					flag = true;
					JobFailReason.Is(TheOnlyAvailableFoodIsInStorageOfHigherPriorityTrans);
					continue;
				}
				Job job = HaulAIUtility.HaulToCellStorageJob(pawn, thing, building.Position, fitInStoreCell: true);
				if (job != null)
				{
					return job;
				}
			}
			if (!flag)
			{
				JobFailReason.Is(NoFoodToFillHopperTrans);
			}
			return null;
		}
	}
}
