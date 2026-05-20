using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public class Pawn_InventoryTracker : IThingHolder, IExposable
{
	public Pawn pawn;

	public ThingOwner<Thing> innerContainer;

	private bool unloadEverything;

	private List<Thing> itemsNotForSale = new List<Thing>();

	private List<Thing> unpackedCaravanItems = new List<Thing>();

	public static readonly Texture2D DrugTex = ContentFinder<Texture2D>.Get("UI/Commands/TakeDrug");

	private static List<ThingDefCount> tmpItemsToKeep = new List<ThingDefCount>();

	private static readonly List<Thing> tmpThingList = new List<Thing>();

	private List<Thing> usableDrugsTmp = new List<Thing>();

	public bool UnloadEverything
	{
		get
		{
			if (unloadEverything)
			{
				return HasAnyUnloadableThing;
			}
			return false;
		}
		set
		{
			if (value && HasAnyUnloadableThing)
			{
				unloadEverything = true;
			}
			else
			{
				unloadEverything = false;
			}
		}
	}

	public bool HasAnyUnpackedCaravanItems => unpackedCaravanItems.Count > 0;

	private bool HasAnyUnloadableThing => FirstUnloadableThing != default(ThingCount);

	public ThingCount FirstUnloadableThing
	{
		get
		{
			if (innerContainer.Count == 0)
			{
				return default(ThingCount);
			}
			if (pawn.drugs?.CurrentPolicy != null)
			{
				DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
				tmpItemsToKeep.Clear();
				for (int i = 0; i < currentPolicy.Count; i++)
				{
					if (currentPolicy[i].takeToInventory > 0)
					{
						tmpItemsToKeep.Add(new ThingDefCount(currentPolicy[i].drug, currentPolicy[i].takeToInventory));
					}
				}
			}
			Pawn_InventoryStockTracker inventoryStock = pawn.inventoryStock;
			if (inventoryStock != null && inventoryStock.stockEntries?.Count > 0)
			{
				foreach (InventoryStockEntry value in pawn.inventoryStock.stockEntries.Values)
				{
					tmpItemsToKeep.Add(new ThingDefCount(value.thingDef, value.count));
				}
			}
			foreach (Thing item in innerContainer)
			{
				int num = -1;
				for (int j = 0; j < tmpItemsToKeep.Count; j++)
				{
					if (item.def == tmpItemsToKeep[j].ThingDef)
					{
						num = j;
						break;
					}
				}
				if (pawn.IsColonist && item.def.IsNutritionGivingIngestible && !item.def.IsDrug && JobGiver_PackFood.IsGoodPackableFoodFor(item, pawn, checkMass: false))
				{
					float inventoryPackableFoodNutrition = JobGiver_PackFood.GetInventoryPackableFoodNutrition(pawn);
					float maxLevel = pawn.needs.food.MaxLevel;
					if (inventoryPackableFoodNutrition - item.GetStatValue(StatDefOf.Nutrition) * (float)item.stackCount <= maxLevel)
					{
						int k;
						for (k = 0; inventoryPackableFoodNutrition - item.GetStatValue(StatDefOf.Nutrition) * (float)k > maxLevel; k++)
						{
						}
						if (item.stackCount - k > 0)
						{
							tmpItemsToKeep.Add(new ThingDefCount(item.def, item.stackCount - k));
							num = tmpItemsToKeep.Count - 1;
						}
					}
				}
				if (num < 0)
				{
					return new ThingCount(item, item.stackCount);
				}
				if (item.stackCount > tmpItemsToKeep[num].Count)
				{
					return new ThingCount(item, item.stackCount - tmpItemsToKeep[num].Count);
				}
				tmpItemsToKeep[num] = new ThingDefCount(tmpItemsToKeep[num].ThingDef, tmpItemsToKeep[num].Count - item.stackCount);
			}
			return default(ThingCount);
		}
	}

	public IThingHolder ParentHolder => pawn;

	public Pawn_InventoryTracker(Pawn pawn)
	{
		this.pawn = pawn;
		innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref itemsNotForSale, "itemsNotForSale", LookMode.Reference);
		Scribe_Collections.Look(ref unpackedCaravanItems, "unpackedCaravanItems", LookMode.Reference);
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Values.Look(ref unloadEverything, "unloadEverything", defaultValue: false);
	}

	public void InventoryTrackerTick()
	{
		if (unloadEverything && !HasAnyUnloadableThing)
		{
			unloadEverything = false;
		}
	}

	public void DropAllNearPawn(IntVec3 pos, bool forbid = false, bool unforbid = false)
	{
		DropAllNearPawnHelper(pos, forbid, unforbid);
	}

	private void DropAllNearPawnHelper(IntVec3 pos, bool forbid = false, bool unforbid = false, bool caravanHaulOnly = false)
	{
		if (pawn.MapHeld == null)
		{
			Log.Error("Tried to drop all inventory near pawn but the pawn is unspawned. pawn=" + pawn);
			return;
		}
		tmpThingList.Clear();
		if (caravanHaulOnly)
		{
			tmpThingList.AddRange(unpackedCaravanItems);
		}
		else
		{
			tmpThingList.AddRange(innerContainer);
		}
		int i;
		for (i = 0; i < tmpThingList.Count; i++)
		{
			if (caravanHaulOnly && !innerContainer.Contains(tmpThingList[i]))
			{
				unpackedCaravanItems.Remove(tmpThingList[i]);
				Log.Warning("Could not drop unpacked caravan item " + tmpThingList[i].Label + ", inventory no longer contains it");
				continue;
			}
			innerContainer.TryDrop(tmpThingList[i], pos, pawn.MapHeld, ThingPlaceMode.Near, out var _, delegate(Thing t, int unused)
			{
				if (forbid)
				{
					t.SetForbiddenIfOutsideHomeArea();
				}
				if (unforbid)
				{
					t.SetForbidden(value: false, warnOnFail: false);
				}
				if (t.def.IsPleasureDrug)
				{
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.DrugBurning, OpportunityType.Important);
				}
				LordJob_FormAndSendCaravan lordJob_FormAndSendCaravan = CaravanFormingUtility.GetFormAndSendCaravanLord(pawn)?.LordJob as LordJob_FormAndSendCaravan;
				if (caravanHaulOnly && lordJob_FormAndSendCaravan != null && lordJob_FormAndSendCaravan.GatheringItemsNow)
				{
					CaravanFormingUtility.TryAddItemBackToTransferables(t, lordJob_FormAndSendCaravan.transferables, tmpThingList[i].stackCount);
				}
				unpackedCaravanItems.Remove(tmpThingList[i]);
			});
		}
	}

	public void DropCount(ThingDef def, int count, bool forbid = false, bool unforbid = false)
	{
		if (pawn.MapHeld == null)
		{
			Log.Error("Tried to drop a thing near pawn but the pawn is unspawned. pawn=" + pawn);
			return;
		}
		tmpThingList.Clear();
		tmpThingList.AddRange(innerContainer);
		int num = 0;
		for (int i = 0; i < tmpThingList.Count; i++)
		{
			Thing thing = tmpThingList[i];
			if (thing.def != def)
			{
				continue;
			}
			int num2 = Math.Min(thing.stackCount, count);
			innerContainer.TryDrop(tmpThingList[i], pawn.Position, pawn.MapHeld, ThingPlaceMode.Near, num2, out var _, delegate(Thing t, int unused)
			{
				if (forbid)
				{
					t.SetForbiddenIfOutsideHomeArea();
				}
				if (unforbid)
				{
					t.SetForbidden(value: false, warnOnFail: false);
				}
				if (t.def.IsPleasureDrug)
				{
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.DrugBurning, OpportunityType.Important);
				}
			});
			num += num2;
			if (num >= count)
			{
				break;
			}
		}
	}

	public void RemoveCount(ThingDef def, int count, bool destroy = true)
	{
		tmpThingList.Clear();
		tmpThingList.AddRange(innerContainer);
		foreach (Thing tmpThing in tmpThingList)
		{
			if (tmpThing.def != def)
			{
				continue;
			}
			if (tmpThing.stackCount > count)
			{
				tmpThing.stackCount -= count;
				break;
			}
			innerContainer.Remove(tmpThing);
			if (destroy)
			{
				tmpThing.Destroy();
			}
			break;
		}
	}

	public void DestroyAll(DestroyMode mode = DestroyMode.Vanish)
	{
		innerContainer.ClearAndDestroyContents(mode);
	}

	public bool Contains(Thing item)
	{
		return innerContainer.Contains(item);
	}

	public int Count(ThingDef def)
	{
		int num = 0;
		foreach (Thing item in innerContainer)
		{
			if (item.def == def)
			{
				num += item.stackCount;
			}
		}
		return num;
	}

	public int Count(Func<Thing, bool> validator)
	{
		int num = 0;
		foreach (Thing item in innerContainer)
		{
			if (validator(item))
			{
				num += item.stackCount;
			}
		}
		return num;
	}

	public void AddHauledCaravanItem(Thing item)
	{
		if (pawn.carryTracker.innerContainer.TryTransferToContainer(item, innerContainer, item.stackCount, out var resultingTransferredItem, canMergeWithExistingStacks: false) > 0)
		{
			unpackedCaravanItems.Add(resultingTransferredItem);
		}
		CompForbiddable compForbiddable = resultingTransferredItem?.TryGetComp<CompForbiddable>();
		if (compForbiddable != null)
		{
			compForbiddable.Forbidden = false;
		}
	}

	public void TryAddAndUnforbid(Thing item)
	{
		CompForbiddable compForbiddable = item.TryGetComp<CompForbiddable>();
		if (innerContainer.TryAdd(item) && compForbiddable != null)
		{
			compForbiddable.Forbidden = false;
		}
	}

	public void TransferCaravanItemsToCarrier(Pawn_InventoryTracker carrierInventory)
	{
		List<Thing> list = new List<Thing>();
		list.AddRange(pawn.inventory.unpackedCaravanItems);
		foreach (Thing item in list)
		{
			if (MassUtility.IsOverEncumbered(carrierInventory.pawn))
			{
				break;
			}
			if (innerContainer.Contains(item))
			{
				pawn.inventory.innerContainer.TryTransferToContainer(item, carrierInventory.innerContainer, item.stackCount);
			}
			unpackedCaravanItems.Remove(item);
		}
	}

	public void DropAllPackingCaravanThings()
	{
		if (pawn.Spawned)
		{
			DropAllNearPawnHelper(pawn.Position, forbid: false, unforbid: false, caravanHaulOnly: true);
			ClearHaulingCaravanCache();
		}
	}

	public void ClearHaulingCaravanCache()
	{
		unpackedCaravanItems.Clear();
	}

	public bool NotForSale(Thing item)
	{
		return itemsNotForSale.Contains(item);
	}

	public void TryAddItemNotForSale(Thing item)
	{
		if (innerContainer.TryAdd(item, canMergeWithExistingStacks: false))
		{
			itemsNotForSale.Add(item);
		}
	}

	public void Notify_ItemRemoved(Thing item)
	{
		itemsNotForSale.Remove(item);
		unpackedCaravanItems.Remove(item);
		if (unloadEverything && !HasAnyUnloadableThing)
		{
			unloadEverything = false;
		}
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public IEnumerable<Thing> GetDrugs()
	{
		foreach (Thing item in innerContainer)
		{
			if (item.TryGetComp<CompDrug>() != null)
			{
				yield return item;
			}
		}
	}

	public IEnumerable<Thing> GetCombatEnhancingDrugs()
	{
		foreach (Thing item in innerContainer)
		{
			CompDrug compDrug = item.TryGetComp<CompDrug>();
			if (compDrug != null && compDrug.Props.isCombatEnhancingDrug)
			{
				yield return item;
			}
		}
	}

	public Thing FindCombatEnhancingDrug()
	{
		return GetCombatEnhancingDrugs().FirstOrDefault();
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		if (!pawn.IsColonistPlayerControlled || !pawn.Drafted || Find.Selector.SingleSelectedThing != pawn)
		{
			yield break;
		}
		usableDrugsTmp.Clear();
		foreach (Thing drug3 in GetDrugs())
		{
			if (FoodUtility.WillIngestFromInventoryNow(pawn, drug3))
			{
				usableDrugsTmp.Add(drug3);
			}
		}
		if (usableDrugsTmp.Count == 0)
		{
			yield break;
		}
		if (usableDrugsTmp.Count == 1)
		{
			Thing drug = usableDrugsTmp[0];
			string defaultLabel = (drug.def.ingestible.ingestCommandString.NullOrEmpty() ? "ConsumeThing".Translate(drug.LabelNoCount, drug) : drug.def.ingestible.ingestCommandString.Formatted(drug.LabelShort));
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = defaultLabel;
			command_Action.defaultDesc = drug.LabelCapNoCount + ": " + drug.def.description.CapitalizeFirst();
			command_Action.icon = drug.def.uiIcon;
			command_Action.iconAngle = drug.def.uiIconAngle;
			command_Action.iconOffset = drug.def.uiIconOffset;
			command_Action.action = delegate
			{
				FoodUtility.IngestFromInventoryNow(pawn, drug);
			};
			yield return command_Action;
			yield break;
		}
		Command_Action command_Action2 = new Command_Action();
		command_Action2.defaultLabel = "TakeDrug".Translate();
		command_Action2.defaultDesc = "TakeDrugDesc".Translate();
		command_Action2.icon = DrugTex;
		command_Action2.action = delegate
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (Thing drug2 in usableDrugsTmp)
			{
				string label = (drug2.def.ingestible.ingestCommandString.NullOrEmpty() ? "ConsumeThing".Translate(drug2.LabelNoCount, drug2) : drug2.def.ingestible.ingestCommandString.Formatted(drug2.LabelShort));
				list.Add(new FloatMenuOption(label, delegate
				{
					FoodUtility.IngestFromInventoryNow(pawn, drug2);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		};
		yield return command_Action2;
	}
}
