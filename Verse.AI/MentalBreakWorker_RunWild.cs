using RimWorld;

namespace Verse.AI
{
	public class MentalBreakWorker_RunWild : MentalBreakWorker
	{
		public override bool BreakCanOccur(Pawn pawn)
		{
			if (!pawn.IsColonistPlayerControlled || pawn.Downed || !pawn.Spawned || pawn.IsQuestLodger() || !base.BreakCanOccur(pawn))
			{
				return false;
			}
			if (pawn.Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
			{
				return false;
			}
			float seasonalTemp = Find.World.tileTemperatures.GetSeasonalTemp(pawn.Map.Tile);
			if (seasonalTemp < pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin) - 7f || seasonalTemp > pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax) + 7f)
			{
				return false;
			}
			return true;
		}

		public override bool TryStart(Pawn pawn, string reason, bool causedByMood)
		{
			TrySendLetter(pawn, "LetterRunWildMentalBreak", reason);
			QuestUtility.SendQuestTargetSignals(pawn.questTags, "RanWild", pawn.Named("SUBJECT"));
			pawn.ChangeKind(PawnKindDefOf.WildMan);
			if (pawn.Faction != null)
			{
				pawn.SetFaction(null);
			}
			pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Catharsis);
			if (pawn.Spawned && !pawn.Downed)
			{
				pawn.jobs.StopAll();
			}
			return true;
		}
	}
}
