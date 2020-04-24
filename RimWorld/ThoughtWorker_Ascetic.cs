using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Ascetic : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.IsColonist)
			{
				return false;
			}
			Room ownedRoom = p.ownership.OwnedRoom;
			if (ownedRoom == null)
			{
				return false;
			}
			int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(ownedRoom.GetStat(RoomStatDefOf.Impressiveness));
			if (def.stages[scoreStageIndex] != null)
			{
				return ThoughtState.ActiveAtStage(scoreStageIndex);
			}
			return ThoughtState.Inactive;
		}
	}
}
