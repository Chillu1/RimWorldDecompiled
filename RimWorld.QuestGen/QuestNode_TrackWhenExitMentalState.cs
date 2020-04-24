using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_TrackWhenExitMentalState : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> tag;

		public SlateRef<MentalStateDef> mentalStateDef;

		[NoTranslate]
		public SlateRef<IEnumerable<string>> inSignals;

		[NoTranslate]
		public SlateRef<string> outSignal;

		protected override bool TestRunInt(Slate slate)
		{
			if (slate.Get<Map>("map") == null)
			{
				return false;
			}
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_TrackWhenExitMentalState questPart_TrackWhenExitMentalState = new QuestPart_TrackWhenExitMentalState();
			questPart_TrackWhenExitMentalState.mapParent = slate.Get<Map>("map").Parent;
			questPart_TrackWhenExitMentalState.tag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(tag.GetValue(slate));
			questPart_TrackWhenExitMentalState.mentalStateDef = mentalStateDef.GetValue(slate);
			questPart_TrackWhenExitMentalState.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignal.GetValue(slate));
			questPart_TrackWhenExitMentalState.inSignals = new List<string>();
			foreach (string item in inSignals.GetValue(slate))
			{
				questPart_TrackWhenExitMentalState.inSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item));
			}
			QuestGen.quest.AddPart(questPart_TrackWhenExitMentalState);
		}
	}
}
