using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_SetRoyalTitle : QuestNode
	{
		public SlateRef<Pawn> pawn;

		public SlateRef<RoyalTitleDef> royalTitle;

		public SlateRef<Faction> faction;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Pawn value = pawn.GetValue(slate);
			if (value.royalty != null)
			{
				value.royalty.SetTitle(faction.GetValue(slate), royalTitle.GetValue(slate), grantRewards: false);
			}
		}
	}
}
