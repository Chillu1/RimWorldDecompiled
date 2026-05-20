namespace RimWorld.QuestGen
{
	public class QuestNode_Filter_DecreeNotPossible : QuestNode_Filter
	{
		protected override QuestPart_Filter MakeFilterQuestPart()
		{
			return new QuestPart_Filter_DecreeNotPossible();
		}
	}
}
