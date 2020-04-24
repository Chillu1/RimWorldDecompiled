using Verse.Grammar;

namespace RimWorld
{
	public abstract class Reward
	{
		public virtual bool MakesUseOfChosenPawnSignal => false;

		public abstract void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed);

		public abstract void AddQuestPartsToGeneratingQuest(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules);

		public abstract string GetDescription(RewardsGeneratorParams parms);
	}
}
