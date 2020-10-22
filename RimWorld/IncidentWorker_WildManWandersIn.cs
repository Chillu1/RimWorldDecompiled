using Verse;

namespace RimWorld
{
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
			Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.WildMan, formerFaction);
			pawn.SetFaction(null);
			GenSpawn.Spawn(pawn, cell, map);
			TaggedString title = def.letterLabel.Formatted(pawn.LabelShort, pawn.Named("PAWN")).CapitalizeFirst();
			TaggedString text = def.letterText.Formatted(pawn.NameShortColored, pawn.Named("PAWN")).AdjustedFor(pawn).CapitalizeFirst();
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, pawn);
			SendStandardLetter(title, text, def.letterDef, parms, pawn);
			return true;
		}

		private bool TryFindEntryCell(Map map, out IntVec3 cell)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore, out cell);
		}

		private bool TryFindFormerFaction(out Faction formerFaction)
		{
			return Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction_NewTemp(out formerFaction, tryMedievalOrBetter: false, allowDefeated: true);
		}
	}
}
