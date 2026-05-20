namespace RimWorld.QuestGen
{
	public class QuestNode_RequireRoyalFavorFromFaction : QuestNode
	{
		public SlateRef<Faction> faction;

		protected override bool TestRunInt(Slate slate)
		{
			if (faction.GetValue(slate) != null)
			{
				return faction.GetValue(slate).allowRoyalFavorRewards;
			}
			return false;
		}

		protected override void RunInt()
		{
		}
	}
}
