using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class Pawn_InventoryTracker : IThingHolder, IExposable
	{
		public Pawn pawn;

		public ThingOwner<Thing> innerContainer;

		private bool unloadEverything;

		private List<Thing> itemsNotForSale = new List<Thing>();

		private static List<ThingDefCount> tmpDrugsToKeep = new List<ThingDefCount>();

		private static List<Thing> tmpThingList = new List<Thing>();

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

		private bool HasAnyUnloadableThing => FirstUnloadableThing != default(ThingCount);

		public ThingCount FirstUnloadableThing
		{
			get
			{
				if (innerContainer.Count == 0)
				{
					return default(ThingCount);
				}
				if (pawn.drugs != null && pawn.drugs.CurrentPolicy != null)
				{
					DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
					tmpDrugsToKeep.Clear();
					for (int i = 0; i < currentPolicy.Count; i++)
					{
						if (currentPolicy[i].takeToInventory > 0)
						{
							tmpDrugsToKeep.Add(new ThingDefCount(currentPolicy[i].drug, currentPolicy[i].takeToInventory));
						}
					}
					for (int j = 0; j < innerContainer.Count; j++)
					{
						if (!innerContainer[j].def.IsDrug)
						{
							return new ThingCount(innerContainer[j], innerContainer[j].stackCount);
						}
						int num = -1;
						for (int k = 0; k < tmpDrugsToKeep.Count; k++)
						{
							if (innerContainer[j].def == tmpDrugsToKeep[k].ThingDef)
							{
								num = k;
								break;
							}
						}
						if (num < 0)
						{
							return new ThingCount(innerContainer[j], innerContainer[j].stackCount);
						}
						if (innerContainer[j].stackCount > tmpDrugsToKeep[num].Count)
						{
							return new ThingCount(innerContainer[j], innerContainer[j].stackCount - tmpDrugsToKeep[num].Count);
						}
						tmpDrugsToKeep[num] = new ThingDefCount(tmpDrugsToKeep[num].ThingDef, tmpDrugsToKeep[num].Count - innerContainer[j].stackCount);
					}
					return default(ThingCount);
				}
				return new ThingCount(innerContainer[0], innerContainer[0].stackCount);
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
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref unloadEverything, "unloadEverything", defaultValue: false);
		}

		public void InventoryTrackerTick()
		{
			innerContainer.ThingOwnerTick();
			if (unloadEverything && !HasAnyUnloadableThing)
			{
				unloadEverything = false;
			}
		}

		public void InventoryTrackerTickRare()
		{
			innerContainer.ThingOwnerTickRare();
		}

		public void DropAllNearPawn(IntVec3 pos, bool forbid = false, bool unforbid = false)
		{
			if (pawn.MapHeld == null)
			{
				Log.Error("Tried to drop all inventory near pawn but the pawn is unspawned. pawn=" + pawn);
				return;
			}
			tmpThingList.Clear();
			tmpThingList.AddRange(innerContainer);
			for (int i = 0; i < tmpThingList.Count; i++)
			{
				innerContainer.TryDrop(tmpThingList[i], pos, pawn.MapHeld, ThingPlaceMode.Near, out Thing _, delegate(Thing t, int unused)
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
	}
}
