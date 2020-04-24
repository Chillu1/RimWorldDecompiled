using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_QuestPart : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			QuestPart_SituationalThought questPart_SituationalThought = FindQuestPart(p);
			if (questPart_SituationalThought == null)
			{
				return ThoughtState.Inactive;
			}
			return ThoughtState.ActiveAtStage(questPart_SituationalThought.stage);
		}

		public QuestPart_SituationalThought FindQuestPart(Pawn p)
		{
			List<QuestPart_SituationalThought> situationalThoughtQuestParts = Find.QuestManager.SituationalThoughtQuestParts;
			for (int i = 0; i < situationalThoughtQuestParts.Count; i++)
			{
				if (situationalThoughtQuestParts[i].quest.State == QuestState.Ongoing && situationalThoughtQuestParts[i].State == QuestPartState.Enabled && situationalThoughtQuestParts[i].def == def && situationalThoughtQuestParts[i].pawn == p)
				{
					return situationalThoughtQuestParts[i];
				}
			}
			return null;
		}
	}
}
