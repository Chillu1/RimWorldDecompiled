using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Notify_PlayerRaidedSomeone : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<Map> getRaidersFromMap;

		public SlateRef<MapParent> getRaidersFromMapParent;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_Notify_PlayerRaidedSomeone questPart_Notify_PlayerRaidedSomeone = new QuestPart_Notify_PlayerRaidedSomeone();
			questPart_Notify_PlayerRaidedSomeone.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_Notify_PlayerRaidedSomeone.getRaidersFromMap = getRaidersFromMap.GetValue(slate);
			questPart_Notify_PlayerRaidedSomeone.getRaidersFromMapParent = getRaidersFromMapParent.GetValue(slate);
			QuestGen.quest.AddPart(questPart_Notify_PlayerRaidedSomeone);
		}
	}
}
