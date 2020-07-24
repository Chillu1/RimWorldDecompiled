using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public abstract class Reward : IExposable
	{
		protected bool usedOrCleanedUp;

		public virtual bool MakesUseOfChosenPawnSignal => false;

		public virtual IEnumerable<GenUI.AnonymousStackElement> StackElements
		{
			get
			{
				yield break;
			}
		}

		public virtual float TotalMarketValue => 0f;

		public abstract void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed);

		public abstract IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules);

		public abstract string GetDescription(RewardsGeneratorParams parms);

		public virtual void Notify_Used()
		{
			usedOrCleanedUp = true;
		}

		public virtual void Notify_PreCleanup()
		{
			usedOrCleanedUp = true;
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref usedOrCleanedUp, "usedOrCleanedUp", defaultValue: false);
		}
	}
}
