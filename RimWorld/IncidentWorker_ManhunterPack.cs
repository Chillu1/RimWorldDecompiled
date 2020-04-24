using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_ManhunterPack : IncidentWorker
	{
		private const float PointsFactor = 1f;

		private const int AnimalsStayDurationMin = 60000;

		private const int AnimalsStayDurationMax = 120000;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			IntVec3 result;
			if (ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(parms.points, map.Tile, out PawnKindDef _))
			{
				return RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Animal);
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnKindDef animalKind = parms.pawnKind;
			if ((animalKind == null && !ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(parms.points, map.Tile, out animalKind)) || ManhunterPackIncidentUtility.GetAnimalsCount(animalKind, parms.points) == 0)
			{
				return false;
			}
			IntVec3 result = parms.spawnCenter;
			if (!result.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Animal))
			{
				return false;
			}
			List<Pawn> list = ManhunterPackIncidentUtility.GenerateAnimals_NewTmp(animalKind, map.Tile, parms.points * 1f, parms.pawnCount);
			Rot4 rot = Rot4.FromAngleFlat((map.Center - result).AngleFlat);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 10);
				QuestUtility.AddQuestTag(GenSpawn.Spawn(pawn, loc, map, rot), parms.questTag);
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
				pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(60000, 120000);
			}
			SendStandardLetter("LetterLabelManhunterPackArrived".Translate(), "ManhunterPackArrived".Translate(animalKind.GetLabelPlural()), LetterDefOf.ThreatBig, parms, list[0]);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Important);
			return true;
		}
	}
}
