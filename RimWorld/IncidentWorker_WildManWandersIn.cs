using Verse;

namespace RimWorld;

public class IncidentWorker_WildManWandersIn : IncidentWorker
{
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		if (!TryFindFormerFaction(out var _))
		{
			return false;
		}
		Map map = (Map)parms.target;
		if (map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
		{
			return false;
		}
		if (ModsConfig.BiotechActive && map.GameConditionManager.ConditionIsActive(GameConditionDefOf.NoxiousHaze))
		{
			return false;
		}
		if (!map.mapTemperature.SeasonAcceptableFor(ThingDefOf.Human))
		{
			return false;
		}
		IntVec3 cell;
		return TryFindEntryCell(map, out cell);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!TryFindEntryCell(map, out var cell))
		{
			return false;
		}
		if (!TryFindFormerFaction(out var formerFaction))
		{
			return false;
		}
		DevelopmentalStage developmentalStage = (Find.Storyteller.difficulty.ChildrenAllowed ? (DevelopmentalStage.Child | DevelopmentalStage.Adult) : DevelopmentalStage.Adult);
		PawnKindDef wildMan = PawnKindDefOf.WildMan;
		Faction faction = formerFaction;
		DevelopmentalStage developmentalStages = developmentalStage;
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(wildMan, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, developmentalStages));
		pawn.SetFaction(null);
		GenSpawn.Spawn(pawn, cell, map);
		string text = (pawn.DevelopmentalStage.Child() ? "FeralChild".Translate().ToString() : pawn.KindLabel);
		TaggedString taggedString = (pawn.DevelopmentalStage.Child() ? "Child".Translate() : "Person".Translate());
		TaggedString title = def.letterLabel.Formatted(text, pawn.Named("PAWN")).CapitalizeFirst();
		TaggedString text2 = def.letterText.Formatted(pawn.NameShortColored, taggedString, pawn.Named("PAWN")).AdjustedFor(pawn).CapitalizeFirst();
		PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text2, ref title, pawn);
		SendStandardLetter(title, text2, def.letterDef, parms, pawn);
		return true;
	}

	private bool TryFindEntryCell(Map map, out IntVec3 cell)
	{
		return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore, out cell);
	}

	private bool TryFindFormerFaction(out Faction formerFaction)
	{
		return Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out formerFaction, tryMedievalOrBetter: false, allowDefeated: true);
	}
}
