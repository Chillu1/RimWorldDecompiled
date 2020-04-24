using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class TradeShip : PassingShip, ITrader, IThingHolder
	{
		public TraderKindDef def;

		private ThingOwner things;

		private List<Pawn> soldPrisoners = new List<Pawn>();

		private int randomPriceFactorSeed = -1;

		private static List<string> tmpExtantNames = new List<string>();

		public override string FullTitle => name + " (" + def.label + ")";

		public int Silver => CountHeldOf(ThingDefOf.Silver);

		public TradeCurrency TradeCurrency => def.tradeCurrency;

		public IThingHolder ParentHolder => base.Map;

		public TraderKindDef TraderKind => def;

		public int RandomPriceFactorSeed => randomPriceFactorSeed;

		public string TraderName => name;

		public bool CanTradeNow => !base.Departed;

		public float TradePriceImprovementOffsetForPlayer => 0f;

		public IEnumerable<Thing> Goods
		{
			get
			{
				for (int i = 0; i < things.Count; i++)
				{
					Pawn pawn = things[i] as Pawn;
					if (pawn == null || !soldPrisoners.Contains(pawn))
					{
						yield return things[i];
					}
				}
			}
		}

		public TradeShip()
		{
		}

		public TradeShip(TraderKindDef def, Faction faction = null)
			: base(faction)
		{
			this.def = def;
			things = new ThingOwner<Thing>(this);
			tmpExtantNames.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				tmpExtantNames.AddRange(maps[i].passingShipManager.passingShips.Select((PassingShip x) => x.name));
			}
			name = NameGenerator.GenerateName(RulePackDefOf.NamerTraderGeneral, tmpExtantNames);
			if (faction != null)
			{
				name = string.Format("{0} {1} {2}", name, "OfLower".Translate(), faction.Name);
			}
			randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
			loadID = Find.UniqueIDsManager.GetNextPassingShipID();
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			foreach (Thing item in TradeUtility.AllLaunchableThingsForTrade(base.Map, this))
			{
				yield return item;
			}
			foreach (Pawn item2 in TradeUtility.AllSellableColonyPawns(base.Map))
			{
				yield return item2;
			}
		}

		public void GenerateThings()
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.traderDef = def;
			parms.tile = base.Map.Tile;
			things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));
		}

		public override void PassingShipTick()
		{
			base.PassingShipTick();
			for (int num = things.Count - 1; num >= 0; num--)
			{
				Pawn pawn = things[num] as Pawn;
				if (pawn != null)
				{
					pawn.Tick();
					if (pawn.Dead)
					{
						things.Remove(pawn);
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref def, "def");
			Scribe_Deep.Look(ref things, "things", this);
			Scribe_Collections.Look(ref soldPrisoners, "soldPrisoners", LookMode.Reference);
			Scribe_Values.Look(ref randomPriceFactorSeed, "randomPriceFactorSeed", 0);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				soldPrisoners.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void TryOpenComms(Pawn negotiator)
		{
			if (CanTradeNow)
			{
				Find.WindowStack.Add(new Dialog_Trade(negotiator, this));
				LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(Goods.OfType<Pawn>(), "LetterRelatedPawnsTradeShip".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent);
				TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon);
			}
		}

		public override void Depart()
		{
			base.Depart();
			things.ClearAndDestroyContentsOrPassToWorld();
			soldPrisoners.Clear();
		}

		public override string GetCallLabel()
		{
			return name + " (" + def.label + ")";
		}

		protected override bool CanCommunicateWith(Pawn negotiator)
		{
			if (base.CanCommunicateWith(negotiator))
			{
				return negotiator.CanTradeWith(base.Faction, TraderKind);
			}
			return false;
		}

		public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
		{
			return HeldThingMatching(thingDef, stuffDef)?.stackCount ?? 0;
		}

		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
			Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
			if (thing2 != null)
			{
				if (!thing2.TryAbsorbStack(thing, respectStackLimit: false))
				{
					thing.Destroy();
				}
				return;
			}
			Pawn pawn = thing as Pawn;
			if (pawn != null && pawn.RaceProps.Humanlike)
			{
				soldPrisoners.Add(pawn);
			}
			things.TryAdd(thing, canMergeWithExistingStacks: false);
		}

		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				soldPrisoners.Remove(pawn);
			}
			TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(base.Map), base.Map, thing);
		}

		private Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
		{
			for (int i = 0; i < things.Count; i++)
			{
				if (things[i].def == thingDef && things[i].Stuff == stuffDef)
				{
					return things[i];
				}
			}
			return null;
		}

		public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
		{
			Thing thing = HeldThingMatching(thingDef, stuffDef);
			if (thing == null)
			{
				Log.Error("Changing count of thing trader doesn't have: " + thingDef);
			}
			thing.stackCount += count;
		}

		public override string ToString()
		{
			return FullTitle;
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return things;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}
	}
}
