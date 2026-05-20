using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PawnGroupKindWorker_Trader : PawnGroupKindWorker
{
	public override float MinPointsToGenerateAnything(PawnGroupMaker groupMaker, FactionDef faction, PawnGroupMakerParms parms = null)
	{
		return 0f;
	}

	public override bool CanGenerateFrom(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
	{
		if (base.CanGenerateFrom(parms, groupMaker) && groupMaker.traders.Any())
		{
			if (parms.tile.Valid)
			{
				return groupMaker.carriers.Any((PawnGenOption x) => Find.WorldGrid[parms.tile].PrimaryBiome.IsPackAnimalAllowed(x.kind.race));
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
				Log.Error("Cannot generate trader caravan for " + parms.faction?.ToString() + ".");
			}
			return;
		}
		if (!parms.faction.def.caravanTraderKinds.Any())
		{
			Log.Error("Cannot generate trader caravan for " + parms.faction?.ToString() + " because it has no trader kinds.");
			return;
		}
		PawnGenOption pawnGenOption = groupMaker.traders.FirstOrDefault((PawnGenOption x) => !x.kind.trader);
		if (pawnGenOption != null)
		{
			Log.Error("Cannot generate arriving trader caravan for " + parms.faction?.ToString() + " because there is a pawn kind (" + pawnGenOption.kind.LabelCap + ") who is not a trader but is in a traders list.");
			return;
		}
		PawnGenOption pawnGenOption2 = groupMaker.carriers.FirstOrDefault((PawnGenOption x) => !x.kind.RaceProps.packAnimal);
		if (pawnGenOption2 != null)
		{
			Log.Error("Cannot generate arriving trader caravan for " + parms.faction?.ToString() + " because there is a pawn kind (" + pawnGenOption2.kind.LabelCap + ") who is not a carrier but is in a carriers list.");
			return;
		}
		if (parms.seed.HasValue)
		{
			Log.Warning("Deterministic seed not implemented for this pawn group kind worker. The result will be random anyway.");
		}
		TraderKindDef traderKindDef = ((parms.traderKind != null) ? parms.traderKind : parms.faction.def.caravanTraderKinds.RandomElementByWeight((TraderKindDef traderDef) => traderDef.CalculatedCommonality));
		Pawn pawn = GenerateTrader(parms, groupMaker, traderKindDef);
		outPawns.Add(pawn);
		ThingSetMakerParams parms2 = new ThingSetMakerParams
		{
			traderDef = traderKindDef,
			tile = parms.tile,
			makingFaction = parms.faction
		};
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
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(groupMaker.traders.RandomElementByWeight((PawnGenOption x) => x.selectionWeight).kind, parms.faction, PawnGenerationContext.NonPlayer, parms.tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, fixedIdeo: parms.ideo, inhabitant: parms.inhabitants));
		pawn.mindState.wantsToTradeWithColony = true;
		PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, actAsIfSpawned: true);
		pawn.trader.traderKind = traderKind;
		parms.points -= pawn.kindDef.combatPower;
		TradeUtility.CheckGiveTraderQuest(pawn);
		return pawn;
	}

	private void GenerateCarriers(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, Pawn trader, List<Thing> wares, List<Pawn> outPawns)
	{
		List<Thing> list = wares.Where((Thing x) => !(x is Pawn)).ToList();
		int num = 0;
		int num2 = Mathf.CeilToInt((float)list.Count / 8f);
		PawnKindDef kind = groupMaker.carriers.Where((PawnGenOption x) => !parms.tile.Valid || Find.WorldGrid[parms.tile].PrimaryBiome.IsPackAnimalAllowed(x.kind.race)).RandomElementByWeight((PawnGenOption x) => x.selectionWeight).kind;
		List<Pawn> list2 = new List<Pawn>();
		for (int num3 = 0; num3 < num2; num3++)
		{
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, parms.faction, PawnGenerationContext.NonPlayer, parms.tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, fixedIdeo: parms.ideo, inhabitant: parms.inhabitants));
			if (num < list.Count)
			{
				pawn.inventory.innerContainer.TryAdd(list[num]);
				num++;
			}
			list2.Add(pawn);
			outPawns.Add(pawn);
		}
		for (; num < list.Count; num++)
		{
			list2.RandomElement().inventory.innerContainer.TryAdd(list[num]);
		}
	}

	private IEnumerable<Pawn> GetSlavesAndAnimalsFromWares(PawnGroupMakerParms parms, Pawn trader, List<Thing> wares)
	{
		for (int i = 0; i < wares.Count; i++)
		{
			if (wares[i] is Pawn pawn)
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
		foreach (PawnGenOptionWithXenotype item2 in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.guards, parms))
		{
			PawnKindDef kind = item2.Option.kind;
			Faction faction = parms.faction;
			XenotypeDef xenotype = item2.Xenotype;
			PlanetTile? tile = parms.tile;
			bool inhabitants = parms.inhabitants;
			Ideo ideo = parms.ideo;
			Pawn item = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitants, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, ideo, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, xenotype));
			outPawns.Add(item);
		}
	}

	public override IEnumerable<PawnKindDef> GeneratePawnKindsExample(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
	{
		throw new NotImplementedException();
	}
}
