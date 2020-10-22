using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class Pawn_InventoryTracker : IThingHolder, IExposable
	{
		public Pawn pawn;

		public ThingOwner<Thing> innerContainer;

		private bool unloadEverything;

		private List<Thing> itemsNotForSale = new List<Thing>();

		public static readonly Texture2D DrugTex = ContentFinder<Texture2D>.Get("UI/Commands/TakeDrug");

		private static List<ThingDefCount> tmpDrugsToKeep = new List<ThingDefCount>();

		private static List<Thing> tmpThingList = new List<Thing>();

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
			if (!pawn.IsColonistPlayerControlled || !pawn.Drafted || Find.Selector.SingleSelectedThing != pawn || pawn.IsTeetotaler())
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
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "ConsumeThing".Translate(drug.LabelNoCount, drug);
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
					list.Add(new FloatMenuOption("ConsumeThing".Translate(drug2.LabelNoCount, drug2), delegate
					{
						FoodUtility.IngestFromInventoryNow(pawn, drug2);
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			};
			yield return command_Action2;
		}
	}
}
