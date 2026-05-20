using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class PawnGroupKindWorker_Normal : PawnGroupKindWorker
{
	public override float MinPointsToGenerateAnything(PawnGroupMaker groupMaker, FactionDef faction, PawnGroupMakerParms parms = null)
	{
		float num = ((parms != null && parms.points > 0f) ? parms.points : 100000f);
		float num2 = float.MaxValue;
		List<PawnGenOptionWithXenotype> options = PawnGroupMakerUtility.GetOptions(parms, faction, groupMaker.options, num, num, float.MaxValue);
		foreach (PawnGenOptionWithXenotype item in options)
		{
			if (item.Option.kind.isFighter && item.Cost < num2 && PawnGroupMakerUtility.PawnGenOptionValid(item.Option, parms))
			{
				num2 = item.Cost;
			}
		}
		if (num2 == float.MaxValue)
		{
			foreach (PawnGenOptionWithXenotype item2 in options)
			{
				if (item2.Cost < num2 && PawnGroupMakerUtility.PawnGenOptionValid(item2.Option, parms))
				{
					num2 = item2.Cost;
				}
			}
		}
		return num2;
	}

	public override bool CanGenerateFrom(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
	{
		if (!base.CanGenerateFrom(parms, groupMaker))
		{
			return false;
		}
		if (!PawnGroupMakerUtility.AnyOptions(parms, parms.faction?.def, groupMaker.options, parms.points))
		{
			return false;
		}
		return true;
	}

	protected override void GeneratePawns(PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
	{
		if (!CanGenerateFrom(parms, groupMaker))
		{
			if (errorOnZeroResults)
			{
				Log.Error("Cannot generate pawns for " + parms.faction?.ToString() + " with " + parms.points + ". Defaulting to a single random cheap group.");
			}
			return;
		}
		bool flag = parms.raidStrategy == null || parms.raidStrategy.pawnsCanBringFood || (parms.faction != null && !parms.faction.HostileTo(Faction.OfPlayer));
		Predicate<Pawn> predicate = ((parms.raidStrategy != null) ? ((Predicate<Pawn>)((Pawn p) => parms.raidStrategy.Worker.CanUsePawn(parms.points, p, outPawns))) : null);
		bool flag2 = false;
		foreach (PawnGenOptionWithXenotype item in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.options, parms))
		{
			PawnKindDef kind = item.Option.kind;
			Faction faction = parms.faction;
			Ideo ideo = parms.ideo;
			XenotypeDef xenotype = item.Xenotype;
			PlanetTile? tile = parms.tile;
			bool allowFood = flag;
			bool inhabitants = parms.inhabitants;
			Predicate<Pawn> validatorPostGear = predicate;
			PawnGenerationRequest request = new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: false, allowDead: false, parms.faction.deactivated, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood, allowAddictions: true, inhabitants, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, validatorPostGear, null, null, null, null, null, null, null, null, null, ideo, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, xenotype);
			if (parms.raidAgeRestriction != null && parms.raidAgeRestriction.Worker.ShouldApplyToKind(item.Option.kind))
			{
				request.BiologicalAgeRange = parms.raidAgeRestriction.ageRange;
				request.AllowedDevelopmentalStages = parms.raidAgeRestriction.developmentStage;
			}
			if (item.Option.kind.pawnGroupDevelopmentStage.HasValue)
			{
				request.AllowedDevelopmentalStages = item.Option.kind.pawnGroupDevelopmentStage.Value;
			}
			if (!Find.Storyteller.difficulty.ChildRaidersAllowed && parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer))
			{
				request.AllowedDevelopmentalStages = DevelopmentalStage.Adult;
			}
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			if (parms.forceOneDowned && !flag2)
			{
				pawn.health.forceDowned = true;
				if (pawn.guest != null)
				{
					pawn.guest.Recruitable = true;
				}
				pawn.mindState.canFleeIndividual = false;
				flag2 = true;
			}
			outPawns.Add(pawn);
		}
	}

	public override IEnumerable<PawnKindDef> GeneratePawnKindsExample(PawnGroupMakerParms parms, PawnGroupMaker groupMaker)
	{
		foreach (PawnGenOptionWithXenotype item in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.options, parms))
		{
			yield return item.Option.kind;
		}
	}
}
