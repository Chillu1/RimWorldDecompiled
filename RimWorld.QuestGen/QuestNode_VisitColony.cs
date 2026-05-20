using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_VisitColony : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<IEnumerable<Pawn>> pawns;

		public SlateRef<Faction> faction;

		public SlateRef<int?> durationTicks;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (!pawns.GetValue(slate).EnumerableNullOrEmpty())
			{
				QuestPart_VisitColony questPart_VisitColony = new QuestPart_VisitColony();
				questPart_VisitColony.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
				questPart_VisitColony.pawns.AddRange(pawns.GetValue(slate));
				questPart_VisitColony.mapParent = slate.Get<Map>("map").Parent;
				questPart_VisitColony.faction = faction.GetValue(slate);
				questPart_VisitColony.durationTicks = durationTicks.GetValue(slate);
				QuestGen.quest.AddPart(questPart_VisitColony);
			}
		}
	}
}
