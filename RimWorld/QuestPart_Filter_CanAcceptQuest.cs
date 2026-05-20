namespace RimWorld
{
	public class QuestPart_Filter_CanAcceptQuest : QuestPart_Filter
	{
		protected override bool Pass(SignalArgs args)
		{
			return QuestUtility.CanAcceptQuest(quest);
		}
	}
}
