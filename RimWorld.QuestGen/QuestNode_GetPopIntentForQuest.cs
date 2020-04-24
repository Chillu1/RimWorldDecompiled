using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetPopIntentForQuest : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override bool TestRunInt(Slate slate)
		{
			SetVars(slate);
			return true;
		}

		protected override void RunInt()
		{
			SetVars(QuestGen.slate);
		}

		private void SetVars(Slate slate)
		{
			float populationIntentForQuest = StorytellerUtilityPopulation.PopulationIntentForQuest;
			slate.Set(storeAs.GetValue(slate), populationIntentForQuest);
		}
	}
}
