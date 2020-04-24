using Verse;

namespace RimWorld
{
	public class IncidentWorker_WandererJoin : IncidentWorker
	{
		private const float RelationWithColonistWeight = 20f;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			IntVec3 cell;
			return TryFindEntryCell(map, out cell);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindEntryCell(map, out IntVec3 cell))
			{
				return false;
			}
			Gender? fixedGender = null;
			if (def.pawnFixedGender != 0)
			{
				fixedGender = def.pawnFixedGender;
			}
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(def.pawnKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, def.pawnMustBeCapableOfViolence, 20f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, null, 1f, null, null, null, null, null, null, null, fixedGender));
			GenSpawn.Spawn(pawn, cell, map);
			TaggedString text = def.letterText.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
			TaggedString title = def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, pawn);
			SendStandardLetter(title, text, LetterDefOf.PositiveEvent, parms, pawn);
			return true;
		}

		private bool TryFindEntryCell(Map map, out IntVec3 cell)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out cell);
		}
	}
}
