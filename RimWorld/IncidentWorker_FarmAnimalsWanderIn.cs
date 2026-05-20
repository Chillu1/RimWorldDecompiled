using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IncidentWorker_FarmAnimalsWanderIn : IncidentWorker
{
	private const string FarmAnimalTradeTag = "AnimalFarm";

	private const float MaxWildness = 0.35f;

	private const float TotalBodySizeToSpawn = 2.5f;

	private static SimpleCurve BaseChanceFactorByAnimalBodySizePerCapitaCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1.5f),
		new CurvePoint(4f, 1f)
	};

	private const float SelectionChanceFactorIfExistingMatingPair = 0.5f;

	public override float ChanceFactorNow(IIncidentTarget target)
	{
		if (!ModsConfig.IdeologyActive || !IdeoUtility.AnyColonistWithRanchingIssue())
		{
			return base.ChanceFactorNow(target);
		}
		return BaseChanceFactorByAnimalBodySizePerCapitaCurve.Evaluate(PawnUtility.PlayerAnimalBodySizePerCapita()) * base.ChanceFactorNow(target);
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		Map map = (Map)parms.target;
		PawnKindDef kind;
		if (RCellFinder.TryFindRandomPawnEntryCell(out var _, map, CellFinder.EdgeRoadChance_Animal))
		{
			return TryFindRandomPawnKind(map, out kind);
		}
		return false;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Animal))
		{
			return false;
		}
		if (!TryFindRandomPawnKind(map, out var kind))
		{
			return false;
		}
		int num = Mathf.Clamp(GenMath.RoundRandom(((parms.totalBodySize > 0f) ? parms.totalBodySize : 2.5f) / kind.RaceProps.baseBodySize), 2, 10);
		if (num >= 2)
		{
			SpawnAnimal(result, map, kind, Gender.Female);
			SpawnAnimal(result, map, kind, Gender.Male);
			num -= 2;
		}
		for (int i = 0; i < num; i++)
		{
			SpawnAnimal(result, map, kind);
		}
		SendStandardLetter(parms.customLetterLabel ?? ((string)"LetterLabelFarmAnimalsWanderIn".Translate(kind.GetLabelPlural()).CapitalizeFirst()), parms.customLetterText ?? ((string)"LetterFarmAnimalsWanderIn".Translate(kind.GetLabelPlural())), LetterDefOf.PositiveEvent, parms, new TargetInfo(result, map));
		return true;
	}

	private void SpawnAnimal(IntVec3 location, Map map, PawnKindDef pawnKind, Gender? gender = null)
	{
		IntVec3 loc = CellFinder.RandomClosewalkCellNear(location, map, 12);
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, null, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, gender));
		GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
		pawn.SetFaction(Faction.OfPlayer);
	}

	private bool TryFindRandomPawnKind(Map map, out PawnKindDef kind)
	{
		return DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef x) => x.RaceProps.Animal && x.race.GetStatValueAbstract(StatDefOf.Wildness) < 0.35f && map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(x.race) && !x.race.tradeTags.NullOrEmpty() && x.race.tradeTags.Contains("AnimalFarm") && !x.RaceProps.Dryad && !x.RaceProps.neverIncludeInQuests).TryRandomElementByWeight((PawnKindDef k) => SelectionChance(k), out kind);
	}

	private float SelectionChance(PawnKindDef pawnKind)
	{
		float num = 0.42000002f - pawnKind.race.GetStatValueAbstract(StatDefOf.Wildness);
		if (PawnUtility.PlayerHasReproductivePair(pawnKind))
		{
			num *= 0.5f;
		}
		return num;
	}
}
