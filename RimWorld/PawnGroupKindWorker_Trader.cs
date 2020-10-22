using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnGroupKindWorker_Trader : PawnGroupKindWorker
	{
		public override float MinPointsToGenerateAnything(PawnGroupMaker groupMaker)
		{
			return 0f;
		}

		public override bool CanGenerateFrom(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
		{
			if (base.CanGenerateFrom(parms, groupMaker) && groupMaker.traders.Any())
			{
				if (parms.tile != -1)
				{
					return groupMaker.carriers.Any((PawnGenOption x) => Find.WorldGrid[parms.tile].biome.IsPackAnimalAllowed(x.kind.race));
				}
				return true;
			}
			return false;
		}

		protected override void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
		{
			if (!CanGenerateFrom(parms, groupMaker))
			{
				if (errorOnZeroResults)
				{
					Log.Error(string.Concat("Cannot generate trader caravan for ", parms.faction, "."));
				}
				return;
			}
			if (!parms.faction.def.caravanTraderKinds.Any())
			{
				Log.Error(string.Concat("Cannot generate trader caravan for ", parms.faction, " because it has no trader kinds."));
				return;
			}
			PawnGenOption pawnGenOption = groupMaker.traders.FirstOrDefault((PawnGenOption x) => !x.kind.trader);
			if (pawnGenOption != null)
			{
				Log.Error(string.Concat("Cannot generate arriving trader caravan for ", parms.faction, " because there is a pawn kind (") + pawnGenOption.kind.LabelCap + ") who is not a trader but is in a traders list.");
				return;
			}
			PawnGenOption pawnGenOption2 = groupMaker.carriers.FirstOrDefault((PawnGenOption x) => !x.kind.RaceProps.packAnimal);
			if (pawnGenOption2 != null)
			{
				Log.Error(string.Concat("Cannot generate arriving trader caravan for ", parms.faction, " because there is a pawn kind (") + pawnGenOption2.kind.LabelCap + ") who is not a carrier but is in a carriers list.");
				return;
			}
			if (parms.seed.HasValue)
			{
				Log.Warning("Deterministic seed not implemented for this pawn group kind worker. The result will be random anyway.");
			}
			TraderKindDef traderKindDef = ((parms.traderKind != null) ? parms.traderKind : parms.faction.def.caravanTraderKinds.RandomElementByWeight((TraderKindDef traderDef) => traderDef.CalculatedCommonality));
			Pawn pawn = GenerateTrader(parms, groupMaker, traderKindDef);
			outPawns.Add(pawn);
			ThingSetMakerParams parms2 = default(ThingSetMakerParams);
			parms2.traderDef = traderKindDef;
			parms2.tile = parms.tile;
			parms2.makingFaction = parms.faction;
			List<Thing> wares = ThingSetMakerDefOf.TraderStock.root.Generate(parms2).InRandomOrder().ToList();
			foreach (Pawn slavesAndAnimalsFromWare in GetSlavesAndAnimalsFromWares(parms, pawn, wares))
			{
				outPawns.Add(slavesAndAnimalsFromWare);
			}
			GenerateCarriers(parms, groupMaker, pawn, wares, outPawns);
			GenerateGuards(parms, groupMaker, pawn, wares, outPawns);
		}

		private Pawn GenerateTrader(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, TraderKindDef traderKind)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(groupMaker.traders.RandomElementByWeight((PawnGenOption x) => x.selectionWeight).kind, parms.faction, PawnGenerationContext.NonPlayer, parms.tile, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, parms.inhabitants));
			pawn.mindState.wantsToTradeWithColony = true;
			PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
			pawn.trader.traderKind = traderKind;
			parms.points -= pawn.kindDef.combatPower;
			return pawn;
		}

		private void GenerateCarriers(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, Pawn trader, List<Thing> wares, List<Pawn> outPawns)
		{
			List<Thing> list = wares.Where((Thing x) => !(x is Pawn)).ToList();
			int i = 0;
			int num = Mathf.CeilToInt((float)list.Count / 8f);
			PawnKindDef kind = groupMaker.carriers.Where((PawnGenOption x) => parms.tile == -1 || Find.WorldGrid[parms.tile].biome.IsPackAnimalAllowed(x.kind.race)).RandomElementByWeight((PawnGenOption x) => x.selectionWeight).kind;
			List<Pawn> list2 = new List<Pawn>();
			for (int j = 0; j < num; j++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, parms.faction, PawnGenerationContext.NonPlayer, parms.tile, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, parms.inhabitants));
				if (i < list.Count)
				{
					pawn.inventory.innerContainer.TryAdd(list[i]);
					i++;
				}
				list2.Add(pawn);
				outPawns.Add(pawn);
			}
			for (; i < list.Count; i++)
			{
				list2.RandomElement().inventory.innerContainer.TryAdd(list[i]);
			}
		}

		private IEnumerable<Pawn> GetSlavesAndAnimalsFromWares(PawnGroupMakerParms parms, Pawn trader, List<Thing> wares)
		{
			for (int i = 0; i < wares.Count; i++)
			{
				Pawn pawn = wares[i] as Pawn;
				if (pawn != null)
				{
					if (pawn.Faction != parms.faction)
					{
						pawn.SetFaction(parms.faction);
					}
					yield return pawn;
				}
			}
		}

		private void GenerateGuards(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, Pawn trader, List<Thing> wares, List<Pawn> outPawns)
		{
			if (!groupMaker.guards.Any())
			{
				return;
			}
			foreach (PawnGenOption item2 in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.guards, parms))
			{
				PawnGenerationRequest request = PawnGenerationRequest.MakeDefault();
				request.KindDef = item2.kind;
				request.Faction = parms.faction;
				request.Tile = parms.tile;
				request.MustBeCapableOfViolence = true;
				request.Inhabitant = parms.inhabitants;
				request.RedressValidator = (Pawn x) => x.royalty == null || !x.royalty.AllTitlesForReading.Any();
				Pawn item = PawnGenerator.GeneratePawn(request);
				outPawns.Add(item);
			}
		}

		public override IEnumerable<PawnKindDef> GeneratePawnKindsExample(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
		{
			throw new NotImplementedException();
		}
	}
}
