namespace RimWorld.QuestGen
{
	public class QuestNode_RequireRoyalFavorFromFaction : QuestNode
	{
		public SlateRef<Faction> faction;

		protected override bool TestRunInt(Slate slate)
		{
			return faction.GetValue(slate).allowRoyalFavorRewards;
		}

		protected override void RunInt()
		{
		}
	}
}
