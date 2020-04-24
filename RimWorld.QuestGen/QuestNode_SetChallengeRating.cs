namespace RimWorld.QuestGen
{
	public class QuestNode_SetChallengeRating : QuestNode
	{
		public SlateRef<int> challengeRating;

		protected override void RunInt()
		{
			QuestGen.quest.challengeRating = challengeRating.GetValue(QuestGen.slate);
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
	}
}
