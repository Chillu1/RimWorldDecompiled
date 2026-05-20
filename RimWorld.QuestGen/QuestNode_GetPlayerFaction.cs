using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetPlayerFaction : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			slate.Set(storeAs.GetValue(slate), Faction.OfPlayer);
		}

		protected override bool TestRunInt(Slate slate)
		{
			slate.Set(storeAs.GetValue(slate), Faction.OfPlayer);
			return true;
		}
	}
}
