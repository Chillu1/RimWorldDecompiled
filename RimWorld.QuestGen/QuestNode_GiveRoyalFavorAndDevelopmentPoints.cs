using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GiveRoyalFavorAndDevelopmentPoints : QuestNode_GiveRoyalFavor
	{
		protected override void PostProcessRewardChoice(QuestPart_Choice rewardChoice)
		{
			if (ModsConfig.IdeologyActive && Faction.OfPlayer.ideos.FluidIdeo != null)
			{
				for (int i = 0; i < rewardChoice.choices.Count; i++)
				{
					rewardChoice.choices[i].rewards.Add(new Reward_DevelopmentPoints(QuestGen.quest));
				}
			}
		}
	}
}
