using Verse;

namespace RimWorld
{
	public abstract class ThoughtWorker_RoomImpressiveness : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.story.traits.HasTrait(TraitDefOf.Ascetic))
			{
				return ThoughtState.Inactive;
			}
			Room room = p.GetRoom();
			if (room == null)
			{
				return ThoughtState.Inactive;
			}
			int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
			if (def.stages[scoreStageIndex] == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(scoreStageIndex);
		}
	}
}
