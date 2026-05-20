namespace RimWorld.QuestGen
{
	public class QuestNode_Filter_FactionNonPlayer : QuestNode_Filter
	{
		protected override QuestPart_Filter MakeFilterQuestPart()
		{
			return new QuestPart_Filter_FactionNonPlayer();
		}
	}
}
