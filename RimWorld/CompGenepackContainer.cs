using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompGenepackContainer : ThingComp, IThingHolder, ISearchableContents
	{
		public ThingOwner innerContainer;

		public List<Thing> leftToLoad = new List<Thing>();

		public bool autoLoad = true;

		[Unsaved(false)]
		private List<Genepack> tmpGenepacks = new List<Genepack>();

		private static readonly CachedTexture EjectTex = new CachedTexture("UI/Gizmos/EjectAll");

		public CompProperties_GenepackContainer Props => (CompProperties_GenepackContainer)props;

		public bool StorageTabVisible => true;

		public bool PowerOn => parent.TryGetComp<CompPowerTrader>().PowerOn;

		public bool Full => ContainedGenepacks.Count >= Props.maxCapacity;

		public bool CanLoadMore
		{
			get
			{
				if (!Full)
				{
					return ContainedGenepacks.Count + leftToLoad.Count < Props.maxCapacity;
				}
				return false;
			}
		}

		public ThingOwner SearchableContents => innerContainer;

		public List<Genepack> ContainedGenepacks
		{
			get
			{
				tmpGenepacks.Clear();
				for (int i = 0; i < innerContainer.Count; i++)
				{
					if (innerContainer[i] is Genepack genepack && genepack.GeneSet.GenesListForReading.Any())
					{
						tmpGenepacks.Add(genepack);
					}
				}
				return tmpGenepacks;
			}
		}

		public override void PostPostMake()
		{
			if (!ModLister.CheckBiotech("Genepack container"))
			{
				parent.Destroy();
				return;
			}
			base.PostPostMake();
			innerContainer = new ThingOwner<Thing>(this);
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			innerContainer.ClearAndDestroyContents();
			base.PostDestroy(mode, previousMap);
		}

		public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode != DestroyMode.WillReplace)
			{
				EjectContents(map);
			}
			for (int i = 0; i < leftToLoad.Count; i++)
			{
				((Genepack)leftToLoad[i]).targetContainer = null;
			}
			leftToLoad.Clear();
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			for (int i = 0; i < leftToLoad.Count; i++)
			{
				if (leftToLoad[i].Map == parent.Map)
				{
					GenDraw.DrawLineBetween(parent.DrawPos, leftToLoad[i].DrawPos);
				}
			}
		}

		public void EjectContents(Map destMap = null)
		{
			if (destMap == null)
			{
				destMap = parent.Map;
			}
			IntVec3 dropLoc = (parent.def.hasInteractionCell ? parent.InteractionCell : parent.Position);
			innerContainer.TryDropAll(dropLoc, destMap, ThingPlaceMode.Near);
		}

		public override void CompTickRare()
		{
			if (innerContainer != null)
			{
				for (int i = 0; i < innerContainer.Count; i++)
				{
					innerContainer[i].TickRare();
				}
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (parent.Faction == Faction.OfPlayer && innerContainer.Any)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "EjectAll".Translate();
				command_Action.defaultDesc = "EjectAllDesc".Translate();
				command_Action.icon = EjectTex.Texture;
				command_Action.action = delegate
				{
					EjectContents(parent.Map);
				};
				yield return command_Action;
			}
			if (!DebugSettings.ShowDevGizmos)
			{
				yield break;
			}
			yield return new Command_Action
			{
				defaultLabel = "DEV: Fill with new packs",
				action = delegate
				{
					innerContainer.ClearAndDestroyContents();
					for (int i = 0; i < Props.maxCapacity; i++)
					{
						innerContainer.TryAdd(ThingMaker.MakeThing(ThingDefOf.Genepack));
					}
				}
			};
		}

		public override string CompInspectStringExtra()
		{
			return "GenepacksStored".Translate() + $": {innerContainer.Count} / {Props.maxCapacity}\n" + "CasketContains".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Collections.Look(ref leftToLoad, "leftToLoad", LookMode.Reference);
			Scribe_Values.Look(ref autoLoad, "autoLoad", defaultValue: true);
		}
	}
}
